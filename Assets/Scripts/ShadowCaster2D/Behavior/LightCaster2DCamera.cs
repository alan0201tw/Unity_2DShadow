using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowCaster2D
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class LightCaster2DCamera : MonoBehaviour
    {
        [SerializeField]
        private Material blendingMaterial;

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination, blendingMaterial);
        }
    }
}