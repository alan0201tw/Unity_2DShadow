using UnityEditor;
using UnityEngine;

namespace ShadowCaster2D.CPU.CustomUnityEditor
{
    [CustomEditor(typeof(ShadowCaster2D), true)]
    public class ShadowCaster2DEditor : Editor
    {
        /// <summary>
        /// Since Unity Editor will locate the handlePosition base on the mesh, the shadow caster will have very 
        /// unstable handlePosition due to continuously updating shadowMesh. So this editor will fix this problem by
        /// providing self-defined position and rotation handle.
        /// </summary>
        private void OnSceneGUI()
        {
            ShadowCaster2D shadowCaster2D = (ShadowCaster2D)target;
            Handles.color = Color.white;
            // For a 2D scene in Unity, the vector pointing outwards from the screen is (0, 0, -1)
            Handles.DrawWireArc(shadowCaster2D.transform.position, ShadowCaster2D.Normal, shadowCaster2D.transform.right, shadowCaster2D.Angle / 2f, shadowCaster2D.Radius);
            Handles.DrawWireArc(shadowCaster2D.transform.position, ShadowCaster2D.Normal, shadowCaster2D.transform.right, shadowCaster2D.Angle / -2f, shadowCaster2D.Radius);

            Tools.hidden = true;
            switch (Tools.current)
            {
                case Tool.Move:
                    shadowCaster2D.transform.position = Handles.PositionHandle(shadowCaster2D.transform.position, shadowCaster2D.transform.rotation);
                    break;
                case Tool.Rotate:
                    shadowCaster2D.transform.rotation = Handles.RotationHandle(shadowCaster2D.transform.rotation, shadowCaster2D.transform.position);
                    break;
                default:
                    Tools.current = Tool.None;
                    break;
            }
        }

        private void OnDisable()
        {
            Tools.hidden = false;
        }
    }
}