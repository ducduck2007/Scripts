using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMapFollow2D : MonoBehaviour
{
    [Header("UI")]
    public RectTransform miniMapRect;

    [Header("Local Player Icon")]
    public RectTransform playerIcon;
    public Image playerIconImage;

    [Header("Teammate Icons")]
    public RectTransform teammateIconPrefab;
    public RectTransform teammatesRoot;
    public bool showTeammates = true;
    public bool excludeLocalFromTeammates = true;

    [Header("Map Bounds (World)")]
    public BoxCollider mapBounds;

    [Header("Options")]
    public bool clampInside = true;
    public bool invertX = false;
    public bool invertY = false;

    [Header("Rotate")]
    public bool rotateIconWithPlayer = true;
    public bool rotateTeammatesWithPlayer = false;

    [Header("Hero Icon (1..6)")]
    public Sprite[] heroIcons = new Sprite[6];

    [Header("Teammate Visual")]
    public bool tintTeammateByTeam = true;
    public Color team1Color = new Color(0.25f, 0.85f, 1f, 1f);
    public Color team2Color = new Color(1f, 0.35f, 0.35f, 1f);

    [Header("Towers")]
    public bool showTowers = true;
    public Color towerTeam1Color = new Color(0.2f, 0.5f, 1f, 1f);
    public Color towerTeam2Color = new Color(1f, 0.2f, 0.2f, 1f);
    public Vector2 towerIconSize = new Vector2(14f, 14f);
    public RectTransform towerIconPrefab;
    public RectTransform towersRoot;
    public Transform[] team1Towers = new Transform[10];
    public Transform[] team2Towers = new Transform[10];

    [Header("Jungle")]
    public bool showJungle = true;
    public Color jungleColor = new Color(1f, 0.9f, 0.1f, 1f);
    public Vector2 jungleIconSize = new Vector2(11f, 11f);
    public RectTransform jungleIconPrefab;
    public RectTransform jungleRoot;
    public Transform[] jungleMonsters = new Transform[10];

    private int _lastHeroType = -1;

    private readonly Dictionary<long, RectTransform> _teammateIcons = new Dictionary<long, RectTransform>(16);
    private readonly List<long> _tmpIds = new List<long>(32);
    private readonly Dictionary<long, TeammateState> _teammates = new Dictionary<long, TeammateState>(32);

    private RectTransform[] _t1Icons, _t2Icons, _jIcons;
    private Transform[] _t1Watch, _t2Watch;
    private bool[] _t1Dead, _t2Dead;

    private struct TeammateState
    {
        public Vector3 pos;
        public float yaw;
        public int heroType;
        public int teamId;
        public bool alive;
    }

    void Awake()
    {
        if (!playerIconImage && playerIcon) playerIconImage = playerIcon.GetComponent<Image>();
        if (!teammatesRoot) teammatesRoot = miniMapRect;
        if (!towersRoot) towersRoot = miniMapRect;
        if (!jungleRoot) jungleRoot = miniMapRect;
    }

    void Start()
    {
        SpawnTowers(team1Towers, towerTeam1Color, "T1", out _t1Icons, out _t1Watch, out _t1Dead);
        SpawnTowers(team2Towers, towerTeam2Color, "T2", out _t2Icons, out _t2Watch, out _t2Dead);
        SpawnJungle();
    }

    void LateUpdate()
    {
        if (!miniMapRect || !playerIcon || !mapBounds) return;
        var cf = CameraFollow.Instance;
        if (cf == null || cf.target == null) return;

        ApplyHeroIconIfNeeded();

        var target = cf.target;
        playerIcon.anchoredPosition = WorldToMiniMap(target.position);
        if (rotateIconWithPlayer) playerIcon.localRotation = Quaternion.Euler(0, 0, -target.eulerAngles.y);

        if (showTeammates) UpdateTeammates();
        else ClearTeammateIcons();

        RefreshTowers(_t1Icons, _t1Watch, _t1Dead, showTowers);
        RefreshTowers(_t2Icons, _t2Watch, _t2Dead, showTowers);

        RefreshJungle();
    }

    public void OnTowerDestroyed(int teamId, int index)
    {
        if (teamId == 1) MarkTowerDead(_t1Icons, _t1Dead, index);
        else MarkTowerDead(_t2Icons, _t2Dead, index);
    }

    public void SetTeammates(List<(long userId, Vector3 pos, float yaw, int heroType, int teamId, bool alive)> list)
    {
        _teammates.Clear();
        if (list == null) return;
        for (int i = 0; i < list.Count; i++)
        {
            var t = list[i];
            _teammates[t.userId] = new TeammateState { pos = t.pos, yaw = t.yaw, heroType = t.heroType, teamId = t.teamId, alive = t.alive };
        }
    }

    private void SpawnTowers(Transform[] src, Color color, string label, out RectTransform[] icons, out Transform[] watch, out bool[] dead)
    {
        int n = src != null ? src.Length : 0;
        icons = new RectTransform[n];
        watch = new Transform[n];
        dead = new bool[n];

        var prefab = towerIconPrefab ? towerIconPrefab : playerIcon;
        if (!prefab || !towersRoot) return;

        for (int i = 0; i < n; i++)
        {
            var s = src[i];
            if (!s) continue;

            var w = ResolveTowerWatch(s);
            watch[i] = w;

            var inst = Instantiate(prefab, towersRoot);
            inst.name = $"Tower_{label}_{i}";
            inst.sizeDelta = towerIconSize;
            inst.localScale = Vector3.one;
            inst.localRotation = Quaternion.identity;
            inst.anchoredPosition = WorldToMiniMap(s.position);

            var img = inst.GetComponent<Image>();
            if (img) { img.color = color; img.enabled = true; }

            bool alive = w && w.gameObject.activeInHierarchy;
            inst.gameObject.SetActive(showTowers && alive);
            dead[i] = !alive;
            icons[i] = inst;
        }
    }

    private void RefreshTowers(RectTransform[] icons, Transform[] watch, bool[] dead, bool show)
    {
        if (icons == null) return;

        for (int i = 0; i < icons.Length; i++)
        {
            var ic = icons[i];
            if (!ic) continue;

            bool active = false;

            if (show)
            {
                if (dead != null && i < dead.Length && dead[i]) active = false;
                else
                {
                    var w = (watch != null && i < watch.Length) ? watch[i] : null;
                    active = w && w.gameObject.activeInHierarchy;
                    if (!active && dead != null && i < dead.Length) dead[i] = true;
                }
            }

            if (ic.gameObject.activeSelf != active) ic.gameObject.SetActive(active);
        }
    }

    private void MarkTowerDead(RectTransform[] icons, bool[] dead, int index)
    {
        if (index < 0) return;
        if (dead != null && index < dead.Length) dead[index] = true;
        if (icons != null && index < icons.Length && icons[index]) icons[index].gameObject.SetActive(false);
    }

    private Transform ResolveTowerWatch(Transform root)
    {
        if (!root) return null;

        for (int i = 0; i < root.childCount; i++)
        {
            var c = root.GetChild(i);
            if (!c) continue;
            var n = c.name.ToLowerInvariant();
            if (n.Contains("chinh") || n.Contains("main")) return c;
        }

        var stack = ListPool<Transform>.Get();
        stack.Add(root);

        while (stack.Count > 0)
        {
            int last = stack.Count - 1;
            var t = stack[last];
            stack.RemoveAt(last);

            for (int i = 0; i < t.childCount; i++)
            {
                var c = t.GetChild(i);
                if (!c) continue;

                var n = c.name.ToLowerInvariant();
                if (n.Contains("truxanhchinh") || n.Contains("trudochinh") || n.Contains("chinh") || n.Contains("main"))
                {
                    ListPool<Transform>.Release(stack);
                    return c;
                }

                stack.Add(c);
            }
        }

        ListPool<Transform>.Release(stack);
        return root;
    }

    private void SpawnJungle()
    {
        int n = jungleMonsters != null ? jungleMonsters.Length : 0;
        _jIcons = new RectTransform[n];

        var prefab = jungleIconPrefab ? jungleIconPrefab : playerIcon;
        if (!prefab || !jungleRoot) return;

        for (int i = 0; i < n; i++)
        {
            var s = jungleMonsters[i];
            if (!s) continue;

            var inst = Instantiate(prefab, jungleRoot);
            inst.name = $"Jungle_{i}";
            inst.sizeDelta = jungleIconSize;
            inst.localScale = Vector3.one;
            inst.localRotation = Quaternion.identity;
            inst.anchoredPosition = WorldToMiniMap(s.position);

            var img = inst.GetComponent<Image>();
            if (img) img.color = jungleColor;

            inst.gameObject.SetActive(showJungle && s.gameObject.activeInHierarchy);
            _jIcons[i] = inst;
        }
    }

    private void RefreshJungle()
    {
        if (_jIcons == null) return;

        for (int i = 0; i < _jIcons.Length; i++)
        {
            var icon = _jIcons[i];
            if (!icon) continue;

            var src = (jungleMonsters != null && i < jungleMonsters.Length) ? jungleMonsters[i] : null;
            bool alive = showJungle && src && src.gameObject.activeInHierarchy;

            if (icon.gameObject.activeSelf != alive) icon.gameObject.SetActive(alive);
            if (alive) icon.anchoredPosition = WorldToMiniMap(src.position);
        }
    }

    private void UpdateTeammates()
    {
        if (TranDauControl.Instance != null)
        {
            _tmpIds.Clear();
            TranDauControl.Instance.GetAllPlayerUserIdsNonAlloc(_tmpIds);

            int localTeam = (B.Instance != null) ? B.Instance.teamId : 0;
            long myId = (UserData.Instance != null) ? UserData.Instance.UserID : 0;

            var filtered = ListPool<long>.Get();
            for (int i = 0; i < _tmpIds.Count; i++)
            {
                long uid = _tmpIds[i];
                if (uid <= 0) continue;
                if (excludeLocalFromTeammates && myId != 0 && uid == myId) continue;

                var obj = TranDauControl.Instance.GetPlayerByUserId(uid);
                if (!obj) continue;

                int teamId = 0;
                if (obj.TryGetComponent<PlayerOther>(out var po)) teamId = po.teamId;
                else if (obj.TryGetComponent<PlayerMove>(out _)) teamId = localTeam;

                if (localTeam != 0 && teamId != localTeam) continue;
                filtered.Add(uid);
            }

            SyncTeammateIcons(filtered);

            for (int i = 0; i < filtered.Count; i++)
            {
                long uid = filtered[i];
                var obj = TranDauControl.Instance.GetPlayerByUserId(uid);
                if (!obj) continue;

                int teamId = localTeam;
                bool alive = true;

                if (obj.TryGetComponent<PlayerOther>(out var po2))
                { teamId = po2.teamId; alive = po2.gameObject.activeInHierarchy; }

                ApplyTeammateIcon(uid, obj.transform.position, obj.transform.eulerAngles.y, GetHeroTypeForUser(uid), teamId, alive);
            }

            CleanupMissingIcons(filtered);
            ListPool<long>.Release(filtered);
            return;
        }

        int lt = (B.Instance != null) ? B.Instance.teamId : 0;
        long my = (UserData.Instance != null) ? UserData.Instance.UserID : 0;

        var ids = ListPool<long>.Get();
        foreach (var kv in _teammates)
        {
            long uid = kv.Key;
            if (excludeLocalFromTeammates && my != 0 && uid == my) continue;
            var st = kv.Value;
            if (!st.alive) continue;
            if (lt != 0 && st.teamId != lt) continue;
            ids.Add(uid);
        }

        SyncTeammateIcons(ids);

        for (int i = 0; i < ids.Count; i++)
        {
            long uid = ids[i];
            var st = _teammates[uid];
            ApplyTeammateIcon(uid, st.pos, st.yaw, st.heroType, st.teamId, st.alive);
        }

        CleanupMissingIcons(ids);
        ListPool<long>.Release(ids);
    }

    private void SyncTeammateIcons(List<long> ids)
    {
        var prefab = teammateIconPrefab ? teammateIconPrefab : playerIcon;
        if (!prefab) return;

        var parent = teammatesRoot ? teammatesRoot : miniMapRect;
        if (!parent) return;

        for (int i = 0; i < ids.Count; i++)
        {
            long uid = ids[i];
            if (_teammateIcons.ContainsKey(uid)) continue;

            var inst = Instantiate(prefab, parent);
            inst.name = $"TeammateIcon_{uid}";
            inst.localScale = Vector3.one * 0.9f;
            inst.gameObject.SetActive(true);
            _teammateIcons[uid] = inst;
        }
    }

    private void ApplyTeammateIcon(long userId, Vector3 worldPos, float yaw, int heroType, int teamId, bool alive)
    {
        if (!_teammateIcons.TryGetValue(userId, out var rt) || !rt) return;

        rt.anchoredPosition = WorldToMiniMap(worldPos);
        rt.localRotation = rotateTeammatesWithPlayer ? Quaternion.Euler(0, 0, -yaw) : Quaternion.identity;

        var img = rt.GetComponent<Image>();
        if (!img) return;

        if (heroType > 0)
        {
            var sp = GetHeroSprite(heroType);
            if (sp) img.sprite = sp;
        }

        if (tintTeammateByTeam) img.color = (teamId == 1) ? team1Color : team2Color;
        img.enabled = alive;
    }

    private void CleanupMissingIcons(List<long> existingIds)
    {
        var toRemove = ListPool<long>.Get();
        foreach (var kv in _teammateIcons)
            if (!existingIds.Contains(kv.Key)) toRemove.Add(kv.Key);

        for (int i = 0; i < toRemove.Count; i++)
        {
            long uid = toRemove[i];
            if (_teammateIcons.TryGetValue(uid, out var rt) && rt) Destroy(rt.gameObject);
            _teammateIcons.Remove(uid);
        }
        ListPool<long>.Release(toRemove);
    }

    private void ClearTeammateIcons()
    {
        foreach (var kv in _teammateIcons)
            if (kv.Value) Destroy(kv.Value.gameObject);
        _teammateIcons.Clear();
    }

    private Vector2 WorldToMiniMap(Vector3 worldPos)
    {
        var b = mapBounds.bounds;
        float dx = Mathf.Max(0.0001f, b.size.x);
        float dz = Mathf.Max(0.0001f, b.size.z);

        float nx = (worldPos.x - b.min.x) / dx;
        float ny = (worldPos.z - b.min.z) / dz;

        if (invertX) nx = 1f - nx;
        if (invertY) ny = 1f - ny;
        if (clampInside) { nx = Mathf.Clamp01(nx); ny = Mathf.Clamp01(ny); }

        return new Vector2((nx - 0.5f) * miniMapRect.rect.width, (ny - 0.5f) * miniMapRect.rect.height);
    }

    private void ApplyHeroIconIfNeeded()
    {
        if (!playerIconImage && playerIcon) playerIconImage = playerIcon.GetComponent<Image>();
        if (!playerIconImage) return;

        int heroType = GetLocalHeroType();
        if (heroType <= 0 || heroType == _lastHeroType) return;

        var sp = GetHeroSprite(heroType);
        if (sp) playerIconImage.sprite = sp;
        _lastHeroType = heroType;
    }

    private int GetLocalHeroType()
    {
        if (B.Instance == null) return 0;
        int ht = B.Instance.heroPlayer + 1;
        return (ht >= 1 && ht <= 6) ? ht : 0;
    }

    private int GetHeroTypeForUser(long userId)
    {
        if (userId <= 0) return 0;
        if (TranDauControl.HeroTypeByUserId != null &&
            TranDauControl.HeroTypeByUserId.TryGetValue(userId, out int ht) && ht > 0) return ht;

        long myId = (UserData.Instance != null) ? UserData.Instance.UserID : 0;
        return (myId != 0 && userId == myId) ? GetLocalHeroType() : 0;
    }

    private Sprite GetHeroSprite(int heroType)
    {
        int i = heroType - 1;
        if (heroIcons == null || i < 0 || i >= heroIcons.Length) return null;
        return heroIcons[i] ? heroIcons[i] : Resources.Load<Sprite>($"Sprites/circle_images/{heroType}");
    }

    private static class ListPool<T>
    {
        private static readonly Stack<List<T>> _pool = new Stack<List<T>>(8);
        public static List<T> Get() => _pool.Count > 0 ? _pool.Pop() : new List<T>(16);
        public static void Release(List<T> l) { l.Clear(); _pool.Push(l); }
    }
}
