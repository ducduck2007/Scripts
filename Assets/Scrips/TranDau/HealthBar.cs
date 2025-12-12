using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image fillImage;
    public float smoothSpeed = 5f;
    private Transform target;
    private Camera cam;
    private float targetFillAmount;

    public void Init(Transform target)
    {
        this.target = target;
        cam = Camera.main;
    }

    public void SetHealth(float current, float max)
    {
        targetFillAmount = current / max;
    }

    private void LateUpdate()
    {
        if (!target) return;

        // Vị trí trên đầu nhân vật
        transform.position = target.position + Vector3.up * 2.2f;

        // Quay mặt về camera
        transform.LookAt(transform.position + cam.transform.forward);

        // Smooth fill
        fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFillAmount, Time.deltaTime * smoothSpeed);

        // Ẩn khi máu đầy
        gameObject.SetActive(fillImage.fillAmount < 0.999f);
    }
}
