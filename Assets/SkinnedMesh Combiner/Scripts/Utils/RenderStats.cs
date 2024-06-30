using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace SMC
{
    /// <summary>
    /// Handle displaying various render statistics such as render time and frames per second.
    /// Frame time and draw calls ar editor code, so in a build they will not display.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RenderStats : MonoBehaviour
    {
        private const int TICK_RATE = 16; // Amount of frames elapsed before render stats updates

        [SerializeField] private Text m_StatsText;
        private int tickCount = TICK_RATE;

        private void LateUpdate()
        {
            if (!m_StatsText) { return; }

            if (tickCount >= TICK_RATE)
            {
                SkinnedMeshRenderer[] smrs = FindObjectsOfType<SkinnedMeshRenderer>(false);
                int smrCount = 0;
                if (smrs != null)
                    smrCount = smrs.Length;

                m_StatsText.text = $"Active SkinnedMeshRenderers: {smrCount}";
                m_StatsText.text += $"\nFrames Per Second: {(int)(1.0f / Time.deltaTime)} FPS";

                #if UNITY_EDITOR // Draw calls and frame time are editor only
                m_StatsText.text += $"\n\nDraw Calls: {UnityStats.drawCalls}";
                m_StatsText.text += $"\nRendering Time: {UnityStats.renderTime * 1000:0.00}ms";
                #endif

                tickCount = 0;
            }
            else
            {
                tickCount++;
            }
        }
    }
}
