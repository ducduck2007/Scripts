using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class AutoMobaOptimizer : MonoBehaviour
{
    [Header("FPS")]
    public int targetFPS = 60;
    public bool disableVSync = true;

    [Header("Animator Culling")]
    public bool cullAnimator = true;

    [Header("Update Culling")]
    public bool cullBehaviourUpdate = true;

    [Header("Disable Shadows")]
    public bool disableAllShadows = true;

    [Header("Canvas Culling")]
    public bool cullCanvas = true;

    private readonly List<CullUnit> units = new List<CullUnit>();

    void Awake()
    {
        // ===== FPS =====
        Application.targetFrameRate = targetFPS;
        if (disableVSync)
            QualitySettings.vSyncCount = 0;

#if !UNITY_EDITOR
        Debug.unityLogger.logEnabled = false;
#endif

        // ===== Shadow =====
        if (disableAllShadows)
        {
            foreach (var r in FindObjectsOfType<Renderer>())
            {
                r.shadowCastingMode = ShadowCastingMode.Off;
                r.receiveShadows = false;
            }
        }

        BuildCullUnits();
    }

    void BuildCullUnits()
    {
        foreach (var go in FindObjectsOfType<GameObject>())
        {
            var rends = go.GetComponentsInChildren<Renderer>(true);
            if (rends.Length == 0) continue;

            var unit = new CullUnit
            {
                renderers = rends
            };

            if (cullAnimator)
                unit.animators = go.GetComponentsInChildren<Animator>(true);

            if (cullBehaviourUpdate)
            {
                var list = new List<Behaviour>();
                foreach (var b in go.GetComponentsInChildren<Behaviour>(true))
                {
                    if (b is Transform) continue;
                    if (b is Renderer) continue;
                    list.Add(b);
                }
                unit.behaviours = list.ToArray();
            }

            if (cullCanvas)
                unit.canvases = go.GetComponentsInChildren<Canvas>(true);

            units.Add(unit);
        }
    }

    void LateUpdate()
    {
        for (int i = 0; i < units.Count; i++)
        {
            bool visible = false;
            var rs = units[i].renderers;
            for (int j = 0; j < rs.Length; j++)
            {
                if (rs[j] && rs[j].isVisible)
                {
                    visible = true;
                    break;
                }
            }

            units[i].Apply(visible);
        }
    }

    // ===================== INNER =====================
    class CullUnit
    {
        public Renderer[] renderers;
        public Animator[] animators;
        public Behaviour[] behaviours;
        public Canvas[] canvases;

        public void Apply(bool visible)
        {
            if (animators != null)
                for (int i = 0; i < animators.Length; i++)
                    if (animators[i]) animators[i].enabled = visible;

            if (behaviours != null)
                for (int i = 0; i < behaviours.Length; i++)
                    if (behaviours[i]) behaviours[i].enabled = visible;

            if (canvases != null)
                for (int i = 0; i < canvases.Length; i++)
                    if (canvases[i]) canvases[i].enabled = visible;
        }
    }
}
