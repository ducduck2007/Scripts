using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public Camera Camera;

    void LateUpdate()
    {
        // Luôn xoay theo camera nhưng khóa trục xoay không cần thiết
        Vector3 lookPos = transform.position + Camera.transform.rotation * Vector3.forward;
        Vector3 up = Camera.transform.rotation * Vector3.up;
        transform.LookAt(lookPos, up);

        // Giữ thẳng ngang (Liên Quân style)
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
    }
}
