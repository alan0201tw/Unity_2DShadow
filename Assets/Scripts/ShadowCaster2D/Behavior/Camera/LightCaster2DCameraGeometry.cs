using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ShadowCaster2D.Geometry
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class LightCaster2DCameraGeometry : MonoBehaviour
    {
        public static LightCaster2DCameraGeometry Instance { get; private set; }
        
        [SerializeField]
        private Material effectMaterial;
        [SerializeField]
        private RenderTexture obstacleTexture;
        [SerializeField]
        private RenderTexture lightMapRaw;
        [SerializeField]
        private RenderTexture lightMapFinal;

        private List<ShadowCaster2DGeometry> shadowCasters = new List<ShadowCaster2DGeometry>();

        private Material shadowMaterial;
        private Material blurringMaterial;
        private CommandBuffer m_commandBuffer;

        public RenderTexture ObstacleTexture { get { return obstacleTexture; } }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            shadowMaterial = new Material(Shader.Find("_FatshihShader/ShadowShaderGPU"));
            blurringMaterial = new Material(Shader.Find("Hidden/Blur"));

            Shader.SetGlobalTexture("_ObstacleTex", obstacleTexture);
        }

        private void OnPreRender()
        {
            if (m_commandBuffer == null)
            {
                lightMapRaw = new RenderTexture(Screen.width, Screen.height, 0);
                lightMapFinal = new RenderTexture(Screen.width, Screen.height, 0);

                SetUpCommandBuffer();
                GetComponent<Camera>().AddCommandBuffer(CameraEvent.BeforeImageEffects, m_commandBuffer);
            }
        }

        private void SetUpCommandBuffer()
        {
            m_commandBuffer = new CommandBuffer();

            /*
             * According to research, the output stream of a geometry shader cannot exceed 1024 bytes.
             * So I might need to use multiple vertices and separate the mesh.
             * Or I'll need to limit the ray count
             */ 

            // create a dummy mesh with only one vertex
            // all other vertices will be created in geometry shader
            Mesh dummyMesh = new Mesh
            {
                // initialize this dummy mesh with a vertex
                vertices = new Vector3[] { Vector3.zero }
            };
            dummyMesh.SetIndices(new int[] { 0 }, MeshTopology.Points, 0);

            // Render the light map
            m_commandBuffer.SetRenderTarget(lightMapRaw);
            m_commandBuffer.ClearRenderTarget(false, true, Color.black);
            foreach (var shadowCaster in shadowCasters)
            {
                Matrix4x4 MVPMatrix = Matrix4x4.TRS(shadowCaster.transform.position, shadowCaster.transform.rotation, Vector3.one);

                m_commandBuffer.DrawMesh(dummyMesh, MVPMatrix, shadowMaterial, 0, -1, shadowCaster.PropertyBlock);
            }

            // Blurring the light map to avoid aliasing artifact
            m_commandBuffer.Blit(lightMapRaw, lightMapFinal);

            Vector2 blurSize = new Vector2(lightMapFinal.texelSize.x * 2.5f, lightMapFinal.texelSize.y * 2.5f);
            blurringMaterial.SetVector("_BlurSize", blurSize);

            for (int i = 0; i < 4; i++)
            {
                m_commandBuffer.GetTemporaryRT(i, lightMapFinal.width, lightMapFinal.height);
                var temp = RenderTexture.GetTemporary(lightMapFinal.width, lightMapFinal.height);
                m_commandBuffer.Blit(lightMapFinal, temp, blurringMaterial, 0);
                m_commandBuffer.Blit(temp, lightMapFinal, blurringMaterial, 1);
                m_commandBuffer.ReleaseTemporaryRT(i);
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            effectMaterial.SetTexture("_LightMap2D", lightMapFinal);
            Graphics.Blit(source, destination, effectMaterial);
        }

        public void RegisterShadowCaster(ShadowCaster2DGeometry ShadowCaster2DGeometry)
        {
            if (!shadowCasters.Contains(ShadowCaster2DGeometry))
            {
                shadowCasters.Add(ShadowCaster2DGeometry);
            }
            else
            {
                Debug.LogWarning("Trying to register duplicated shadow caster!");
            }
        }
    }
}