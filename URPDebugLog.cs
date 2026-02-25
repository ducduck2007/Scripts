using UnityEngine;
using UnityEngine.Rendering;

public class URPDebugLog : MonoBehaviour
{
    private void Start()
    {
        Debug.Log($"[URPDebugLog] GraphicsSettings.currentRenderPipeline = {GraphicsSettings.currentRenderPipeline?.name}");
        Debug.Log($"[URPDebugLog] GraphicsSettings.defaultRenderPipeline = {GraphicsSettings.defaultRenderPipeline?.name}");
        Debug.Log($"[URPDebugLog] QualitySettings.renderPipeline = {QualitySettings.renderPipeline?.name}");
    }
}
