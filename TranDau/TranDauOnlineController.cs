// using System;
// using System.Collections.Generic;
// using UnityEngine;

// public class TranDauOnlineController : ManualSingleton<TranDauOnlineController>
// {
//     [Header("Camera")]
//     public CameraFollow cameraF;

//     [Header("Players")]
//     public PlayerMove[] playerMoves;
//     public PlayerOther[] playerOthers;

//     [Header("Map Objects")]
//     public TruLinh[] truLinhs;
//     public JungleMonster[] jungleMonsters;

//     [Header("Minions")]
//     public GameObject minionPrefab;
//     public Transform minionContainer;

//     [Header("Debug")]
//     public bool debugNet;

//     [Header("UI")]
//     public bool enableHealthBars = true;

//     // ==================== PLAYER MANAGEMENT ====================
//     private Dictionary<long, GameObject> activePlayers = new Dictionary<long, GameObject>();
//     private PlayerMove localPlayerMove;
//     private long localUserId;

//     private List<PlayerMove> allPlayerMoves = new List<PlayerMove>();
//     private List<PlayerOther> allPlayerOthers = new List<PlayerOther>();

//     // ==================== MINIONS ====================
//     private Dictionary<long, MinionMove> activeMinions = new Dictionary<long, MinionMove>();
//     private List<MinionMove> minionMoves = new List<MinionMove>();

//     // ==================== TARGET CACHE ====================
//     private List<Transform> cachedPlayerTargets = new List<Transform>();
//     private float refreshCacheTimer = 0f;
//     private const float CACHE_REFRESH_INTERVAL = 1f;

//     // ==================== PUBLIC PROPERTIES ====================
//     public PlayerMove playerMove => localPlayerMove;
//     public PlayerOther playerOther => null;

//     // ==================== INITIALIZATION ====================
//     public virtual void Start()
//     {
//         try
//         {
//             localUserId = UserData.Instance.UserID;

//             if (debugNet)
//             {
//                 Debug.Log("========================================");
//                 Debug.Log("[TranDauOnline] START");
//                 Debug.Log($"  Local userId: {localUserId}");
//                 Debug.Log($"  Team ID: {B.Instance.teamId}");
//                 Debug.Log($"  PlayerMoves: {playerMoves?.Length ?? 0}");
//                 Debug.Log($"  PlayerOthers: {playerOthers?.Length ?? 0}");
//                 Debug.Log($"  enableHealthBars: {enableHealthBars}");
//                 Debug.Log("========================================");
//             }

//             InitializeGame();

//             if (debugNet)
//                 Debug.Log("[TranDauOnline] START COMPLETE - Ready for CMD 50");
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"[TranDauOnline] START FAILED: {e.Message}\n{e.StackTrace}");
//         }
//     }

//     private void Update()
//     {
//         refreshCacheTimer += Time.deltaTime;
//         if (refreshCacheTimer >= CACHE_REFRESH_INTERVAL)
//         {
//             refreshCacheTimer = 0f;
//             RefreshTargetCache();
//         }
//     }

//     private void InitializeGame()
//     {
//         minionMoves.Clear();
//         activeMinions.Clear();
//         activePlayers.Clear();

//         InitializePools();
//         RefreshTargetCache();
//     }

//     private void InitializePools()
//     {
//         allPlayerMoves.Clear();
//         allPlayerOthers.Clear();

//         if (playerMoves != null)
//         {
//             foreach (var pm in playerMoves)
//             {
//                 if (pm == null) continue;

//                 pm.enabled = false;
//                 if (pm.controller != null) pm.controller.enabled = false;

//                 SafeSetHealthBarActive(pm.HealthBar, false, $"Pool.HideLocal:{pm.name}");

//                 pm.gameObject.SetActive(false);
//                 allPlayerMoves.Add(pm);
//             }
//         }

//         if (playerOthers != null)
//         {
//             foreach (var po in playerOthers)
//             {
//                 if (po == null) continue;

//                 po.enabled = false;

//                 SafeSetHealthBarActive(po.HealthBar, false, $"Pool.HideRemote:{po.name}");

//                 po.gameObject.SetActive(false);
//                 allPlayerOthers.Add(po);
//             }
//         }

//         if (debugNet)
//             Debug.Log($"[Pool] Initialized (all hidden) - PlayerMoves: {allPlayerMoves.Count}, PlayerOthers: {allPlayerOthers.Count}");
//     }

//     // ==================== TARGET CACHE ====================
//     private void RefreshTargetCache()
//     {
//         cachedPlayerTargets.Clear();

//         foreach (var kvp in activePlayers)
//         {
//             if (kvp.Value != null && kvp.Value.activeSelf)
//                 cachedPlayerTargets.Add(kvp.Value.transform);
//         }
//     }

