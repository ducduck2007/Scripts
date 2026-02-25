using System.Collections.Generic;
using UnityEngine;

public class TranDauControl : ManualSingleton<TranDauControl>
{
    public static readonly Dictionary<long, int> HeroTypeByUserId = new Dictionary<long, int>(128);

    public static void CacheHeroType(long userId, int heroType)
    {
        if (userId <= 0 || heroType <= 0) return;
        HeroTypeByUserId[userId] = heroType;
    }

    public CameraFollow cameraF;
    public PlayerMove[] playerMoves;
    public TruLinh[] truLinhs;

    public PlayerMove playerMove => playerMoves[B.Instance.heroPlayer];

    [System.Serializable]
    public class MonsterPrefabEntry
    {
        public int campId;
        public JungleMonster prefab;
        public float spawnDelaySeconds;
    }

    [Header("Monsters (Prefab-based)")]
    public Transform monstersContainer;
    public List<MonsterPrefabEntry> monsterPrefabs = new List<MonsterPrefabEntry>(16);

    private readonly Dictionary<int, JungleMonster> _activeMonstersById = new Dictionary<int, JungleMonster>(32);
    private readonly Dictionary<int, MonsterPrefabEntry> _monsterPrefabMap = new Dictionary<int, MonsterPrefabEntry>(32);
    private bool _monsterPrefabMapBuilt = false;

    private struct PendingMonsterState
    {
        public int id;
        public int campId;
        public float x;
        public float y;
        public int hp;
        public int hpMax;
        public bool hasPos;
        public bool hasHp;
    }

    private readonly Dictionary<int, PendingMonsterState> _pendingMonsterState = new Dictionary<int, PendingMonsterState>(32);

    private readonly Dictionary<int, float> _spawnAtTime = new Dictionary<int, float>(32);
    private readonly List<int> _spawnOrder = new List<int>(32);

    private void BuildMonsterPrefabMapIfNeeded()
    {
        if (_monsterPrefabMapBuilt) return;
        _monsterPrefabMapBuilt = true;
        _monsterPrefabMap.Clear();

        for (int i = 0; i < monsterPrefabs.Count; i++)
        {
            var e = monsterPrefabs[i];
            if (e == null || e.prefab == null) continue;
            _monsterPrefabMap[e.campId] = e;
        }
    }

    private float GetSpawnDelayForMonster(int campId)
    {
        BuildMonsterPrefabMapIfNeeded();
        if (_monsterPrefabMap.TryGetValue(campId, out var e) && e != null)
            return Mathf.Max(0f, e.spawnDelaySeconds);
        return 0f;
    }

    private JungleMonster GetPrefabForMonster(int campId)
    {
        BuildMonsterPrefabMapIfNeeded();
        if (_monsterPrefabMap.TryGetValue(campId, out var e) && e != null && e.prefab != null)
            return e.prefab;
        return null;
    }

    private void ScheduleSpawnIfNeeded(int monsterId, int campId)
    {
        if (monsterId <= 0) return;
        if (_activeMonstersById.ContainsKey(monsterId)) return;
        if (_spawnAtTime.ContainsKey(monsterId)) return;

        float delay = GetSpawnDelayForMonster(campId);
        _spawnAtTime[monsterId] = Time.time + delay;
        _spawnOrder.Add(monsterId);

        if (!_pendingMonsterState.TryGetValue(monsterId, out var st))
            st = new PendingMonsterState { id = monsterId };

        st.campId = campId;
        _pendingMonsterState[monsterId] = st;
    }

    private void TrySpawnDueMonsters()
    {
        if (_spawnOrder.Count == 0) return;
        float now = Time.time;

        for (int i = _spawnOrder.Count - 1; i >= 0; i--)
        {
            int id = _spawnOrder[i];

            if (!_spawnAtTime.TryGetValue(id, out float at))
            {
                _spawnOrder.RemoveAt(i);
                continue;
            }

            if (now < at) continue;

            _spawnAtTime.Remove(id);
            _spawnOrder.RemoveAt(i);

            if (_activeMonstersById.ContainsKey(id))
                continue;

            if (!_pendingMonsterState.TryGetValue(id, out var st))
            {
                Debug.LogError($"[TranDauControl] No pending state for monsterId={id}");
                continue;
            }

            var prefab = GetPrefabForMonster(st.campId);
            if (prefab == null)
            {
                var keys = string.Join(", ", _monsterPrefabMap.Keys);
                Debug.LogError($"[TranDauControl] Missing prefab for campId={st.campId} (monsterId={id}). Registered campIds: [{keys}]");
                continue;
            }

            Vector3 pos = st.hasPos ? new Vector3(st.x, 0f, st.y) : Vector3.zero;
            var m = Instantiate(prefab, pos, Quaternion.identity);

            if (monstersContainer != null)
                m.transform.SetParent(monstersContainer, true);

            m.id = id;
            m.campId = st.campId;

            if (st.hasHp || st.hasPos)
            {
                m.UpdateFromServer(
                    st.hasPos ? st.x : m.transform.position.x,
                    st.hasPos ? st.y : m.transform.position.z,
                    st.hasHp ? st.hp : 0,
                    st.hasHp ? st.hpMax : 0);
            }

            _activeMonstersById[id] = m;
        }
    }

