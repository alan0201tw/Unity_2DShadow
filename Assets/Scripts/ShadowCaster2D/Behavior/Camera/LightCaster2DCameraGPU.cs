using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ShadowCaster2D.GPU
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class LightCaster2DCameraGPU : MonoBehaviour
    {
        public static LightCaster2DCameraGPU Instance { get; private set; }
        
        [SerializeField]
        private Material effectMaterial;
        [SerializeField]
        private RenderTexture obstacleTexture;

        private List<ShadowCaster2DGPU> shadowCasters = new List<ShadowCaster2DGPU>();

        private Material shadowMaterial;
        private Material blurringMaterial;
        private CommandBuffer m_commandBuffer;

        private RenderTexture lightMapRaw;
        private RenderTexture lightMapFinal;

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

            //m_commandBuffer = new CommandBuffer();
        }

        private void OnPreRender()
        {
            if (m_commandBuffer == null)
            {
                SetUpCommandBuffer();
                GetComponent<Camera>().AddCommandBuffer(CameraEvent.BeforeImageEffects, m_commandBuffer);
            }
        }

        private void SetUpCommandBuffer()
        {
            m_commandBuffer = new CommandBuffer();
            m_commandBuffer.Clear();

            lightMapRaw = new RenderTexture(Screen.width, Screen.height, 0)
            {
                name = "Raw light map"
            };
            lightMapFinal = new RenderTexture(Screen.width, Screen.height, 0)
            {
                name = "Final(Blurred) light map"
            };

            // Render the light map
            m_commandBuffer.SetRenderTarget(lightMapRaw);
            m_commandBuffer.ClearRenderTarget(false, true, Color.black);
            foreach (var shadowCaster in shadowCasters)
            {
                //Matrix4x4 MVPMatrix = Matrix4x4.TRS(shadowCaster.transform.position, shadowCaster.transform.rotation, Vector3.one);
                //m_commandBuffer.DrawMesh(shadowCaster.ShadowMesh, MVPMatrix, shadowMaterial, 0, -1, shadowCaster.m_propertyBlock);

                m_commandBuffer.DrawRenderer(shadowCaster.ShadowMeshRenderer, shadowMaterial, 0, -1);
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

        public void RegisterShadowCaster(ShadowCaster2DGPU shadowCaster2DGPU)
        {
            if (!shadowCasters.Contains(shadowCaster2DGPU))
            {
                shadowCasters.Add(shadowCaster2DGPU);
            }
            else
            {
                Debug.LogWarning("Trying to register duplicated shadow caster!", this);
            }
        }
    }
}