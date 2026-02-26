using UnityEngine;

public class AnimatorCulling : MonoBehaviour
{
    private Animator anim;
    private Renderer[] rends;
    private bool _lastVisible;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>(true);
        rends = GetComponentsInChildren<Renderer>(true);
        if (anim != null) anim.cullingMode = AnimatorCullingMode.CullCompletely;
    }

    void Update()
    {
        if (!anim) return;

        bool visible = false;
        for (int i = 0; i < rends.Length; i++)
        {
            var r = rends[i];
            if (r && r.isVisible) { visible = true; break; }
        }

        if (visible == _lastVisible) return;
        _lastVisible = visible;

        // Không toggle anim.enabled liên tục
        anim.cullingMode = visible ? AnimatorCullingMode.AlwaysAnimate
                                   : AnimatorCullingMode.CullCompletely;
    }
}
