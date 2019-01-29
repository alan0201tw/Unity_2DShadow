using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowCaster2D.GPU
{
    /// <summary>
    /// This version of 2D shadow caster requires less computation on CPU.
    /// But puts the effort on GPU.
    /// Also, this approach cannot be tuned by layer. We need to do the "tagging" in shader layer.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class ShadowCaster2DGPU : MonoBehaviour
    {
        #region Unity visible parameters
        [SerializeField]
        [Range(30, 360)]
        private int indicesCount = 60;
        [SerializeField]
        [Range(50, 250)]
        private int stepCount = 75;
        [SerializeField]
        private float radius = 1f;
        [SerializeField]
        [Range(0f, 360f)]
        private float angle = 360f;
        #endregion

        [SerializeField]
        private Color shadowColor = Color.white;

        public MaterialPropertyBlock m_propertyBlock;
        private MeshFilter m_meshFilter;

        public MeshRenderer ShadowMeshRenderer { get; private set; }
        public Mesh ShadowMesh { get; private set; }

        private void Start()
        {
            m_propertyBlock = new MaterialPropertyBlock();
            m_meshFilter = GetComponent<MeshFilter>();
            ShadowMeshRenderer = GetComponent<MeshRenderer>();
            ShadowMesh = new Mesh();
            m_meshFilter.mesh = ShadowMesh;

            UpdateShadowMesh();

            LightCaster2DCameraGPU.Instance.RegisterShadowCaster(this);
        }

        private void Update()
        {
            m_propertyBlock.SetVector("_CenterWorldPos", transform.position);
            m_propertyBlock.SetColor("_Color", shadowColor);
            m_propertyBlock.SetFloat("_Radius", radius);
            //m_propertyBlock.SetTexture("_ObstacleTex", m_lightCasterCamera.ObstacleTexture);
            m_propertyBlock.SetInt("_StepCount", stepCount);

            ShadowMeshRenderer.SetPropertyBlock(m_propertyBlock);
        }

        private void UpdateShadowMesh()
        {
            // Updating shadowMesh
            Vector3[] vertices = new Vector3[indicesCount + 2];
            int[] indices = new int[indicesCount * 3];

            /* IMPORTANT :
             *   all vertices are in local space.
             *   ...
             *   ...
             */

            Vector3 rayDirection = transform.right * radius;
            float currentAngle = transform.eulerAngles.z;
            float angleStep = angle / indicesCount;
            // create a line-mesh
            vertices[0] = Vector3.zero;
            for (int i = 1; i < indicesCount + 2; i++)
            {
                rayDirection.x = Mathf.Cos(currentAngle * Mathf.Deg2Rad);
                rayDirection.y = Mathf.Sin(currentAngle * Mathf.Deg2Rad);
                rayDirection.Normalize();
                rayDirection *= radius;

                vertices[i] = transform.InverseTransformPoint(transform.position + rayDirection);

                currentAngle += angleStep;

                //Debug.DrawLine(transform.position, transform.TransformPoint(vertices[i]), shadowColor);

                if (i < indicesCount + 1)
                {
                    indices[(i - 1) * 3] = 0;
                    indices[(i - 1) * 3 + 1] = i;
                    indices[(i - 1) * 3 + 2] = i + 1;
                }
            }
            //indices[indicesCount] = 0;

            ShadowMesh.Clear();

            ShadowMesh.vertices = vertices;
            ShadowMesh.triangles = indices;
            //m_shadowMesh.SetIndices(indices, MeshTopology.Triangles, 0);
            ShadowMesh.RecalculateNormals();
        }
    }
}