    public GameObject minionPrefab;
    public Transform minionContainer;

    private readonly Dictionary<long, MinionMove> activeMinions = new Dictionary<long, MinionMove>(64);
    private readonly Dictionary<long, GameObject> playersByUserId = new Dictionary<long, GameObject>(128);

    public GameObject GetPlayerByUserId(long userId)
    {
        playersByUserId.TryGetValue(userId, out var obj);
        return obj;
    }

    [System.Serializable]
    public class HeroPrefabEntry
    {
        public int heroType;
        public PlayerOther prefab;
    }

    public List<HeroPrefabEntry> heroPrefabs = new List<HeroPrefabEntry>(8);
    public Transform othersContainer;

    private readonly Dictionary<int, PlayerOther> heroPrefabMap = new Dictionary<int, PlayerOther>(16);
    private readonly List<int> _availableHeroTypes = new List<int>(16);
    private bool _heroPrefabMapBuilt;

    private readonly Dictionary<long, PlayerOther> othersByUserId = new Dictionary<long, PlayerOther>(64);
    private readonly HashSet<long> seenOtherIds = new HashSet<long>(64);
    private readonly List<long> toRemoveOthers = new List<long>(64);
    private readonly Dictionary<long, float> _lastSeenOtherTime = new Dictionary<long, float>(64);

    [SerializeField] private float otherMissingGraceSeconds = 0.8f;

    private readonly Dictionary<long, TruLinh> _turretById = new Dictionary<long, TruLinh>(32);
    private bool _turretMapBuilt = false;
    private bool _reconciledTurretsOnce = false;

    private void BuildHeroPrefabMapIfNeeded()
    {
        if (_heroPrefabMapBuilt) return;
        _heroPrefabMapBuilt = true;

        heroPrefabMap.Clear();
        _availableHeroTypes.Clear();

        for (int i = 0; i < heroPrefabs.Count; i++)
        {
            var e = heroPrefabs[i];
            if (e == null || e.prefab == null || e.heroType <= 0) continue;

            heroPrefabMap[e.heroType] = e.prefab;

            if (!_availableHeroTypes.Contains(e.heroType))
                _availableHeroTypes.Add(e.heroType);
        }

        if (_availableHeroTypes.Count == 0)
            Debug.LogError("[TranDauControl] heroPrefabs list is empty or invalid.");
    }

    public int ResolveAndCacheHeroType(long userId, int teamId, int snapshotHeroType)
    {
        if (snapshotHeroType > 0)
        {
            CacheHeroType(userId, snapshotHeroType);
            return snapshotHeroType;
        }

        if (HeroTypeByUserId.TryGetValue(userId, out var cached) && cached > 0)
            return cached;

        long myId = UserData.Instance != null ? UserData.Instance.UserID : 0;

        if (myId != 0 && userId == myId)
        {
            int myHeroType = (B.Instance != null ? B.Instance.heroPlayer + 1 : 0);
            if (myHeroType > 0)
            {
                CacheHeroType(userId, myHeroType);
                return myHeroType;
            }
        }

        BuildHeroPrefabMapIfNeeded();
        if (_availableHeroTypes.Count == 0)
            return 0;

        unchecked
        {
            long x = userId ^ (userId >> 32);
            int h = (int)x;
            h = (h * 16777619) ^ (teamId * 374761393);
            if (h == int.MinValue) h = 0;
            int idx = Mathf.Abs(h) % _availableHeroTypes.Count;
            int ht = _availableHeroTypes[idx];
            CacheHeroType(userId, ht);
            return ht;
        }
    }

    private void RegisterPlayer(long userId, GameObject obj)
    {
        if (userId <= 0 || obj == null) return;
        playersByUserId[userId] = obj;
    }

    private readonly List<Transform> cachedPlayerTargets = new List<Transform>(128);
    private float refreshCacheTimer;
    private const float CACHE_REFRESH_INTERVAL = 1f;

