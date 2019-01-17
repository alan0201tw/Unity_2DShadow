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
        private static LightCaster2DCameraGPU instance;
        public static LightCaster2DCameraGPU Instance { get { return instance; } }
        
        [SerializeField]
        private Material effectMaterial;
        [SerializeField]
        private RenderTexture obstacleTexture;
        [SerializeField]
        private RenderTexture lightMap;

        private List<ShadowCaster2DGPU> shadowCasters = new List<ShadowCaster2DGPU>();

        private Material shadowMaterial;
        private CommandBuffer m_commandBuffer;

        public RenderTexture ObstacleTexture { get { return obstacleTexture; } }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        private void Start()
        {
            shadowMaterial = new Material(Shader.Find("_FatshihShader/ShadowShaderGPU"));

            Shader.SetGlobalTexture("_ObstacleTex", obstacleTexture);
        }

        private void OnPreRender()
        {
            if (m_commandBuffer == null)
            {
                m_commandBuffer = new CommandBuffer();
                GetComponent<Camera>().AddCommandBuffer(CameraEvent.AfterHaloAndLensFlares, m_commandBuffer);

                // First : Render the obstacle texture : This will be done by obstacle camera.

                // Second : Render the light map
                m_commandBuffer.SetRenderTarget(lightMap);
                m_commandBuffer.ClearRenderTarget(false, true, Color.black);
                foreach (var shadowCaster in shadowCasters)
                {
                    Matrix4x4 MVPMatrix = Matrix4x4.TRS(shadowCaster.transform.position, shadowCaster.transform.rotation, Vector3.one);

                    m_commandBuffer.DrawMesh(shadowCaster.ShadowMesh, MVPMatrix, shadowMaterial, 0, -1, shadowCaster.PropertyBlock);
                }
                // Finally : Blend the light map with the frame
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
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
                Debug.LogWarning("Trying to register duplicated shadow caster!");
            }
        }
    }
}