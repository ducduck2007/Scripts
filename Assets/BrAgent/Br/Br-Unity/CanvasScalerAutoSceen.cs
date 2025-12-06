using UnityEngine;
using UnityEngine.UI;

public class CanvasScalerAutoSceen : MonoBehaviour
{
    private void Awake()
    {
        float aspectRatio = AgentUnity.AspectRatioSceen();
        if (aspectRatio >= 1.99F)
        {
            CanvasScaler canvasScaler = GetComponent<CanvasScaler>();
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            canvasScaler.matchWidthOrHeight = 1F;
        }
    }
}