    private bool _localSetupDone;
    private readonly Stack<MinionMove> _minionPool = new Stack<MinionMove>(64);

    [System.Serializable]
    public struct SkillCastInfo
    {
        public long attackerId;
        public int skillId;
        public int typeSkill;
        public int projectileId;

        public Vector3 origin;
        public Vector3 dir;

        public bool hasTarget;
        public Vector3 targetPos;

        public float speed;
        public float radius;
        public float maxRange;

        public int angle;
    }

    public void Start()
    {
        _reconciledTurretsOnce = false;
        playersByUserId.Clear();
        minionMoves.Clear();
        activeMinions.Clear();
        BuildTurretMapIfNeeded();

        _localSetupDone = false;

        _activeMonstersById.Clear();
        _pendingMonsterState.Clear();
        _spawnAtTime.Clear();
        _spawnOrder.Clear();
        _monsterPrefabMapBuilt = false;

        for (int i = 0; i < playerMoves.Length; i++)
        {
            if (i == B.Instance.heroPlayer)
            {
                playerMoves[i].gameObject.SetActive(true);
                if (playerMoves[i].HealthBar != null)
                    playerMoves[i].HealthBar.gameObject.SetActive(true);
                if (playerMoves[i].controller != null)
                    playerMoves[i].controller.enabled = true;
            }
            else
            {
                if (playerMoves[i].controller != null)
                    playerMoves[i].controller.enabled = false;
                playerMoves[i].gameObject.SetActive(false);
            }
        }

        foreach (var kv in othersByUserId)
        {
            if (kv.Value == null) continue;
            kv.Value.gameObject.SetActive(false);
            if (kv.Value.HealthBar != null)
                kv.Value.HealthBar.gameObject.SetActive(false);
        }

        cameraF.SetTarget(playerMove.transform);
        playerMove.SetPotion();

        RefreshTargetCache();

        PlayLoadGate.MarkReady();
        PlayLoadGate.FlushTo(this);
        MatchStartGate.TryHideLoading();
    }

    private void Update()
    {
        TrySpawnDueMonsters();

        refreshCacheTimer += Time.deltaTime;
        if (refreshCacheTimer >= CACHE_REFRESH_INTERVAL)
        {
            refreshCacheTimer = 0f;
            RefreshTargetCache();
        }
    }

    private void RefreshTargetCache()
    {
        cachedPlayerTargets.Clear();

        for (int i = 0; i < playerMoves.Length; i++)
        {
            var pm = playerMoves[i];
            if (pm != null && pm.gameObject.activeSelf)
                cachedPlayerTargets.Add(pm.transform);
        }

        foreach (var kv in othersByUserId)
        {
            var po = kv.Value;
            if (po != null && po.gameObject.activeSelf)
                cachedPlayerTargets.Add(po.transform);
        }
    }

    public Transform FindNearestEnemy(Vector3 position, float range, int myTeamId)
    {
        Transform nearest = null;
        float minDist = range * range;

        for (int i = 0; i < cachedPlayerTargets.Count; i++)
        {
            var t = cachedPlayerTargets[i];
            if (t == null || !t.gameObject.activeSelf) continue;

            if (t.TryGetComponent<PlayerMove>(out var pm))
            {
                if (myTeamId != 0 && B.Instance.teamId == myTeamId)
                    continue;
            }

            if (t.TryGetComponent<PlayerOther>(out var po))
            {
                if (myTeamId != 0 && po.teamId == myTeamId)
                    continue;
            }

            float sqr = (t.position - position).sqrMagnitude;
            if (sqr < minDist)
            {
                minDist = sqr;
                nearest = t;
            }
        }

        return nearest;
    }