//     public Transform FindNearestEnemy(Vector3 position, float range, int myTeamId)
//     {
//         Transform nearest = null;
//         float minDistSqr = range * range;

//         foreach (var target in cachedPlayerTargets)
//         {
//             if (target == null || !target.gameObject.activeSelf)
//                 continue;

//             int targetTeamId = GetPlayerTeamId(target.gameObject);

//             if (myTeamId != 0 && targetTeamId == myTeamId)
//                 continue;

//             float sqrDist = (target.position - position).sqrMagnitude;
//             if (sqrDist < minDistSqr)
//             {
//                 minDistSqr = sqrDist;
//                 nearest = target;
//             }
//         }

//         return nearest;
//     }

//     private int GetPlayerTeamId(GameObject playerObj)
//     {
//         var pm = playerObj.GetComponent<PlayerMove>();
//         if (pm != null)
//             return B.Instance.teamId;

//         var po = playerObj.GetComponent<PlayerOther>();
//         if (po != null)
//             return po.teamId;

//         return 0;
//     }

//     // ==================== MAIN SYNC FROM SERVER (CMD 50 - 30 FPS) ====================
//     public void Init(List<PlayerOutPutSv> playersData)
//     {
//         if (playersData == null || playersData.Count == 0)
//         {
//             if (debugNet)
//                 Debug.LogWarning("[TranDauOnline] Init called with empty player data");
//             return;
//         }

//         foreach (var pdata in playersData)
//         {
//             if (pdata.userId == localUserId)
//             {
//                 InitOrUpdateLocalPlayer(pdata);
//             }
//             else
//             {
//                 InitOrUpdateRemotePlayer(pdata);
//             }
//         }
//     }

//     // ==================== LOCAL PLAYER ====================
//     private void InitOrUpdateLocalPlayer(PlayerOutPutSv pdata)
//     {
//         if (localPlayerMove == null)
//         {
//             SpawnLocalPlayer(pdata);
//         }
//         else
//         {
//             UpdateLocalPlayerState(pdata);
//         }
//     }

//     private void SpawnLocalPlayer(PlayerOutPutSv pdata)
//     {
//         if (localPlayerMove != null)
//         {
//             UpdateLocalPlayerState(pdata);
//             return;
//         }

//         PlayerMove pm = GetAvailablePlayerMove();
//         if (pm == null)
//         {
//             Debug.LogError("[TranDauOnline] No available PlayerMove!");
//             return;
//         }

//         pm.gameObject.SetActive(true);

//         pm.enabled = true;
//         if (pm.controller != null)
//             pm.controller.enabled = true;

//         // Quan trọng: set local + activePlayers sớm để không bị “spawn lại” nếu UI nổ
//         localPlayerMove = pm;
//         activePlayers[pdata.userId] = pm.gameObject;

//         if (enableHealthBars)
//         {
//             SafeSetHealthBarActive(pm.HealthBar, true, $"Local.Show:{pm.name}");
//             SafeSetThanhMau(pm.HealthBar, 0, $"Local.Color:{pm.name}");
//         }
//         else
//         {
//             SafeSetHealthBarActive(pm.HealthBar, false, $"Local.Hide:{pm.name}");
//         }

//         Vector3 spawnPos = new Vector3(pdata.x, 0, pdata.y);
//         if (pm.controller != null)
//         {
//             pm.controller.enabled = false;
//             pm.transform.position = spawnPos;
//             pm.controller.enabled = true;
//         }
//         else
//         {
//             pm.transform.position = spawnPos;
//         }

//         B.Instance.PosX = spawnPos.x;
//         B.Instance.PosZ = spawnPos.z;
//         B.Instance.teamId = pdata.teamId;

//         int layer = LayerMask.NameToLayer(pdata.teamId == 1 ? "player1" : "player2");
//         pm.gameObject.layer = layer;
//         pm.enemyLayer = LayerMask.GetMask(pdata.teamId == 1 ? "player2" : "player1");

//         pm.SetHp(100, 100);

//         if (cameraF != null)
//             cameraF.SetTarget(pm.transform);

//         if (debugNet)
//             Debug.Log($"[TranDauOnline] LOCAL spawn: {pm.name} uid={pdata.userId} pos=({spawnPos.x:0.##},{spawnPos.z:0.##}) team={pdata.teamId}");
//     }

//     private void UpdateLocalPlayerState(PlayerOutPutSv pdata)
//     {
//         if (localPlayerMove == null) return;

//         localPlayerMove.ApplyServerData(pdata);

//         if (!pdata.isAlive && localPlayerMove.isAlive)
//         {
//             localPlayerMove.onDeath();
//         }
//     }

