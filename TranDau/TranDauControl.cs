// File: TranDauControl.cs
using System.Collections.Generic;
using UnityEngine;

public class TranDauControl : ManualSingleton<TranDauControl>
{
    public CameraFollow cameraF;
    public PlayerMove[] playerMoves;
    public TruLinh[] truLinhs;

    public PlayerMove playerMove
    {
        get { return playerMoves[B.Instance.heroPlayer]; }
    }

    public PlayerOther[] playerOthers;
    public PlayerOther playerOther
    {
        get { return playerOthers[B.Instance.heroOther]; }
    }

    public JungleMonster[] jungleMonsters;

    public GameObject minionPrefab;
    public Transform minionContainer;
    private Dictionary<long, MinionMove> activeMinions = new Dictionary<long, MinionMove>();

    // ==================== PLAYER ID MAPPING ====================
    private readonly Dictionary<long, GameObject> playersByUserId = new Dictionary<long, GameObject>();

    public GameObject GetPlayerByUserId(long userId)
    {
        playersByUserId.TryGetValue(userId, out var obj);
        return obj;
    }

    private void RegisterPlayer(long userId, GameObject obj)
    {
        if (userId <= 0 || obj == null) return;
        playersByUserId[userId] = obj;
    }

    // ========== OPTIMIZATION: Cache targets để tránh FindObjectsOfType ==========
    private readonly List<Transform> cachedPlayerTargets = new List<Transform>();
    private float refreshCacheTimer = 0f;
    private const float CACHE_REFRESH_INTERVAL = 1f;

    // ==================== SNAPSHOT STATE (FIX CMD50) ====================
    private bool _localSetupDone = false;
    private bool _otherSetupDone = false;

    private long _otherUserId = -1;
    private int _otherTeamId = -1;

