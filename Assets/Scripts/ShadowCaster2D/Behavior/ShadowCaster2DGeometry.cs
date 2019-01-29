using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowCaster2D.Geometry
{
    /// <summary>
    /// This version of 2D shadow caster uses geometry shader to create the shadow mesh.
    /// So we do not need to compute the mesh here, but we can pass in a dummy mesh ( a single point ) to the shader.
    /// And compute the mesh in geometry shader.
    /// </summary>
    [ExecuteInEditMode]
    public class ShadowCaster2DGeometry : MonoBehaviour
    {
        #region Unity visible parameters
        [SerializeField]
        [Range(6, 60)]
        private int indicesCount = 6;
        [SerializeField]
        [Range(50, 250)]
        private int stepCount = 60;
        [SerializeField]
        private float radius = 1f;
        [SerializeField]
        [Range(0f, 360f)]
        private float angle = 0f;
        #endregion

        [SerializeField]
        private Color shadowColor = Color.white;

        public MaterialPropertyBlock PropertyBlock { get; private set; }

        private Vector4 m_geometryParams = new Vector4();

        private void Start()
        {
            PropertyBlock = new MaterialPropertyBlock();

            LightCaster2DCameraGeometry.Instance.RegisterShadowCaster(this);
        }

        private void Update()
        {
            PropertyBlock.SetVector("_CenterWorldPos", transform.position);
            PropertyBlock.SetColor("_Color", shadowColor);
            //m_propertyBlock.SetTexture("_ObstacleTex", m_lightCasterCamera.ObstacleTexture);
            m_geometryParams.x = radius;
            m_geometryParams.y = stepCount;
            m_geometryParams.z = angle;
            m_geometryParams.w = 1;
            PropertyBlock.SetVector("_GeometryParams", m_geometryParams); // (radius, stepCount, angle, TBD)
        }
    }
}