//     // ==================== REMOTE PLAYER ====================
//     private void InitOrUpdateRemotePlayer(PlayerOutPutSv pdata)
//     {
//         if (!activePlayers.TryGetValue(pdata.userId, out GameObject remotePlayerObj) || remotePlayerObj == null || !remotePlayerObj.activeSelf)
//         {
//             SpawnRemotePlayer(pdata);
//             return;
//         }

//         UpdateRemotePlayer(remotePlayerObj, pdata);
//     }

//     private void SpawnRemotePlayer(PlayerOutPutSv pdata)
//     {
//         PlayerOther po = GetAvailablePlayerOther();
//         if (po == null)
//         {
//             Debug.LogError("[TranDauOnline] No available PlayerOther!");
//             return;
//         }

//         po.gameObject.SetActive(true);
//         po.enabled = true;

//         Vector3 spawnPos = new Vector3(pdata.x, 0, pdata.y);

//         // po.ResetNetworkState(spawnPos, pdata.heading, pdata.teamId);

//         if (enableHealthBars)
//         {
//             SafeSetHealthBarActive(po.HealthBar, true, $"Remote.Show:{po.name}");
//             SafeSetThanhMau(po.HealthBar, pdata.teamId == B.Instance.teamId ? 1 : 2, $"Remote.Color:{po.name}");
//         }
//         else
//         {
//             SafeSetHealthBarActive(po.HealthBar, false, $"Remote.Hide:{po.name}");
//         }

//         int layer = LayerMask.NameToLayer(pdata.teamId == 1 ? "player1" : "player2");
//         po.gameObject.layer = layer;
//         po.enemyLayer = LayerMask.GetMask(pdata.teamId == 1 ? "player2" : "player1");

//         po.ApplyServerData(pdata);

//         activePlayers[pdata.userId] = po.gameObject;

//         if (debugNet)
//             Debug.Log($"[TranDauOnline] REMOTE spawn: {po.name} uid={pdata.userId} inst={po.gameObject.GetInstanceID()} pos=({spawnPos.x:0.##},{spawnPos.z:0.##}) team={pdata.teamId}");
//     }

//     private void UpdateRemotePlayer(GameObject remotePlayerObj, PlayerOutPutSv pdata)
//     {
//         if (remotePlayerObj == null || !remotePlayerObj.activeSelf)
//             return;

//         PlayerOther po = remotePlayerObj.GetComponent<PlayerOther>();
//         if (po != null)
//         {
//             po.ApplyServerData(pdata);
//         }
//     }

//     private PlayerMove GetAvailablePlayerMove()
//     {
//         foreach (var pm in allPlayerMoves)
//         {
//             if (pm != null && !activePlayers.ContainsValue(pm.gameObject))
//                 return pm;
//         }
//         return null;
//     }

//     private PlayerOther GetAvailablePlayerOther()
//     {
//         foreach (var po in allPlayerOthers)
//         {
//             if (po != null && !activePlayers.ContainsValue(po.gameObject))
//                 return po;
//         }
//         return null;
//     }

//     // ==================== COMBAT ACTIONS FROM SERVER ====================
//     public void SetAttackState(long userId, bool isAttack, bool hasTarget)
//     {
//         if (userId == localUserId)
//             return;

//         if (!activePlayers.TryGetValue(userId, out GameObject playerObj) || playerObj == null)
//         {
//             if (debugNet)
//                 Debug.LogWarning($"[TranDauOnline] SetAttackState: Player {userId} not found");
//             return;
//         }

//         PlayerOther po = playerObj.GetComponent<PlayerOther>();
//         if (po != null)
//         {
//             po.SetAttackState(isAttack, hasTarget);
//         }
//     }

//     public void SetCastSkillState(long userId, int skillId, bool hasTarget)
//     {
//         if (userId == localUserId)
//             return;

//         if (!activePlayers.TryGetValue(userId, out GameObject playerObj) || playerObj == null)
//         {
//             if (debugNet)
//                 Debug.LogWarning($"[TranDauOnline] SetCastSkillState: Player {userId} not found");
//             return;
//         }

//         PlayerOther po = playerObj.GetComponent<PlayerOther>();
//         if (po != null)
//         {
//             po.CastSkillFromServer(skillId, hasTarget);
//         }
//     }

//     // ==================== DEATH/RESPAWN ====================
//     public void PlayerDeath(long victimId)
//     {
//         try
//         {
//             if (victimId == localUserId)
//             {
//                 if (localPlayerMove != null)
//                     localPlayerMove.onDeath();
//             }
//             else
//             {
//                 if (activePlayers.TryGetValue(victimId, out GameObject playerObj) && playerObj != null)
//                 {
//                     PlayerOther po = playerObj.GetComponent<PlayerOther>();
//                     if (po != null)
//                         po.onDeath();
//                 }
//             }

