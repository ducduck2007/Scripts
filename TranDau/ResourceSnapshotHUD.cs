using System.Threading;
using TMPro;
using UnityEngine;

public class ResourceSnapshotHUD : MonoBehaviour
{
    [Header("Local Player HUD")]
    public TMP_Text txtGold;
    public TMP_Text txtSkills;
    public TMP_Text txtShield;
    public TMP_Text txtKDA; // kéo trong Canvas

    [Header("Debug")]
    public bool debugHud = true;

    // Debug tool: nếu muốn ép show 1 userId cụ thể
    public long forceUserId = 0;

    private long _localUserId;

    private void OnEnable()
    {
        _localUserId = UserData.Instance != null ? UserData.Instance.UserID : 0;
        UdpResourceSnapshotSystem.OnPlayerResourceUpdated += OnAnyPlayerUpdated;

        if (debugHud)
        {
            // Debug.Log($"[HUD CMD51] OnEnable | threadId={Thread.CurrentThread.ManagedThreadId} | localUserId={_localUserId}");
            LogNullRefs();
        }
    }

    private void OnDisable()
    {
        UdpResourceSnapshotSystem.OnPlayerResourceUpdated -= OnAnyPlayerUpdated;
    }

    private void OnAnyPlayerUpdated(PlayerResourceData data, bool isLocal)
    {
        // fallback nếu OnEnable chưa có UserData
        if (_localUserId == 0 && UserData.Instance != null)
            _localUserId = UserData.Instance.UserID;

        // Nếu có ép userId thì show đúng thằng đó (phục vụ debug)
        if (forceUserId != 0)
        {
            if (data.userId != forceUserId) return;
            ApplyToUI(data);
            return;
        }

        // Local-only: chỉ update local player
        bool reallyLocal = isLocal || (_localUserId != 0 && data.userId == _localUserId);
        if (!reallyLocal) return;

        if (debugHud)
        {
            // Debug.Log($"[HUD CMD51] LOCAL UPDATE | userId={data.userId} gold={data.gold} kda={data.kills}/{data.deaths}/{data.assists}");
        }

        ApplyToUI(data);
    }

    private void ApplyToUI(PlayerResourceData data)
    {
        if (txtGold != null) txtGold.text = data.gold.ToString();
        if (txtSkills != null) txtSkills.text = $"{data.skill1Level}/{data.skill2Level}/{data.skill3Level}";
        if (txtShield != null) txtShield.text = data.shield.ToString();
        if (txtKDA != null) txtKDA.text = $"{data.kills}/{data.deaths}/{data.assists}";
    }

    private void LogNullRefs()
    {
        if (txtGold == null) Debug.LogWarning("[HUD CMD51] txtGold is NULL");
        if (txtSkills == null) Debug.LogWarning("[HUD CMD51] txtSkills is NULL");
        if (txtShield == null) Debug.LogWarning("[HUD CMD51] txtShield is NULL");
        if (txtKDA == null) Debug.LogWarning("[HUD CMD51] txtKDA is NULL");
    }
}
