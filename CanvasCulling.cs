using UnityEngine;

public class CanvasCulling : MonoBehaviour
{
    Canvas c;

    void Awake() => c = GetComponent<Canvas>();

    void OnBecameVisible() => c.enabled = true;
    void OnBecameInvisible() => c.enabled = false;
}
