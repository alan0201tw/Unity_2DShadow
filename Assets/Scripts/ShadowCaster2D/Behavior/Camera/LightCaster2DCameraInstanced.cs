using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ShadowCaster2D.GPU
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class LightCaster2DCameraInstanced : MonoBehaviour
    {
        public static LightCaster2DCameraInstanced Instance { get; private set; }

        [SerializeField]
        [Tooltip("Put in blurring material.")]
        private Material m_effectMaterial;
        [SerializeField]
        private RenderTexture m_obstacleTexture;
        [SerializeField]
        private LayerMask m_obstacleLayer;

        private Camera m_camera;

        private bool isHavingNewShadowCaster = false;
        private List<ShadowCaster2DInstanced> shadowCasters = new List<ShadowCaster2DInstanced>();

        private Material m_shadowMaterial;
        private Material m_blurringMaterial;
        private CommandBuffer m_commandBuffer;

        private RenderTexture m_lightMapRaw;
        private RenderTexture m_lightMapFinal;

        public RenderTexture ObstacleTexture { get { return m_obstacleTexture; } }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            m_shadowMaterial = new Material(Shader.Find("_FatshihShader/ShadowShaderInstanced"));
            // enableInstancing
            m_shadowMaterial.enableInstancing = true;
            m_blurringMaterial = new Material(Shader.Find("Hidden/Blur"));

            Shader.SetGlobalTexture("_ObstacleTex", m_obstacleTexture);

            m_camera = GetComponent<Camera>();
            //m_commandBuffer = new CommandBuffer();
        }

        private void Update()
        {
            //LayerMask originalCullingMask = m_camera.cullingMask;

            //m_camera.targetTexture = ObstacleTexture;
            //m_camera.cullingMask = m_obstacleLayer;
            //m_camera.Render();

            //m_camera.targetTexture = null;
            //m_camera.cullingMask = originalCullingMask;
            //m_camera.Render();
        }

        private void OnPreRender()
        {
            // IMPORTANT! : remember to remove the old command buffer
            if (m_commandBuffer == null || isHavingNewShadowCaster)
            {
                isHavingNewShadowCaster = false;
                if(m_commandBuffer != null)
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
                name = "Raw light map"
            };
            m_lightMapFinal = new RenderTexture(Screen.width, Screen.height, 0)
            {
                name = "Final(Blurred) light map"
            };

            // Render the light map
            m_commandBuffer.SetRenderTarget(m_lightMapRaw);
            m_commandBuffer.ClearRenderTarget(false, true, Color.black);

            List<Matrix4x4> matrices = new List<Matrix4x4>(shadowCasters.Count);

            /*
            m_propertyBlock.SetVector("_CenterWorldPos", transform.position);
            m_propertyBlock.SetColor("_Color", shadowColor);
            m_propertyBlock.SetFloat("_Radius", radius);
            //m_propertyBlock.SetTexture("_ObstacleTex", m_lightCasterCamera.ObstacleTexture);
            m_propertyBlock.SetInt("_StepCount", stepCount);
            */

            MaterialPropertyBlock masterPropertyBlock = new MaterialPropertyBlock();
            List<Vector4> allCenterWorldPos = new List<Vector4>(shadowCasters.Count);
            List<Vector4> allColor = new List<Vector4>(shadowCasters.Count);
            List<float> allRadius = new List<float>(shadowCasters.Count);
            List<int> allStepCount = new List<int>(shadowCasters.Count);

            foreach (var shadowCaster in shadowCasters)
            {
                matrices.Add(Matrix4x4.TRS(shadowCaster.transform.position, shadowCaster.transform.rotation, Vector3.one));
                allCenterWorldPos.Add(shadowCaster.PropertyBlock.GetVector("_CenterWorldPos"));
                allColor.Add(shadowCaster.PropertyBlock.GetVector("_Color"));
                allRadius.Add(shadowCaster.PropertyBlock.GetFloat("_Radius"));
                allStepCount.Add(shadowCaster.PropertyBlock.GetInt("_StepCount"));
            }

            masterPropertyBlock.SetVectorArray("_CenterWorldPos", allCenterWorldPos);
            masterPropertyBlock.SetVectorArray("_Color", allColor);
            masterPropertyBlock.SetFloatArray("_Radius", allRadius);
            //masterPropertyBlock.SetVectorArray("_StepCountArray", allStepCount);
            //

            m_commandBuffer.DrawMeshInstanced(shadowCasters[0].ShadowMesh, 0, m_shadowMaterial, 0, 
                matrices.ToArray(), shadowCasters.Count, masterPropertyBlock);

            // Blurring the light map to avoid aliasing artifact
            m_commandBuffer.Blit(m_lightMapRaw, m_lightMapFinal);

            //Vector2 blurSize = new Vector2(m_lightMapFinal.texelSize.x * 2.5f, m_lightMapFinal.texelSize.y * 2.5f);
            //m_blurringMaterial.SetVector("_BlurSize", blurSize);

            //for (int i = 0; i < 4; i++)
            //{
            //    m_commandBuffer.GetTemporaryRT(i, m_lightMapFinal.width, m_lightMapFinal.height);
            //    var temp = RenderTexture.GetTemporary(m_lightMapFinal.width, m_lightMapFinal.height);
            //    m_commandBuffer.Blit(m_lightMapFinal, temp, m_blurringMaterial, 0);
            //    m_commandBuffer.Blit(temp, m_lightMapFinal, m_blurringMaterial, 1);
            //    m_commandBuffer.ReleaseTemporaryRT(i);
            //}
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            m_effectMaterial.SetTexture("_LightMap2D", m_lightMapFinal);
            Graphics.Blit(source, destination, m_effectMaterial);
        }

        public void RegisterShadowCaster(ShadowCaster2DInstanced shadowCaster2D)
        {
            if (!shadowCasters.Contains(shadowCaster2D))
            {
                isHavingNewShadowCaster = true;
                shadowCasters.Add(shadowCaster2D);
            }
            else
            {
                Debug.LogWarning("Trying to register duplicated shadow caster!", this);
            }
        }
    }
}
 