//             if (debugNet)
//                 Debug.Log($"[TranDauOnline] Player {victimId} died");
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"[TranDauOnline] PlayerDeath error: {e.Message}");
//         }
//     }

//     public void PlayerRespawn(long userId, int x, int y, int hp)
//     {
//         try
//         {
//             float scaledX = x / 2f;
//             float scaledY = y / 2f;

//             if (userId == localUserId)
//             {
//                 if (localPlayerMove != null)
//                 {
//                     B.Instance.PosX = scaledX;
//                     B.Instance.PosZ = scaledY;
//                     localPlayerMove.onRespawn(hp);
//                 }
//             }
//             else
//             {
//                 if (activePlayers.TryGetValue(userId, out GameObject playerObj) && playerObj != null)
//                 {
//                     PlayerOther po = playerObj.GetComponent<PlayerOther>();
//                     if (po != null)
//                         po.onRespawn(scaledX, scaledY, hp);
//                 }
//             }

//             if (debugNet)
//                 Debug.Log($"[TranDauOnline] Player {userId} respawned at ({scaledX:0.##}, {scaledY:0.##})");
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"[TranDauOnline] PlayerRespawn error: {e.Message}");
//         }
//     }

//     // ==================== MINIONS ====================
//     public void InitMinions(List<MinionOutPutSv> minionsData)
//     {
//         // Tạm thời bỏ theo yêu cầu (test movement)
//     }

//     public void MinionDeath(long id)
//     {
//         // Tạm thời bỏ theo yêu cầu (test movement)
//     }

//     // ==================== MONSTERS ====================
//     public void InitMonster(List<JungleMonsterOutPutSv> monstersData)
//     {
//         // Tạm thời bỏ theo yêu cầu (test movement)
//     }

//     public void MonterDeath(long id)
//     {
//         // Tạm thời bỏ theo yêu cầu (test movement)
//     }

//     // ==================== TURRETS ====================
//     public void TruLinhDeath(int id)
//     {
//         try
//         {
//             var turret = GetTru(id);
//             if (turret != null) turret.OnDeath();
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"[TranDauOnline] TruLinhDeath error: {e.Message}");
//         }
//     }

//     public void PutTruBan(long turretId, long targetId, int targetType, int team)
//     {
//         try
//         {
//             Transform target = null;

//             if (targetType == 0)
//             {
//                 if (activePlayers.TryGetValue(targetId, out GameObject playerObj) && playerObj != null)
//                     target = playerObj.transform;
//             }
//             else if (targetType == 1)
//             {
//                 var minion = GetLinh(targetId);
//                 if (minion != null) target = minion.transform;
//             }

//             if (target == null) return;

//             var turret = GetTru(turretId);
//             if (turret != null) turret.Shoot(target);
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"[TranDauOnline] PutTruBan error: {e.Message}");
//         }
//     }

//     // ==================== HELPERS ====================
//     private TruLinh GetTru(long turretId)
//     {
//         if (truLinhs == null) return null;

//         foreach (var item in truLinhs)
//         {
//             if (item != null && item.idTru == turretId) return item;
//         }
//         return null;
//     }

//     private MinionMove GetLinh(long minionId)
//     {
//         foreach (var item in minionMoves)
//         {
//             if (item != null && item.minionId == minionId) return item;
//         }
//         return null;
//     }

//     private JungleMonster GetQuaiRung(long monsterId)
//     {
//         if (jungleMonsters == null) return null;

//         foreach (var item in jungleMonsters)
//         {
//             if (item != null && item.id == monsterId) return item;
//         }
//         return null;
//     }

//     // ==================== PUBLIC HELPERS ====================
//     public GameObject GetPlayerByUserId(long userId)
//     {
//         if (activePlayers.TryGetValue(userId, out GameObject playerObj))
//             return playerObj;
//         return null;
//     }

//     // ==================== HEALTHBAR SAFE HELPERS ====================
//     private void SafeSetHealthBarActive(ProgressBar bar, bool active, string tag)
//     {
//         if (!enableHealthBars) active = false;
//         if (bar == null) return;

//         try
//         {
//             bar.gameObject.SetActive(active);
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"[TranDauOnline][HB] SetActive failed tag={tag}: {e.Message}");
//         }
//     }

//     private void SafeSetThanhMau(ProgressBar bar, int type, string tag)
//     {
//         if (!enableHealthBars) return;
//         if (bar == null) return;

//         try
//         {
//             bar.SetThanhMau(type);
//         }
//         catch (Exception e)
//         {
//             Debug.LogError($"[TranDauOnline][HB] SetThanhMau failed tag={tag}: {e.Message}");
//         }
//     }
// }
