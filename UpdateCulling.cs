using UnityEngine;

public class UpdateCulling : MonoBehaviour
{
    public int checkEvery = 6;

    private Behaviour[] behaviours;
    private Renderer[] rends;
    private int _frame;
    private bool _lastVisible;

    void Awake()
    {
        behaviours = GetComponentsInChildren<Behaviour>(true);
        rends = GetComponentsInChildren<Renderer>(true);
    }

    void LateUpdate()
    {
        if (++_frame < checkEvery) return;
        _frame = 0;

        bool visible = false;
        for (int i = 0; i < rends.Length; i++)
        {
            var r = rends[i];
            if (r && r.isVisible) { visible = true; break; }
        }

        if (visible == _lastVisible) return;
        _lastVisible = visible;

        for (int i = 0; i < behaviours.Length; i++)
        {
            var b = behaviours[i];
            if (!b) continue;

            // không tự tắt script này, không tắt các component “core” nếu cần
            if (b == this) continue;

            // KHÔNG khuyến nghị toggle Animator ở đây (đã có AnimatorCulling)
            if (b is Animator) continue;

            b.enabled = visible;
        }
    }
}
