using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ShadowCaster2D.GPU
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class ShadowCaster2DCamera : MonoBehaviour
    {
        private Camera m_camera = null;
        private bool isHavingNewShadowCaster = false;
        private List<ShadowCaster2DGPU> shadowCasters = new List<ShadowCaster2DGPU>();
        
        private Material m_effectMaterial = null;
        private Material m_blurringMaterial = null;
        private CommandBuffer m_commandBuffer = null;
        
        [SerializeField]
        private RenderTexture m_lightMapRaw = null;
        [SerializeField]
        private RenderTexture m_lightMapFinal = null;

        private void Start()
        {
            m_effectMaterial = new Material(Shader.Find("_FatshihShader/LightBlending2DShader"));
            m_effectMaterial.SetVector("_Ambient", new Vector4(0.2f, 0.2f, 0.2f, 0.2f));
            m_blurringMaterial = new Material(Shader.Find("Hidden/Blur"));

            m_camera = GetComponent<Camera>();
        }

        private void OnPreRender()
        {
            if (m_commandBuffer == null || isHavingNewShadowCaster)
            {
                isHavingNewShadowCaster = false;
                if (m_commandBuffer != null)
                    m_camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, m_commandBuffer);
                SetUpCommandBuffer();
                m_camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, m_commandBuffer);
            }
        }

        private void SetUpCommandBuffer()
        {
            m_commandBuffer = new CommandBuffer();
            m_commandBuffer.Clear();

            m_lightMapRaw = new RenderTexture(Screen.width, Screen.height, 0)
            {
                name = "Raw light map",
                format = RenderTextureFormat.ARGB32,
                filterMode = FilterMode.Point,
                anisoLevel = 0
            };
            m_lightMapFinal = new RenderTexture(Screen.width, Screen.height, 0)
            {
                name = "Final(Blurred) light map",
                format = RenderTextureFormat.ARGB32,
                filterMode = FilterMode.Point,
                anisoLevel = 0
            };

            // Render the light map
            m_commandBuffer.SetRenderTarget(m_lightMapRaw);
            m_commandBuffer.ClearRenderTarget(false, true, Color.black);
            foreach (ShadowCaster2DGPU shadowCaster in shadowCasters)
            {
                //Matrix4x4 TRS = 
                //    Matrix4x4.TRS(
                //        shadowCaster.transform.position, 
                //        shadowCaster.transform.rotation, 
                //        shadowCaster.transform.localScale);

                //m_commandBuffer.DrawMesh(
                //    shadowCaster.ShadowMesh,
                //    TRS,
                //    shadowCaster.ShadowMaterial,
                //    0, 0);

                m_commandBuffer.DrawRenderer(
                    shadowCaster.ShadowMeshRenderer,
                    shadowCaster.ShadowMaterial
                    );
            }

            // Blurring the light map to avoid aliasing artifact
            m_commandBuffer.Blit(m_lightMapRaw, m_lightMapFinal);

            Vector2 blurSize = new Vector2(m_lightMapFinal.texelSize.x * 2.5f, m_lightMapFinal.texelSize.y * 2.5f);
            m_blurringMaterial.SetVector("_BlurSize", blurSize);

            for (int i = 0; i < 4; i++)
            {
                m_commandBuffer.GetTemporaryRT(i, m_lightMapFinal.width, m_lightMapFinal.height);
                var temp = RenderTexture.GetTemporary(m_lightMapFinal.width, m_lightMapFinal.height);
                m_commandBuffer.Blit(m_lightMapFinal, temp, m_blurringMaterial, 0);
                m_commandBuffer.Blit(temp, m_lightMapFinal, m_blurringMaterial, 1);
                m_commandBuffer.ReleaseTemporaryRT(i);
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            m_effectMaterial.SetTexture("_LightMap2D", m_lightMapFinal);
            Graphics.Blit(source, destination, m_effectMaterial);
        }

        public void RegisterShadowCaster(ShadowCaster2DGPU shadowCaster2DGPU)
        {
            if (!shadowCasters.Contains(shadowCaster2DGPU))
            {
                isHavingNewShadowCaster = true;
                shadowCasters.Add(shadowCaster2DGPU);
            }
            else
            {
                Debug.LogWarning("Trying to register duplicated shadow caster!", this);
            }
        }
    }
}