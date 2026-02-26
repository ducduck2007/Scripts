using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneQualitySwitcher : MonoBehaviour
{
    [Header("Quality Names")]
    [SerializeField] private string qualityLow = "Play";  // dùng Mobile_RPAsset_Play
    [SerializeField] private string qualityHigh = "Game"; // dùng Mobile_RPAsset

    [Header("Scenes Using Low Quality")]
    [SerializeField] private string[] lowQualityScenes = { "Play", "DauTap" };

    private void Awake()
    {
        var sceneName = SceneManager.GetActiveScene().name;
        var targetQuality = IsLowScene(sceneName) ? qualityLow : qualityHigh;

        var index = System.Array.IndexOf(QualitySettings.names, targetQuality);
        // if (index < 0)
        // {
        //     Debug.LogError($"[SceneQualitySwitcher] Quality '{targetQuality}' not found");
        //     return;
        // }

        QualitySettings.SetQualityLevel(index, true);
        // Debug.Log($"[SceneQualitySwitcher] scene={sceneName} -> quality={QualitySettings.names[QualitySettings.GetQualityLevel()]}");
    }

    private bool IsLowScene(string sceneName)
    {
        for (int i = 0; i < lowQualityScenes.Length; i++)
        {
            if (lowQualityScenes[i] == sceneName) return true;
        }
        return false;
    }
}
