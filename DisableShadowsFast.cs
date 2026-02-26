using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class DisableShadowsFast : MonoBehaviour
{
    public Transform root;          // nếu null -> dùng transform của object này
    public int batchPerFrame = 200; // chia nhỏ tránh spike

    IEnumerator Start()
    {
        var rRoot = root ? root : transform;
        var rends = rRoot.GetComponentsInChildren<Renderer>(true);

        int n = 0;
        for (int i = 0; i < rends.Length; i++)
        {
            var r = rends[i];
            if (!r) continue;

            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;

            n++;
            if (n >= batchPerFrame)
            {
                n = 0;
                yield return null;
            }
        }
    }
}