    public virtual void Start()
    {
        playersByUserId.Clear();

        // ==== FIX: clear luôn dictionary minion để tránh rác id/obj sau match mới ====
        minionMoves.Clear();
        activeMinions.Clear();

        for (int i = 0; i < playerMoves.Length; i++)
        {
            if (i == B.Instance.heroPlayer)
            {
                playerMoves[i].gameObject.SetActive(true);
                if (playerMoves[i].HealthBar != null)
                    playerMoves[i].HealthBar.gameObject.SetActive(true);
            }
            else
            {
                playerMoves[i].controller.enabled = false;
                playerMoves[i].gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < playerOthers.Length; i++)
        {
            if (i == B.Instance.heroOther)
            {
                playerOthers[i].gameObject.SetActive(true);
                if (playerOthers[i].HealthBar != null)
                    playerOthers[i].HealthBar.gameObject.SetActive(true);
            }
            else
            {
                playerOthers[i].gameObject.SetActive(false);
            }
        }

        cameraF.SetTarget(playerMove.transform);
        playerMove.SetPotion();

        RefreshTargetCache();
    }

    private void Update()
    {
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

        foreach (var pm in playerMoves)
        {
            if (pm != null && pm.gameObject.activeSelf)
                cachedPlayerTargets.Add(pm.transform);
        }

        foreach (var po in playerOthers)
        {
            if (po != null && po.gameObject.activeSelf)
                cachedPlayerTargets.Add(po.transform);
        }
    }

    // API cho PlayerOther tìm enemy (cache)
    public Transform FindNearestEnemy(Vector3 position, float range, int myTeamId)
    {
        Transform nearest = null;
        float minDist = range * range;

        foreach (var t in cachedPlayerTargets)
        {
            if (t == null || !t.gameObject.activeSelf) continue;

            // bỏ qua bản thân
            if (t == this.transform) continue;

            // Team check
            var pm = t.GetComponent<PlayerMove>();
            if (pm != null)
            {
                // PlayerMove là local player (team của local = B.Instance.teamId)
                if (myTeamId != 0 && B.Instance.teamId == myTeamId)
                    continue;
            }

            var po = t.GetComponent<PlayerOther>();
            if (po != null)
            {
                if (po == playerOther && myTeamId != 0 && po.teamId == myTeamId)
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

    // ==================== FIX CMD50: Init chỉ UPDATE DATA, SETUP 1 LẦN ====================
    public void Init(List<PlayerOutPutSv> playersData)
    {
        if (playersData == null || playersData.Count == 0) return;
        if (playerMove == null || playerOther == null) return;

        long myId = UserData.Instance.UserID;

        // 1) Update local player
        for (int i = 0; i < playersData.Count; i++)
        {
            var pdata = playersData[i];
            if (pdata.userId != myId) continue;

            playerMove.ApplyServerData(pdata);

            if (!_localSetupDone)
            {
                RegisterPlayer(pdata.userId, playerMove.gameObject);
                SetupLocalTeamLayersOnce(pdata.teamId);
                _localSetupDone = true;
                RefreshTargetCache();
            }
            break;
        }

        // 2) Update other player (design hiện tại: chỉ có 1 enemy hero object playerOther)
        PlayerOutPutSv other = default;
        bool hasOther = false;

        for (int i = 0; i < playersData.Count; i++)
        {
            var pdata = playersData[i];
            if (pdata.userId == myId) continue;

            other = pdata;
            hasOther = true;
            break;
        }

        if (!hasOther) return;

        // Nếu enemy userId đổi (match mới/reconnect) => reset setup
        if (_otherUserId != other.userId)
        {
            _otherSetupDone = false;
            _otherUserId = other.userId;
            _otherTeamId = -1;
        }

        // Luôn update movement/rotation/hp
        playerOther.ApplyServerData(other);

        // Setup one-time hoặc khi đổi team
        if (!_otherSetupDone || _otherTeamId != other.teamId)
        {
            _otherTeamId = other.teamId;

            playerOther.SetTeamId(other.teamId);

            int layer = LayerMask.NameToLayer(other.teamId == 1 ? "player1" : "player2");
            if (playerOther.gameObject.layer != layer)
                playerOther.gameObject.layer = layer;

            RegisterPlayer(other.userId, playerOther.gameObject);

            _otherSetupDone = true;
            RefreshTargetCache();
        }
    }

    private void SetupLocalTeamLayersOnce(int myTeamId)
    {
        if (myTeamId == 1)
        {
            int layer1 = LayerMask.NameToLayer("player1");
            if (playerMove.controller.gameObject.layer != layer1)
                playerMove.controller.gameObject.layer = layer1;

            if (playerOther.enemyLayer != layer1)
                playerOther.enemyLayer = layer1;

            int layer2Mask = LayerMask.GetMask("player2");
            if (playerMove.enemyLayer != layer2Mask)
                playerMove.enemyLayer = layer2Mask;
        }
        else
        {
            int layer2 = LayerMask.NameToLayer("player2");
            if (playerMove.controller.gameObject.layer != layer2)
                playerMove.controller.gameObject.layer = layer2;

            if (playerOther.enemyLayer != layer2)
                playerOther.enemyLayer = layer2;

            int layer1Mask = LayerMask.GetMask("player1");
            if (playerMove.enemyLayer != layer1Mask)
                playerMove.enemyLayer = layer1Mask;
        }
    }

    public void InitMonster(List<JungleMonsterOutPutSv> monstersData)
    {
        if (monstersData == null || monstersData.Count == 0) return;

        foreach (var mdata in monstersData)
        {
            foreach (var monster in jungleMonsters)
            {
                if (monster != null && monster.id == mdata.id)
                {
                    // ✅ CMD50: chỉ update pos (hp/hpMax=0)
                    bool hasHp = (mdata.hpMax > 0 || mdata.hp > 0);

                    if (hasHp)
                    {
                        monster.UpdateFromServer(mdata.x, mdata.y, mdata.hp, mdata.hpMax);
                    }
                    else
                    {
                        // chỉ update vị trí, giữ hp hiện tại
                        monster.UpdateFromServer(mdata.x, mdata.y, 0, 0); // nếu UpdateFromServer overwrite hp khi =0, cần sửa JungleMonster
                    }
                    break;
                }
            }
        }
    }

    // ==================== APPLY RESOURCES FROM CMD51 ====================

    public void ApplyMinionResources(List<MinionResourceData> data)
    {
        if (data == null || data.Count == 0) return;

        foreach (var r in data)
        {
            if (activeMinions.TryGetValue(r.id, out var m) && m != null)
            {
                // giữ pos hiện tại, update hp
                float x = m.transform.position.x;
                float y = m.transform.position.z;
                m.UpdateFromServer(x, y, r.hp, r.maxHp);
            }
        }
    }

    public void ApplyMonsterResources(List<MonsterResourceData> data)
    {
        if (data == null || data.Count == 0) return;

        foreach (var r in data)
        {
            foreach (var monster in jungleMonsters)
            {
                if (monster != null && monster.id == r.id)
                {
                    float x = monster.transform.position.x;
                    float y = monster.transform.position.z;
                    monster.UpdateFromServer((int)x, (int)y, r.hp, r.maxHp);
                    break;
                }
            }
        }
    }

    public void ApplyTurretResources(List<TurretResourceData> data)
    {
        if (data == null || data.Count == 0) return;

        foreach (var r in data)
        {
            foreach (var t in truLinhs)
            {
                if (t != null && t.idTru == r.id)
                {
                    t.UpdateHpFromServer(r.hp, r.maxHp);
                    break;
                }
            }
        }
    }

    public void SetAttackState(bool isAttack, bool hasTarget)
    {
        playerOther.SetAttackState(isAttack, hasTarget);
    }

    public void SetCastSkillState(int skillId, bool hasTarget)
    {
        if (skillId == 1) playerOther.CastSkillFromServer(1, hasTarget);
        else if (skillId == 2) playerOther.CastSkillFromServer(2, hasTarget);
        else if (skillId == 3) playerOther.CastSkillFromServer(3, hasTarget);
    }

    // ========================= UDP COMPAT WRAPPERS =========================
    public void SetAttackState(long userId, bool isAttack, bool hasTarget) => OnUdpAttackState(userId, isAttack, hasTarget);
    public void SetCastSkillState(long userId, int skillId, bool hasTarget) => OnUdpCastSkillState(userId, skillId, hasTarget);
    public void PlayerDeath(long victimUserId) => OnUdpPlayerDeath(victimUserId);

    // Wrapper này hiểu x,y là "Unity position" (đã scale)
    public void PlayerRespawn(long userId, int unityX, int unityY, int hp)
    {
        float x = unityX;
        float y = unityY;

        if (userId == UserData.Instance.UserID)
        {
            if (playerMove != null)
            {
                B.Instance.PosX = x;
                B.Instance.PosZ = y;
                playerMove.onRespawn(hp);
            }
            return;
        }

        if (playerOther != null)
            playerOther.onRespawn(x, y, hp);
    }

    public void OnUdpCmd50_SyncPlayers(List<PlayerOutPutSv> playersData)
    {
        Init(playersData);
    }

    public void OnUdpAttackState(long userId, bool isAttack, bool hasTarget)
    {
        if (userId == UserData.Instance.UserID) return;
        SetAttackState(isAttack, hasTarget);
    }

    public void OnUdpCastSkillState(long userId, int skillId, bool hasTarget)
    {
        if (userId == UserData.Instance.UserID) return;
        SetCastSkillState(skillId, hasTarget);
    }

    public void OnUdpPlayerDeath(long victimUserId)
    {
        if (victimUserId == UserData.Instance.UserID)
        {
            if (playerMove != null) playerMove.onDeath();
            return;
        }

        if (playerOther != null) playerOther.onDeath();
    }

    public void OnUdpPlayerRespawn(long userId, int x, int y, int hp)
    {
        float scaledX = x / 2f;
        float scaledY = y / 2f;

        if (userId == UserData.Instance.UserID)
        {
            if (playerMove != null)
            {
                B.Instance.PosX = scaledX;
                B.Instance.PosZ = scaledY;
                playerMove.onRespawn(hp);
            }
            return;
        }

        if (playerOther != null)
            playerOther.onRespawn(scaledX, scaledY, hp);
    }

    // ==================== MINION LOGIC (ENABLED) ====================
    public void InitMinions(List<MinionOutPutSv> minionsData)
    {
        if (minionsData == null) return;

        if (minionPrefab == null)
        {
            Debug.LogError("[Minion] minionPrefab is NULL. Assign it in Inspector!");
            return;
        }

        foreach (var mdata in minionsData)
        {
            if (activeMinions.TryGetValue(mdata.id, out var existing) && existing != null)
            {
                // ✅ CMD50: chỉ update pos. CMD51 mới update hp.
                bool hasHp = (mdata.maxHp > 0 || mdata.hp > 0);

                if (hasHp)
                {
                    existing.UpdateFromServer(mdata.x, mdata.y, mdata.hp, mdata.maxHp);
                }
                else
                {
                    // chỉ update vị trí, giữ hp hiện tại
                    var p = existing.transform.position;
                    existing.UpdateFromServer(mdata.x, mdata.y, 0, 0); // nếu UpdateFromServer overwrite hp khi =0, cần sửa MinionMove (mình note ở dưới)
                }
            }
            else
            {
                SpawnMinion(mdata);
            }
        }

        // Remove minions that no longer exist
        List<long> toRemove = new List<long>();
        foreach (var kvp in activeMinions)
        {
            bool stillExists = minionsData.Exists(m => m.id == kvp.Key);
            if (!stillExists) toRemove.Add(kvp.Key);
        }

        foreach (var id in toRemove)
        {
            if (activeMinions.TryGetValue(id, out var m) && m != null)
                Destroy(m.gameObject);

            activeMinions.Remove(id);

            for (int i = minionMoves.Count - 1; i >= 0; i--)
            {
                if (minionMoves[i] == null || minionMoves[i].minionId == id)
                    minionMoves.RemoveAt(i);
            }
        }
    }

    private List<MinionMove> minionMoves = new List<MinionMove>();

    private void SpawnMinion(MinionOutPutSv data)
    {
        Debug.Log($"[Minion] Spawn id={data.id} team={data.teamId} pos=({data.x},{data.y}) hp={data.hp}/{data.maxHp}");

        Vector3 spawnPos = new Vector3(data.x, 0, data.y);
        GameObject minionObj = Instantiate(minionPrefab, spawnPos, Quaternion.identity);

        if (minionContainer != null)
        {
            minionObj.transform.SetParent(minionContainer);
        }

        MinionMove minion = minionObj.GetComponent<MinionMove>();
        if (minion == null)
        {
            minion = minionObj.AddComponent<MinionMove>();
        }

        minion.SetData(data.id, data.teamId, data.laneId);
        minion.UpdateFromServer(data.x, data.y, data.hp, data.maxHp);

        activeMinions[data.id] = minion;
        minionMoves.Add(minion);
    }

    public void MonterDeath(long id)
    {
        if (GetQuaiRung(id) != null)
        {
            GetQuaiRung(id).Die();
        }
    }

    public void MinionDeath(long id)
    {
        for (int i = 0; i < minionMoves.Count; i++)
        {
            if (minionMoves[i].minionId == id)
            {
                minionMoves[i].OnDeath();
                minionMoves.Remove(minionMoves[i]);
                return;
            }
        }
    }

    public void TruLinhDeath(int id)
    {
        if (GetTru(id) != null)
        {
            GetTru(id).OnDeath();
        }
    }

    public void PutTruBan(long idtru, long idTaget, int typeTaget, int team)
    {
        Transform transform = null;
        if (typeTaget == 0)
        {
            if (idTaget == UserData.Instance.UserID) transform = playerMove.transform;
            else transform = playerOther.transform;
        }
        else if (typeTaget == 1)
        {
            if (GetLinh(idTaget) != null)
            {
                transform = GetLinh(idTaget).transform;
            }
        }
        if (transform == null) return;

        if (GetTru(idtru) != null)
        {
            GetTru(idtru).Shoot(transform);
        }
    }

    private TruLinh GetTru(long idtru)
    {
        foreach (TruLinh item in truLinhs)
        {
            if (item.idTru == idtru)
            {
                return item;
            }
        }
        return null;
    }

    private MinionMove GetLinh(long idLinh)
    {
        foreach (MinionMove item in minionMoves)
        {
            if (item.minionId == idLinh)
            {
                return item;
            }
        }
        return null;
    }

    private JungleMonster GetQuaiRung(long idLinh)
    {
        foreach (JungleMonster item in jungleMonsters)
        {
            if (item.id == idLinh)
            {
                return item;
            }
        }
        return null;
    }
}
