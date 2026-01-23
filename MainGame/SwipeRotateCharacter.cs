using UnityEngine;

public class SwipeRotateCharacter : MonoBehaviour
{
    public float rotateSpeed = 0.3f;

    private Vector2 lastPos;

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
            lastPos = Input.mousePosition;
        else if (Input.GetMouseButton(0))
            Rotate(Input.mousePosition);

#elif UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount == 0) return;

        Touch t = Input.GetTouch(0);
        if (t.phase == TouchPhase.Began)
            lastPos = t.position;
        else if (t.phase == TouchPhase.Moved)
            Rotate(t.position);
#endif
    }

    void Rotate(Vector2 pos)
    {
        Vector2 delta = pos - lastPos;

        // Vuốt ngang → xoay Z
        float angleY = -delta.y * rotateSpeed;

        transform.Rotate(0f, angleY, 0);
        lastPos = pos;
    }
}

