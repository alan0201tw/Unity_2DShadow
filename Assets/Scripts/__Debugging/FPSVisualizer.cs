using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FatshihDebug
{
    [ExecuteInEditMode]
    public class FPSVisualizer : MonoBehaviour
    {
        private float frameRate;
        private float averageFPS;

        private int passedFrameCount;

        [SerializeField]
        private float FPSUpdateRate = 0.1f;
        [SerializeField]
        [Range(12,60)]
        private int m_fontSize = 12;

        private void OnGUI()
        {
            GUIStyle style = new GUIStyle
            {
                fontSize = m_fontSize
            };
            GUI.Label(new Rect(10, 10, 200, m_fontSize * 15f), "FPS : " + frameRate, style);
            GUI.Label(new Rect(10, m_fontSize * 3f, 200, m_fontSize * 15f), "averageFPS : " + averageFPS, style);
        }

        private void Start()
        {
            StartCoroutine(UpdateFPS());

            passedFrameCount = 0;
        }

        private void Update()
        {
            passedFrameCount++;

            averageFPS = passedFrameCount / Time.time;

            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                SceneManager.LoadScene(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SceneManager.LoadScene(1);
            }
        }

        private IEnumerator UpdateFPS()
        {
            while (true)
            {
                frameRate = 1 / Time.deltaTime;

                yield return new WaitForSeconds(FPSUpdateRate);
            }
        }
    }
}