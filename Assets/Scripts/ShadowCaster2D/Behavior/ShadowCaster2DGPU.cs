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
        [Header("Light Caster Parameters")]
        [SerializeField]
        [Range(30, 360)]
        private int m_IndicesCount = 60;
        [SerializeField]
        [Range(50, 250)]
        private int m_StepCount = 75;
        [SerializeField]
        private float m_Radius = 1f;
        [SerializeField]
        [Range(0f, 360f)]
        private float m_Angle = 360f;
        [SerializeField]
        private Color m_ShadowColor = Color.white;

        [Header("Obstacle Parameters")]
        [SerializeField]
        private LayerMask m_ObstacleLayer = 0;

        [Header("Target Camera")]
        [SerializeField]
        private ShadowCaster2DCamera m_TargetCamera = null;
        #endregion

        private MeshFilter m_meshFilter = null;
        private Camera m_obstacleCamera = null;

        public MeshRenderer ShadowMeshRenderer { get; private set; }
        public Mesh ShadowMesh { get; private set; }
        
        public Material ShadowMaterial { get; set; }
        public RenderTexture ObstacleTexture { get; set; }
        public Camera ObstacleCamera { get { return m_obstacleCamera; } }

        private void Reset()
        {
            m_TargetCamera = FindObjectOfType<ShadowCaster2DCamera>();
        }

        private void Start()
        {
            m_TargetCamera.RegisterShadowCaster(this);
            
            m_meshFilter = GetComponent<MeshFilter>();
            ShadowMeshRenderer = GetComponent<MeshRenderer>();
            ShadowMesh = new Mesh();
            m_meshFilter.mesh = ShadowMesh;
            ShadowMesh.MarkDynamic();

            ShadowMaterial = new Material(Shader.Find("_FatshihShader/ShadowShaderGPU"));

            UpdateShadowMesh();

            // create a dummy camera object for obstacle detection
            m_obstacleCamera = gameObject.GetComponent<Camera>();
            if (m_obstacleCamera == null)
                m_obstacleCamera = gameObject.AddComponent<Camera>();
            // if you want it to be hidden in inspector, add " | HideFlags.HideInInspector "
            {
                m_obstacleCamera.hideFlags = HideFlags.HideAndDontSave;
                m_obstacleCamera.orthographic = true;
                m_obstacleCamera.orthographicSize = m_Radius;
                m_obstacleCamera.useOcclusionCulling = false;
                m_obstacleCamera.allowHDR = false;
                m_obstacleCamera.allowMSAA = false;
                m_obstacleCamera.clearFlags = CameraClearFlags.Color;
                m_obstacleCamera.backgroundColor = new Color(0, 0, 0, 0);
                m_obstacleCamera.depth = -1;

                m_obstacleCamera.cullingMask = m_ObstacleLayer;

                ObstacleTexture = new RenderTexture(
                    new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.ARGB32)
                    )
                {
                    name = "ObstacleTexture for " + name
                };

                m_obstacleCamera.targetTexture = ObstacleTexture;
            }
            ShadowMaterial.SetTexture("_ObstacleTex", ObstacleTexture);
        }

        private void Update()
        {
            ShadowMaterial.SetMatrix("_ObstacleCameraViewMatrix", ObstacleCamera.worldToCameraMatrix);
            ShadowMaterial.SetMatrix("_ObstacleCameraProjMatrix", ObstacleCamera.projectionMatrix);

            ShadowMaterial.SetVector("_CenterWorldPos", transform.position);
            ShadowMaterial.SetColor("_Color", m_ShadowColor);
            ShadowMaterial.SetFloat("_Radius", m_Radius);
            ShadowMaterial.SetInt("_StepCount", m_StepCount);

            // if the m_Radius, m_Angle field is changed, UpdateShadowMesh() need to be called to re-build mesh
            m_obstacleCamera.orthographicSize = m_Radius;
        }

        private void UpdateShadowMesh()
        {
            // Updating shadowMesh
            Vector3[] vertices = new Vector3[m_IndicesCount + 2];
            int[] indices = new int[m_IndicesCount * 3];

            /* IMPORTANT :
             *   all vertices are in local space.
             */

            Vector3 rayDirection = transform.right;
            float currentAngle = transform.eulerAngles.z;
            float angleStep = m_Angle / m_IndicesCount;
            // create a line-mesh
            vertices[0] = Vector3.zero;
            for (int i = 1; i < m_IndicesCount + 2; i++)
            {
                rayDirection.x = Mathf.Cos(currentAngle * Mathf.Deg2Rad);
                rayDirection.y = Mathf.Sin(currentAngle * Mathf.Deg2Rad);
                rayDirection.z = 0.0f;
                rayDirection.Normalize();
                rayDirection *= m_Radius;

                vertices[i] = transform.InverseTransformPoint(transform.position + rayDirection);

                currentAngle += angleStep;

                //Debug.DrawLine(transform.position, transform.TransformPoint(vertices[i]), shadowColor);

                if (i < m_IndicesCount + 1)
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