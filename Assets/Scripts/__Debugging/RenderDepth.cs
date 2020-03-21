using UnityEngine;

namespace FatshihDebug
{
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    public class RenderDepth : MonoBehaviour
    {
        [SerializeField]
        private DepthTextureMode mode = DepthTextureMode.Depth;

        void OnEnable()
        {
            GetComponent<Camera>().depthTextureMode = mode;
        }
    }
}