using UnityEngine;

namespace FatshihDebug
{
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    public class RenderDepth : MonoBehaviour
    {
        [SerializeField]
        private DepthTextureMode mode;

        void OnEnable()
        {
            GetComponent<Camera>().depthTextureMode = mode;
        }
    }
}