    public void Init(List<PlayerOutPutSv> playersData)
    {
        if (playersData == null || playersData.Count == 0) return;
        if (playerMove == null || UserData.Instance == null) return;

        long myId = UserData.Instance.UserID;

        PlayerOutPutSv myData = null;
        for (int i = 0; i < playersData.Count; i++)
        {
            var p = playersData[i];
            if (p.userId == myId)
            {
                myData = p;
                break;
            }
        }

        if (myData != null)
        {
            playerMove.ApplyServerData(myData);

            if (!_localSetupDone)
            {
                RegisterPlayer(myData.userId, playerMove.gameObject);
                SetupLocalTeamLayersOnce(myData.teamId);
                _localSetupDone = true;
            }
        }

        seenOtherIds.Clear();

        float now = Time.time;

        for (int i = 0; i < playersData.Count; i++)
        {
            var p = playersData[i];
            if (myData != null && p.userId == myId)
                continue;

            seenOtherIds.Add(p.userId);
            _lastSeenOtherTime[p.userId] = now;

            int resolvedHeroType = ResolveAndCacheHeroType(p.userId, p.teamId, p.heroType);
            var po = GetOrCreateOther(p.userId, resolvedHeroType);

            if (po == null)
            {
                Debug.LogError($"[Init] Failed to create PlayerOther uid={p.userId}");
                continue;
            }

            po.gameObject.SetActive(true);
            if (po.HealthBar != null)
                po.HealthBar.gameObject.SetActive(true);

            po.SetTeamId(p.teamId);

            int layer = LayerMask.NameToLayer(p.teamId == 1 ? "player1" : "player2");
            po.gameObject.layer = layer;

            po.ApplyServerData(new PlayerOutPutSv
            {
                userId = p.userId,
                displayName = p.displayName,
                teamId = p.teamId,
                x = p.x,
                y = p.y,
                heading = p.heading,
                speed = p.speed,
                isMoving = p.isMoving,
                isAlive = p.isAlive,
                heroType = resolvedHeroType,
                hp = p.hp,
                maxHp = p.maxHp
            });

            RegisterPlayer(p.userId, po.gameObject);
        }

        toRemoveOthers.Clear();
        foreach (var kv in othersByUserId)
        {
            long uid = kv.Key;
            var po = kv.Value;

            if (po == null)
            {
                toRemoveOthers.Add(uid);
                continue;
            }

            if (!seenOtherIds.Contains(uid))
            {
                _lastSeenOtherTime.TryGetValue(uid, out float lastSeen);
                float missingFor = now - lastSeen;

                if (missingFor >= otherMissingGraceSeconds)
                {
                    po.gameObject.SetActive(false);
                    if (po.HealthBar != null)
                        po.HealthBar.gameObject.SetActive(false);
                }
            }
        }

        for (int i = 0; i < toRemoveOthers.Count; i++)
        {
            long uid = toRemoveOthers[i];
            othersByUserId.Remove(uid);
            playersByUserId.Remove(uid);
            _lastSeenOtherTime.Remove(uid);
        }

        RefreshTargetCache();

        if (_localSetupDone && seenOtherIds.Count > 0)
            MatchStartGate.MarkBothPlayersReady();
    }

    private void SetupLocalTeamLayersOnce(int myTeamId)
    {
        if (playerMove == null || playerMove.controller == null) return;

        if (myTeamId == 1)
        {
            int layer1 = LayerMask.NameToLayer("player1");
            playerMove.controller.gameObject.layer = layer1;

            int layer2Mask = LayerMask.GetMask("player2");
            playerMove.enemyLayer = layer2Mask;
        }
        else
        {
            int layer2 = LayerMask.NameToLayer("player2");
            playerMove.controller.gameObject.layer = layer2;

            int layer1Mask = LayerMask.GetMask("player1");
            playerMove.enemyLayer = layer1Mask;
        }
    }

    public void InitMonster(List<JungleMonsterOutPutSv> monstersData)
    {
        if (monstersData == null || monstersData.Count == 0) return;

        for (int i = 0; i < monstersData.Count; i++)
        {
            var mdata = monstersData[i];
            if (mdata == null || mdata.id <= 0) continue;

            _pendingMonsterState.TryGetValue(mdata.id, out var st);
            st.id = mdata.id;
            st.campId = mdata.campId;
            st.x = mdata.x;
            st.y = mdata.y;
            st.hasPos = true;

            if (mdata.hpMax > 0)
            {
                st.hp = mdata.hp;
                st.hpMax = mdata.hpMax;
                st.hasHp = true;
            }
            _pendingMonsterState[mdata.id] = st;

            if (_activeMonstersById.TryGetValue(mdata.id, out var existing) && existing != null)
            {
                existing.campId = mdata.campId;
                existing.UpdateFromServer(mdata.x, mdata.y, mdata.hp, mdata.hpMax);
                continue;
            }

            SpawnMonsterImmediate(st);
        }
    }

    private void SpawnMonsterImmediate(PendingMonsterState st)
    {
        var prefab = GetPrefabForMonster(st.campId);
        if (prefab == null)
        {
            var keys = string.Join(", ", _monsterPrefabMap.Keys);
            Debug.LogError($"[TranDauControl] Missing prefab for campId={st.campId} (monsterId={st.id}). Registered campIds: [{keys}]");
            return;
        }

        Vector3 pos = st.hasPos ? new Vector3(st.x, 0f, st.y) : Vector3.zero;
        var m = Instantiate(prefab, pos, Quaternion.identity);

        if (monstersContainer != null)
            m.transform.SetParent(monstersContainer, true);

        m.id = st.id;
        m.campId = st.campId;

        m.UpdateFromServer(
            st.hasPos ? st.x : m.transform.position.x,
            st.hasPos ? st.y : m.transform.position.z,
            st.hasHp ? st.hp : 1,
            st.hasHp ? st.hpMax : 1
        );

        _activeMonstersById[st.id] = m;

        _spawnAtTime.Remove(st.id);
        _spawnOrder.Remove(st.id);
    }

