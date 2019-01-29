using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ShadowCaster2D.CPU
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class LightCaster2DCamera : MonoBehaviour
    {
        public static LightCaster2DCamera Instance { get; private set; }

        [SerializeField]
        private Material blendingMaterial;
        [SerializeField]
        private RenderTexture lightMap;

        private CommandBuffer m_commandBuffer;

        private Material m_shadowMaterial;

        private List<ShadowCaster2D> shadowCasters = new List<ShadowCaster2D>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            m_shadowMaterial = new Material(Shader.Find("_FatshihShader/ShadowShader"));
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

            // Render the light map
            m_commandBuffer.SetRenderTarget(lightMap);
            m_commandBuffer.ClearRenderTarget(false, true, Color.black);
            foreach (var shadowCaster in shadowCasters)
            {
                Matrix4x4 MVPMatrix = Matrix4x4.TRS(shadowCaster.transform.position, shadowCaster.transform.rotation, Vector3.one);

                m_commandBuffer.DrawMesh(shadowCaster.ShadowMesh, MVPMatrix, m_shadowMaterial, 0, -1, shadowCaster.PropertyBlock);
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination, blendingMaterial);
        }

        public void RegisterShadowCaster(ShadowCaster2D shadowCaster2D)
        {
            if (!shadowCasters.Contains(shadowCaster2D))
            {
                shadowCasters.Add(shadowCaster2D);
            }
            else
            {
                Debug.LogWarning("Trying to register duplicated shadow caster!");
            }
        }
    }
}