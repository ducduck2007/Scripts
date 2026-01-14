using System.Collections.Generic;
using UnityEngine;

public class TranDauControl : ManualSingleton<TranDauControl>
{
    public CameraFollow cameraF;
    public PlayerMove[] playerMoves;
    public TruLinh[] truLinhs;
    public PlayerMove playerMove
    {
        get
        {
            return playerMoves[B.Instance.heroPlayer];
        }
    }
    public PlayerOther[] playerOthers;
    public PlayerOther playerOther
    {
        get
        {
            return playerOthers[B.Instance.heroOther];
        }
    }
    public JungleMonster[] jungleMonsters;

    public GameObject minionPrefab;
    public Transform minionContainer;
    private Dictionary<long, MinionMove> activeMinions = new Dictionary<long, MinionMove>();

    // ========== OPTIMIZATION: Cache targets để tránh FindObjectsOfType ==========
    private List<Transform> cachedPlayerTargets = new List<Transform>();
    private float refreshCacheTimer = 0f;
    private const float CACHE_REFRESH_INTERVAL = 1f; // Refresh cache mỗi 1 giây

    public virtual void Start()
    {
        minionMoves.Clear();
        for (int i = 0; i < playerMoves.Length; i++)
        {
            if (i == B.Instance.heroPlayer)
            {
                playerMoves[i].gameObject.SetActive(true);
                if (playerMoves[i].HealthBar != null)
                {
                    playerMoves[i].HealthBar.gameObject.SetActive(true);
                }
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
                playerOthers[i].HealthBar.gameObject.SetActive(true);
            }
            else
            {
                playerOthers[i].gameObject.SetActive(false);
            }
        }
        cameraF.SetTarget(playerMove.transform);
        playerMove.SetPotion();

        // OPTIMIZATION: Build initial cache
        RefreshTargetCache();
    }

    // ========== OPTIMIZATION: Update cache periodically ==========
    private void Update()
    {
        refreshCacheTimer += Time.deltaTime;
        if (refreshCacheTimer >= CACHE_REFRESH_INTERVAL)
        {
            refreshCacheTimer = 0f;
            RefreshTargetCache();
        }
    }

    // ========== OPTIMIZATION: Cache tất cả targets 1 lần ==========
    private void RefreshTargetCache()
    {
        cachedPlayerTargets.Clear();

        // Cache PlayerMoves
        foreach (var pm in playerMoves)
        {
            if (pm != null && pm.gameObject.activeSelf)
            {
                cachedPlayerTargets.Add(pm.transform);
            }
        }

        // Cache PlayerOthers (PlayerOther)
        foreach (var po in playerOthers)
        {
            if (po != null && po.gameObject.activeSelf)
            {
                cachedPlayerTargets.Add(po.transform);
            }
        }
    }

    // ========== OPTIMIZATION: API cho PlayerOther tìm enemy ==========
    public Transform FindNearestEnemy(Vector3 position, float range, int myTeamId)
    {
        Transform nearest = null;
        float minDist = range * range; // Dùng sqrMagnitude để nhanh hơn

        foreach (var target in cachedPlayerTargets)
        {
            if (target == null || !target.gameObject.activeSelf)
                continue;

            // Check team - bỏ qua đồng đội
            var pm = target.GetComponent<PlayerMove>();
            if (pm != null)
            {
                if (myTeamId != 0 && B.Instance.teamId == myTeamId)
                    continue;
            }

            var po = target.GetComponent<PlayerOther>();
            if (po != null)
            {
                if (myTeamId != 0 && po.teamId == myTeamId)
                    continue;
            }

            // Check distance với sqrMagnitude (nhanh hơn Distance)
            float sqrDist = (target.position - position).sqrMagnitude;
            if (sqrDist < minDist)
            {
                minDist = sqrDist;
                nearest = target;
            }
        }

        return nearest;
    }