    public void UpdateMonsterResource(int id, int campId, int hp, int maxHp)
    {
        if (id <= 0) return;

        if (_activeMonstersById.TryGetValue(id, out var m) && m != null)
        {
            float x = m.transform.position.x;
            float y = m.transform.position.z;
            m.campId = campId;
            m.UpdateFromServer(x, y, hp, maxHp);
            return;
        }

        _pendingMonsterState.TryGetValue(id, out var st);
        st.id = id;
        st.campId = campId;
        if (maxHp > 0)
        {
            st.hp = hp;
            st.hpMax = maxHp;
            st.hasHp = true;
        }
        _pendingMonsterState[id] = st;

        ScheduleSpawnIfNeeded(id, campId);
    }

    public void UpdateMonsterResourceWithPosition(int id, int campId, int hp, int maxHp, float x, float y)
    {
        if (id <= 0) return;

        if (_activeMonstersById.TryGetValue(id, out var m) && m != null)
        {
            m.campId = campId;
            m.UpdateFromServer(x, y, hp, maxHp);
            return;
        }

        _pendingMonsterState.TryGetValue(id, out var st);
        st.id = id;
        st.campId = campId;
        st.x = x;
        st.y = y;
        st.hasPos = true;
        if (maxHp > 0)
        {
            st.hp = hp;
            st.hpMax = maxHp;
            st.hasHp = true;
        }
        _pendingMonsterState[id] = st;

        ScheduleSpawnIfNeeded(id, campId);
    }

    public void MonterDeath(long id)
    {
        int mid = (int)id;
        if (mid <= 0) return;

        if (_activeMonstersById.TryGetValue(mid, out var m) && m != null)
            m.Die();
    }

    public void SetAttackState(long userId, bool isAttack, bool hasTarget)
        => OnUdpAttackState(userId, isAttack, hasTarget);

    public void SetCastSkillState(long userId, int skillId, bool hasTarget)
        => OnUdpCastSkillState(userId, skillId, hasTarget);

    public void PlayerDeath(long victimUserId)
        => OnUdpPlayerDeath(victimUserId);

    public void PlayerRespawn(long userId, int unityX, int unityY, int hp)
    {
        float x = unityX;
        float y = unityY;

        if (UserData.Instance != null && userId == UserData.Instance.UserID)
        {
            if (playerMove != null)
            {
                B.Instance.PosX = x;
                B.Instance.PosZ = y;
                playerMove.onRespawn(hp);
            }

            if (CanvasController.Instance != null)
            {
                var spawn = FindObjectOfType<CanvasSpawn>(true);
                if (spawn != null) spawn.StopCountdown();
                CanvasController.Instance.HideSpawnCanvas();
            }

            return;
        }

        var obj = GetPlayerByUserId(userId);
        if (obj != null && obj.TryGetComponent<PlayerOther>(out var po))
        {
            po.onRespawn(x, y, hp);
        }
    }

    public void OnUdpCmd50_SyncPlayers(List<PlayerOutPutSv> playersData)
    {
        Init(playersData);
    }

    public void OnUdpAttackState(long userId, bool isAttack, bool hasTarget)
    {
        if (UserData.Instance != null && userId == UserData.Instance.UserID)
            return;

        var obj = GetPlayerByUserId(userId);
        if (obj != null && obj.TryGetComponent<PlayerOther>(out var po))
        {
            po.SetAttackState(isAttack, hasTarget);
        }
    }

    public void OnUdpCastSkillState(long userId, int skillId, bool hasTarget)
    {
        if (UserData.Instance != null && userId == UserData.Instance.UserID)
            return;

        var obj = GetPlayerByUserId(userId);
        if (obj != null && obj.TryGetComponent<PlayerOther>(out var po))
        {
            if (skillId == 1) po.CastSkillFromServer(1, hasTarget);
            else if (skillId == 2) po.CastSkillFromServer(2, hasTarget);
            else if (skillId == 3) po.CastSkillFromServer(3, hasTarget);
        }
    }

