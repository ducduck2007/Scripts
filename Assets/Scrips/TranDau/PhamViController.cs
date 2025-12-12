using UnityEngine;

public class PhamViController : MonoBehaviour
{
    public float blinkSpeed = 3f;
    public Color color1 = new Color(1f, 0f, 0f, 0.3f); // Màu 1
    public Color color2 = new Color(1f, 0.5f, 0.5f, 0.7f); // Màu 2
    public bool isBlinking = false;

    private Renderer[] renderers;
    private float timer = 0f;

    private void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        SetVisibility(false);
    }

    private void Update()
    {
        if (!isBlinking || renderers == null) return;

        timer += Time.deltaTime * blinkSpeed;
        float lerpValue = 0.5f + 0.5f * Mathf.Sin(timer * Mathf.PI);

        foreach (Renderer rend in renderers)
        {
            if (rend == null) continue;

            Material mat = rend.material;
            if (mat != null)
            {
                mat.color = Color.Lerp(color1, color2, lerpValue);
            }
        }
    }

    public void StartBlinking()
    {
        isBlinking = true;
        SetVisibility(true);
        timer = 0f;
    }

    public void StopBlinking()
    {
        isBlinking = false;
        SetVisibility(false);
    }

    private void SetVisibility(bool visible)
    {
        if (renderers == null) return;

        foreach (Renderer rend in renderers)
        {
            if (rend != null)
            {
                rend.enabled = visible;
            }
        }
    }
}