    public void Init(List<PlayerOutPutSv> playersData)
    {
        if (playerMove == null || playerOther == null)
        {
            return;
        }
        foreach (var pdata in playersData)
        {
            if (pdata.userId == UserData.Instance.UserID)
            {
                playerMove.ApplyServerData(pdata);
                if (pdata.teamId == 1)
                {
                    int layer1 = LayerMask.NameToLayer("player1");
                    if (playerMove.controller.gameObject.layer != layer1)
                    {
                        playerMove.controller.gameObject.layer = layer1;
                    }
                    if (playerOther.enemyLayer != layer1)
                    {
                        playerOther.enemyLayer = layer1;
                    }
                    int layer2 = LayerMask.GetMask("player2");
                    if (playerMove.enemyLayer != layer2)
                    {
                        playerMove.enemyLayer = layer2;
                    }
                }
                else
                {
                    int layer2 = LayerMask.NameToLayer("player2");
                    if (playerMove.controller.gameObject.layer != layer2)
                    {
                        playerMove.controller.gameObject.layer = layer2;
                    }
                    if (playerOther.enemyLayer != layer2)
                    {
                        playerOther.enemyLayer = layer2;
                    }
                    int layer1 = LayerMask.GetMask("player1");
                    if (playerMove.enemyLayer != layer1)
                    {
                        playerMove.enemyLayer = layer1;
                    }
                }
            }
            else
            {
                playerOther.ApplyServerData(pdata);
                if (pdata.teamId == 1)
                {
                    int layer = LayerMask.NameToLayer("player1");
                    if (playerOther.gameObject.layer != layer)
                    {
                        playerOther.gameObject.layer = layer;
                    }
                }
                else
                {
                    int layer = LayerMask.NameToLayer("player2");
                    if (playerOther.gameObject.layer != layer)
                    {
                        playerOther.gameObject.layer = layer;
                    }
                }
            }
        }

        // OPTIMIZATION: Refresh cache sau khi init
        RefreshTargetCache();
    }

    public void InitMonster(List<JungleMonsterOutPutSv> monstersData)
    {
        foreach (var mdata in monstersData)
        {
            foreach (var monster in jungleMonsters)
            {
                if (monster.id == mdata.id)
                {
                    monster.UpdateFromServer(mdata.x, mdata.y, mdata.hp, mdata.hpMax);
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
        if (skillId == 1)
        {
            playerOther.CastSkillFromServer(1, hasTarget);
        }
        else if (skillId == 2)
        {
            playerOther.CastSkillFromServer(2, hasTarget);
        }
        else if (skillId == 3)
        {
            playerOther.CastSkillFromServer(3, hasTarget);
        }
    }

    // ========== MINION LOGIC - DISABLED BUT KEPT FOR COMPATIBILITY ==========
    public void InitMinions(List<MinionOutPutSv> minionsData)
    {
        // OPTIMIZATION: Tắt tạm minion để giảm CPU load
        // Uncomment đoạn dưới khi cần spawn minions
        return;

        /*
        if (minionsData == null || minionPrefab == null) return;

        foreach (var mdata in minionsData)
        {
            if (activeMinions.ContainsKey(mdata.id))
            {
                activeMinions[mdata.id].UpdateFromServer(mdata.x, mdata.y, mdata.hp, mdata.maxHp);
            }
            else
            {
                SpawnMinion(mdata);
            }
        }

        List<long> toRemove = new List<long>();
        foreach (var kvp in activeMinions)
        {
            bool stillExists = minionsData.Exists(m => m.id == kvp.Key);
            if (!stillExists)
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var id in toRemove)
        {
            if (activeMinions[id] != null)
            {
                Destroy(activeMinions[id].gameObject);
            }
            activeMinions.Remove(id);
        }
        */
    }

    private List<MinionMove> minionMoves = new List<MinionMove>();

    private void SpawnMinion(MinionOutPutSv data)
    {
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
        // Debug.Log($"Spawned Minion ID={data.id}, Team={data.teamId}, Lane={data.laneId}");
    }

    public void MonterDeath(long id)
    {
        try
        {
            if (GetQuaiRung(id) != null)
            {
                GetQuaiRung(id).Die();
            }
        }
        catch (System.Exception)
        {
            throw;
        }
    }

    public void MinionDeath(long id)
    {
        try
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
        catch (System.Exception)
        {
            throw;
        }
    }

    public void TruLinhDeath(int id)
    {
        try
        {
            if (GetTru(id) != null)
            {
                GetTru(id).OnDeath();
            }
        }
        catch (System.Exception)
        {
            throw;
        }
    }

    public void PutTruBan(long idtru, long idTaget, int typeTaget, int team)
    {
        Transform transform = null;
        if (typeTaget == 0)
        {
            if (idTaget == UserData.Instance.UserID)
            {
                transform = playerMove.transform;
            }
            else
            {
                transform = playerOther.transform;
            }
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