    public void OnUdpPlayerDeath(long victimUserId)
    {
        if (UserData.Instance != null && victimUserId == UserData.Instance.UserID)
        {
            if (playerMove != null)
                playerMove.onDeath();
            return;
        }

        var obj = GetPlayerByUserId(victimUserId);
        if (obj != null && obj.TryGetComponent<PlayerOther>(out var po))
            po.onDeath();
    }

    public void OnUdpPlayerRespawn(long userId, int x, int y, int hp)
    {
        float scaledX = x / 2f;
        float scaledY = y / 2f;

        if (UserData.Instance != null && userId == UserData.Instance.UserID)
        {
            if (playerMove != null)
            {
                B.Instance.PosX = scaledX;
                B.Instance.PosZ = scaledY;
                playerMove.onRespawn(hp);
            }

            if (CanvasController.Instance != null)
            {
                var spawn = FindObjectOfType<CanvasSpawn>(true);
                if (spawn != null) spawn.StopCountdown();
                CanvasController.Instance.HideSpawnCanvas();
            }

            return;
        }

        var obj = GetPlayerByUserId(userId);
        if (obj != null && obj.TryGetComponent<PlayerOther>(out var po))
            po.onRespawn(scaledX, scaledY, hp);
    }

    private readonly List<long> _tmpRemoveMinionIds = new List<long>(64);
    private readonly List<MinionMove> minionMoves = new List<MinionMove>(64);

    public void InitMinions(List<MinionOutPutSv> minionsData)
    {
        if (minionsData == null || minionPrefab == null) return;

        for (int i = 0; i < minionsData.Count; i++)
        {
            var mdata = minionsData[i];

            if (activeMinions.TryGetValue(mdata.id, out var existing) && existing != null)
            {
                existing.ApplySnapshot(mdata.x, mdata.y, mdata.isAttack);
            }
            else
            {
                SpawnMinion(mdata);
            }
        }

        _tmpRemoveMinionIds.Clear();
        foreach (var kvp in activeMinions)
        {
            bool stillExists = false;
            for (int i = 0; i < minionsData.Count; i++)
            {
                if (minionsData[i].id == kvp.Key)
                {
                    stillExists = true;
                    break;
                }
            }
            if (!stillExists)
                _tmpRemoveMinionIds.Add(kvp.Key);
        }

        for (int i = 0; i < _tmpRemoveMinionIds.Count; i++)
        {
            var id = _tmpRemoveMinionIds[i];
            if (activeMinions.TryGetValue(id, out var m) && m != null)
                ReturnMinionToPool(m);

            activeMinions.Remove(id);

            for (int k = minionMoves.Count - 1; k >= 0; k--)
            {
                if (minionMoves[k] == null || minionMoves[k].minionId == id)
                    minionMoves.RemoveAt(k);
            }
        }
    }

    private void SpawnMinion(MinionOutPutSv data)
    {
        Vector3 spawnPos = new Vector3(data.x, 0, data.y);

        MinionMove minion = GetMinionFromPool();
        GameObject obj;

        if (minion != null)
        {
            obj = minion.gameObject;
            obj.transform.position = spawnPos;
            obj.transform.rotation = Quaternion.identity;
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(minionPrefab, spawnPos, Quaternion.identity);
            minion = obj.GetComponent<MinionMove>();
            if (minion == null) minion = obj.AddComponent<MinionMove>();
        }

        if (minionContainer != null)
            obj.transform.SetParent(minionContainer, true);

        minion.SetData(data.id, data.teamId, data.laneId);
        minion.ApplySnapshot(data.x, data.y, data.isAttack);

        activeMinions[data.id] = minion;
        minionMoves.Add(minion);
    }

    public void MinionDeath(long id)
    {
        for (int i = 0; i < minionMoves.Count; i++)
        {
            if (minionMoves[i].minionId == id)
            {
                minionMoves[i].OnDeath();
                minionMoves.RemoveAt(i);
                return;
            }
        }
    }

    public void TruLinhDeath(int id)
    {
        var t = GetTru(id);
        if (t == null)
            return;

        if (t.maxHP > 0)
            t.UpdateHP(0, t.maxHP);

        if (t.IsMainTurret)
            return;
    }

    public void PutTruBan(long idtru, long idTarget, int typeTarget, int team)
    {
        Transform tr = null;

        if (typeTarget == 0)
        {
            if (UserData.Instance != null && idTarget == UserData.Instance.UserID)
                tr = playerMove?.transform;
            else
            {
                var obj = GetPlayerByUserId(idTarget);
                tr = obj != null ? obj.transform : null;
            }
        }
        else if (typeTarget == 1)
        {
            var linh = GetLinh(idTarget);
            if (linh != null)
                tr = linh.transform;
        }

        if (tr == null) return;

        var tru = GetTru(idtru);
    }

