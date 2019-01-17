using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowCaster2D
{
    /// <summary>
    /// The GameObject containing this component, should not be able to rotate around X and Y axis.
    /// </summary>
    [ExecuteInEditMode]
    public class ShadowCaster2D : MonoBehaviour
    {
        #region Static variables
        public readonly static Vector3 Normal = Vector3.back;
        private readonly static int Layer = 31;
        #endregion

        #region Unity visible parameters
        [SerializeField]
        [Tooltip("This component will fire (sampleCount + 1) rays every frame.")]
        [Range(12, 120)]
        private int sampleCount;
        [SerializeField]
        private float radius;
        [SerializeField]
        [Range(0f, 360f)]
        private float angle;
        [SerializeField]
        private LayerMask obstacleMask;

        [SerializeField]
        private Color shadowColor = Color.white;
        #endregion

        #region Graphics/Geometry variables
        private Mesh m_shadowMesh;
        private Material m_shadowMaterial;
        #endregion

        #region Artifact reducing parameters
        private readonly int edgeResolveIterations = 4;
        private readonly float edgeDstThreshold = 0.2f;
        #endregion

        #region Temprorary variables, just here to improve performance
        List<Vector3> hitPoints = new List<Vector3>();
        #endregion

        public float Radius { get { return radius; } }
        public float Angle { get { return angle; } }
        public float RayAngleInterval { get { return angle / sampleCount; } }

        private void Awake()
        {
            if (m_shadowMaterial == null)
            {
                // might be able to use custom shader in the future, with light attenuation
                m_shadowMaterial = new Material(Shader.Find("_FatshihShader/ShadowShader"));
            }

            m_shadowMesh = new Mesh();
            // mark this mesh as dynamic to improve performance
            m_shadowMesh.MarkDynamic();
        }

        private void LateUpdate()
        {
            RenderShadowMesh();
        }

        protected virtual void RenderShadowMesh()
        {
            UpdateShadowMesh();
            m_shadowMaterial.SetColor("_Color", shadowColor);
            m_shadowMaterial.SetVector("_ShadowCasterParam", new Vector4(transform.position.x, transform.position.y, transform.position.z, Radius));
            Graphics.DrawMesh(m_shadowMesh, transform.position, transform.rotation, m_shadowMaterial, Layer);
        }

        /// <summary>
        /// This method should be called every frame. It will update shadowMesh base on ray-casts.
        /// </summary>
        private void UpdateShadowMesh()
        {
            hitPoints.Clear();
            /* 
            * The middle line of the casting rays is transform.eulerAngles.z
            * So the starting direction is transform.eulerAngles.z - Angle / 2
            * And for each step, add stepAngleSize degrees to the current direction (around y axis)
            */
            float rayAngle = transform.eulerAngles.z - Angle / 2;
            RaycastHit2DPartialInfo previousViewCast = CastDetectionRay(rayAngle);
            hitPoints.Add(previousViewCast.point);

            for (int i = 1; i <= sampleCount; i++)
            {
                // DO NOT call the property RayAngleInterval, since it'll do redundant computation.
                rayAngle += angle / sampleCount;
                RaycastHit2DPartialInfo newViewCast = CastDetectionRay(rayAngle);

                /* 
                 * If we think that there are false casting.
                 * * is the hit point.
                 * 
                 *      |
                 *      |*
                 *      |
                 *   
                 *   |
                 *   |*
                 *   |
                 *   
                 * in above case, we'll connect the two hit points, which might intersects with the geometry.
                 * So we try to detect this problem here, and if we think that this problem happens, fire more rays between these
                 * two rays.
                 */
                bool edgeDstThresholdExceeded = Mathf.Abs(previousViewCast.distance - newViewCast.distance) > edgeDstThreshold;
                if (previousViewCast.isHittingCollider != newViewCast.isHittingCollider || (previousViewCast.isHittingCollider && newViewCast.isHittingCollider && edgeDstThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(previousViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero)
                    {
                        hitPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero)
                    {
                        hitPoints.Add(edge.pointB);
                    }
                }

                hitPoints.Add(newViewCast.point);
                previousViewCast = newViewCast;
            }

            // Updating shadowMesh
            int vertexCount = hitPoints.Count + 1;
            Vector3[] vertices = new Vector3[vertexCount];
            int[] triangles = new int[(vertexCount - 2) * 3];
            // assign vertices[0] to local origin, that is [0, 0, 0]
            vertices[0] = Vector3.zero;
            for (int i = 0; i < vertexCount - 1; i++)
            {
                vertices[i + 1] = transform.InverseTransformPoint(hitPoints[i]);

                if (i < vertexCount - 2)
                {
                    triangles[i * 3] = 0;
                    triangles[i * 3 + 1] = i + 1;
                    triangles[i * 3 + 2] = i + 2;
                }
            }

            m_shadowMesh.Clear();

            m_shadowMesh.vertices = vertices;
            m_shadowMesh.triangles = triangles;
            m_shadowMesh.RecalculateNormals();
        }

        private EdgeInfo FindEdge(RaycastHit2DPartialInfo minViewCast, RaycastHit2DPartialInfo maxViewCast)
        {
            float minAngle = minViewCast.angle;
            float maxAngle = maxViewCast.angle;
            Vector3 minPoint = Vector3.zero;
            Vector3 maxPoint = Vector3.zero;

            for (int i = 0; i < edgeResolveIterations; i++)
            {
                float angle = (minAngle + maxAngle) / 2;
                RaycastHit2DPartialInfo newViewCast = CastDetectionRay(angle);

                bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.distance - newViewCast.distance) > edgeDstThreshold;
                if (newViewCast.isHittingCollider == minViewCast.isHittingCollider && !edgeDstThresholdExceeded)
                {
                    minAngle = angle;
                    minPoint = newViewCast.point;
                }
                else
                {
                    maxAngle = angle;
                    maxPoint = newViewCast.point;
                }
            }

            return new EdgeInfo(minPoint, maxPoint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rayAngle">The angle between this ray and the starting ray.</param>
        /// <returns></returns>
        private RaycastHit2DPartialInfo CastDetectionRay(float rayAngle)
        {
            Vector3 direction = GetDirectionByAngle(rayAngle, true);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, Radius, obstacleMask);
            // hits something
            if (hit.collider != null)
            {
                return new RaycastHit2DPartialInfo(true, hit.point, hit.distance, rayAngle);
            }
            return new RaycastHit2DPartialInfo(false, transform.position + direction * Radius, Radius, rayAngle);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="angleInDegrees">The input angle(in degrees).</param>
        /// <param name="isWorldSpace">Is the returned direction vector in world space or local space?</param>
        /// <returns>The vector that represents the direction rotates "angleInDegrees" degrees around transform.right</returns>
        private Vector3 GetDirectionByAngle(float angleInDegrees, bool isWorldSpace)
        {
            if (!isWorldSpace)
            {
                angleInDegrees += transform.eulerAngles.z;
            }
            // Here we assume the direction is started by (1, 0, 0), user can rotate the transform to change the
            // corresponding starting direction in world space. But in local space, it's always (1, 0, 0).
            return new Vector3(Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0);
        }
    }
}