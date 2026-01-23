using UnityEngine;

public class MobileSkillAim : MonoBehaviour
{
    float lastY;

    public void UpdateAim(Vector2 input)
    {
        if (input.sqrMagnitude < 0.1f) return;

        float angleY = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;
        lastY = angleY;

        transform.rotation = Quaternion.Euler(0, angleY, 0);
    }

    public float GetAngleY()
    {
        return lastY;
    }
}