    public TruLinh GetTru(long idtru)
    {
        for (int i = 0; i < truLinhs.Length; i++)
        {
            var t = truLinhs[i];
            if (t != null && t.idTru == idtru)
                return t;
        }
        return null;
    }

    private MinionMove GetLinh(long idLinh)
    {
        for (int i = 0; i < minionMoves.Count; i++)
        {
            var item = minionMoves[i];
            if (item != null && item.minionId == idLinh)
                return item;
        }
        return null;
    }

    public void UpdateMinionResource(long id, int hp, int maxHp)
    {
        if (activeMinions.TryGetValue(id, out var minion) && minion != null)
        {
            float x = minion.transform.position.x;
            float y = minion.transform.position.z;
            minion.UpdateFromServer(x, y, hp, maxHp);
        }
    }

    public void UpdateTurretResource(long id, int teamId, int hp, int maxHp)
    {
        BuildTurretMapIfNeeded();

        if (_turretById.TryGetValue(id, out var t) && t != null)
        {
            t.UpdateHP(hp, maxHp);
            return;
        }

        int idx = (int)id;
        if (truLinhs != null && idx >= 0 && idx < truLinhs.Length)
        {
            var t2 = truLinhs[idx];
            if (t2 != null)
            {
                if (t2.idTru <= 0) t2.idTru = idx;
                _turretById[t2.idTru] = t2;

                t2.UpdateHP(hp, maxHp);
                return;
            }
        }
    }

    private void BuildTurretMapIfNeeded()
    {
        if (_turretMapBuilt) return;
        _turretMapBuilt = true;

        _turretById.Clear();

        if (truLinhs == null) return;

        for (int i = 0; i < truLinhs.Length; i++)
        {
            var t = truLinhs[i];
            if (t == null) continue;

            if (t.idTru <= 0)
                t.idTru = i;

            long id = t.idTru;

            if (_turretById.ContainsKey(id))
                continue;

            _turretById[id] = t;
        }
    }

    public void ReconcileMissingTurretsFromSnapshot(HashSet<long> presentIds)
    {
        if (_reconciledTurretsOnce) return;
        _reconciledTurretsOnce = true;

        if (truLinhs == null || truLinhs.Length == 0) return;
        if (presentIds == null || presentIds.Count == 0) return;

        for (int i = 0; i < truLinhs.Length; i++)
        {
            var t = truLinhs[i];
            if (t == null) continue;

            long key = t.idTru > 0 ? t.idTru : i;

            if (!presentIds.Contains(key))
            {
                if (!t.IsMainTurret)
                {
                    Destroy(t.gameObject);
                }
                else
                {
                    t.UpdateHP(0, t.maxHP > 0 ? t.maxHP : 1);
                }
            }
        }
    }

    private MinionMove GetMinionFromPool()
    {
        while (_minionPool.Count > 0)
        {
            var m = _minionPool.Pop();
            if (m != null) return m;
        }
        return null;
    }

    private void ReturnMinionToPool(MinionMove m)
    {
        if (m == null) return;
        m.gameObject.SetActive(false);
        _minionPool.Push(m);
    }

    private PlayerOther GetOrCreateOther(long userId, int heroType)
    {
        if (userId <= 0) return null;

        if (othersByUserId.TryGetValue(userId, out var existing) && existing != null)
        {
            HeroTypeByUserId.TryGetValue(userId, out var cachedHeroType);

            if (cachedHeroType > 0 && heroType > 0 && cachedHeroType != heroType)
            {
                Destroy(existing.gameObject);
                othersByUserId.Remove(userId);
                playersByUserId.Remove(userId);
            }
            else
            {
                return existing;
            }
        }

        BuildHeroPrefabMapIfNeeded();

        int resolvedHeroType = (heroType > 0)
            ? heroType
            : ResolveAndCacheHeroType(userId, 0, 0);

        PlayerOther prefab = null;
        heroPrefabMap.TryGetValue(resolvedHeroType, out prefab);

        if (prefab == null && _availableHeroTypes.Count > 0)
        {
            int fallbackHero = _availableHeroTypes[0];
            heroPrefabMap.TryGetValue(fallbackHero, out prefab);
        }

        if (prefab == null)
        {
            Debug.LogError("[TranDauControl] No hero prefab found.");
            return null;
        }

        var po = Instantiate(prefab, Vector3.zero, Quaternion.identity);

        if (othersContainer != null)
            po.transform.SetParent(othersContainer, false);

        po.transform.localPosition = Vector3.zero;
        po.transform.localRotation = Quaternion.identity;
        po.transform.localScale = Vector3.one;

        po.gameObject.SetActive(false);
        othersByUserId[userId] = po;

        return po;
    }

    public void GetAllPlayerUserIdsNonAlloc(List<long> outIds)
    {
        if (outIds == null) return;
        outIds.Clear();

        foreach (var kv in playersByUserId)
        {
            if (kv.Key > 0)
                outIds.Add(kv.Key);
        }
    }

    public void EmergencyMemoryCleanup()
    {
        Debug.Log("[TranDauControl] Emergency memory cleanup...");

        HeroTypeByUserId.Clear();
        playersByUserId.Clear();
        _lastSeenOtherTime.Clear();
        seenOtherIds.Clear();
        toRemoveOthers.Clear();

        var removeKeys = new List<long>(othersByUserId.Count);

        foreach (var kv in othersByUserId)
        {
            var po = kv.Value;
            if (po == null || !po.gameObject.activeInHierarchy)
            {
                removeKeys.Add(kv.Key);
                if (po != null) Destroy(po.gameObject);
            }
        }

        foreach (var key in removeKeys)
        {
            othersByUserId.Remove(key);
        }

        var removeMinions = new List<long>(activeMinions.Count);

        foreach (var kv in activeMinions)
        {
            var minion = kv.Value;
            if (minion == null || !minion.gameObject.activeInHierarchy)
            {
                removeMinions.Add(kv.Key);
                if (minion != null)
                {
                    minion.gameObject.SetActive(false);
                    _minionPool.Push(minion);
                }
            }
        }

        foreach (var key in removeMinions)
        {
            activeMinions.Remove(key);
        }

        while (_minionPool.Count > 20)
        {
            var m = _minionPool.Pop();
            if (m != null) Destroy(m.gameObject);
        }

        cachedPlayerTargets.Clear();
        minionMoves.TrimExcess();

        Debug.Log($"[TranDauControl] Cleanup done: " +
                  $"PlayerOthers={othersByUserId.Count}, " +
                  $"ActiveMinions={activeMinions.Count}, " +
                  $"PooledMinions={_minionPool.Count}");
    }

    public void OnUdpSkillCast(SkillCastInfo info)
    {
        GameObject attackerObj = GetPlayerByUserId(info.attackerId);

        if (attackerObj == null)
        {
            long myId = UserData.Instance != null ? UserData.Instance.UserID : 0;
            if (myId != 0 && info.attackerId == myId)
                attackerObj = playerMove != null ? playerMove.gameObject : null;
        }

        if (attackerObj == null)
            return;

        SkillCastInfo fixedInfo = info;

        bool needDir = SkillCastProtocol33.UsesDirection(fixedInfo.typeSkill);
        bool needTarget = SkillCastProtocol33.UsesTargetPos(fixedInfo.typeSkill);

        if (fixedInfo.hasTarget && (needTarget || fixedInfo.dir.sqrMagnitude < 0.0001f))
        {
            Vector3 d = fixedInfo.targetPos - fixedInfo.origin;
            d.y = 0f;
            if (d.sqrMagnitude > 0.0001f)
                fixedInfo.dir = d.normalized;
        }

        if (needDir && fixedInfo.dir.sqrMagnitude > 0.0001f)
        {
            fixedInfo.dir.y = 0f;
            fixedInfo.dir.Normalize();
        }

        if (needTarget && !fixedInfo.hasTarget)
        {
            if (fixedInfo.dir.sqrMagnitude > 0.0001f && fixedInfo.maxRange > 0.01f)
                fixedInfo.targetPos = fixedInfo.origin + fixedInfo.dir * fixedInfo.maxRange;
            else
                fixedInfo.targetPos = fixedInfo.origin;

            fixedInfo.hasTarget = true;
        }

        if (fixedInfo.dir.sqrMagnitude > 0.0001f)
        {
            Vector3 look = fixedInfo.dir;
            look.y = 0f;
            Quaternion rot = Quaternion.LookRotation(look);

            if (attackerObj.TryGetComponent<PlayerMove>(out var pm) && pm.controller != null)
            {
                pm.controller.transform.rotation = rot;
            }
            else
            {
                attackerObj.transform.rotation = rot;
            }

            if (attackerObj.TryGetComponent<PlayerOther>(out var po2))
                po2.transform.rotation = rot;
        }

        attackerObj.SendMessage("OnServerSkillCast", fixedInfo, SendMessageOptions.DontRequireReceiver);
    }
}
