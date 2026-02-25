using System;
using UnityEngine;

public class MatchFoundDataBase
{
    // ========== SINGLETON (giống FriendDataBase) ==========
    private static MatchFoundDataBase instance;

    public static MatchFoundDataBase Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new MatchFoundDataBase();
                Debug.Log("[MatchFoundDataBase] Instance created");
            }
            return instance;
        }
    }

    // ========== MATCH STATE ==========
    public bool HasPendingMatch { get; private set; }
    public int MatchId { get; private set; } = -1;
    public int TotalPlayers { get; private set; }
    public int AcceptedCount { get; private set; }
    public long TimeoutMs { get; private set; }

    public int ModeId { get; private set; } = -1;
    public string MapName { get; private set; } = "";
    public int TeamSize { get; private set; } = 1;

    private float _remainingSeconds;
    private bool _isCounting;

    // ========== EVENTS ==========
    public event Action OnMatchFound;
    public event Action OnAcceptProgressUpdated;
    public event Action OnMatchReady;
    public event Action OnMatchCancelled;
    public event Action<float> OnTimerTick;

    // ========== CONSTRUCTOR (private) ==========
    private MatchFoundDataBase()
    {
        Debug.Log("[MatchFoundDataBase] Initialized");
        ClearMatch();
    }

    // ========== CLEAR ==========
    public void ClearMatch()
    {
        HasPendingMatch = false;
        MatchId = -1;
        TotalPlayers = 0;
        AcceptedCount = 0;
        TimeoutMs = 0;
        ModeId = -1;
        MapName = "";
        TeamSize = 1;
        _remainingSeconds = 0;
        _isCounting = false;
    }

    // ========== APPLY MATCH FOUND (CMD 103) ==========
    public void ApplyMatchFound(Message msg)
    {
        try
        {
            MatchId = TryGetInt(msg, "idTranDau", -1);
            TotalPlayers = TryGetInt(msg, "tongSoNguoiChoiTranDau", 0);
            AcceptedCount = TryGetInt(msg, "soNguoiDaChapNhanTranDau", 0);
            TimeoutMs = TryGetLong(msg, "thoiGianConLaiChapNhan", 30000);

            ModeId = TryGetInt(msg, "idCheDoChoiTranDau", -1);
            MapName = TryGetString(msg, "tenBanDoTranDau", "");
            TeamSize = TryGetInt(msg, "soNguoiMoiDoiTranDau", 1);

            HasPendingMatch = (MatchId > 0);

            _remainingSeconds = TimeoutMs / 1000f;
            _isCounting = true;

            Debug.Log($"[MatchFoundDataBase] Match found: MatchId={MatchId}, {TotalPlayers} players, {TeamSize}v{TeamSize}");

            OnMatchFound?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"[MatchFoundDataBase] ApplyMatchFound error: {e}");
        }
    }

    // ========== UPDATE PROGRESS (CMD 104) ==========
    public void UpdateAcceptProgress(Message msg)
    {
        try
        {
            AcceptedCount = TryGetInt(msg, "soNguoiDaChapNhanTranDau", 0);

            Debug.Log($"[MatchFoundDataBase] Accept progress: {AcceptedCount}/{TotalPlayers}");

            OnAcceptProgressUpdated?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"[MatchFoundDataBase] UpdateAcceptProgress error: {e}");
        }
    }

    // ========== MATCH READY (CMD 106) ==========
    public void MatchReady()
    {
        Debug.Log("[MatchFoundDataBase] Match ready!");

        _isCounting = false;
        ClearMatch();

        OnMatchReady?.Invoke();
    }

    // ========== MATCH CANCELLED (CMD 107) ==========
    public void MatchCancelled()
    {
        Debug.Log("[MatchFoundDataBase] Match cancelled");

        _isCounting = false;
        ClearMatch();

        OnMatchCancelled?.Invoke();
    }

    // ========== TIMER UPDATE ==========
    public void Update()
    {
        if (_isCounting && _remainingSeconds > 0)
        {
            _remainingSeconds -= Time.deltaTime;

            if (_remainingSeconds <= 0)
            {
                _remainingSeconds = 0;
                _isCounting = false;

                // Auto decline khi hết time
                SendData.PartyDeclineMatch();
            }

            OnTimerTick?.Invoke(_remainingSeconds);
        }
    }

    // ========== HELPERS ==========
    public float GetRemainingSeconds() => _remainingSeconds;

    private int TryGetInt(Message msg, string key, int def)
    {
        try { return msg.GetInt(key); }
        catch { return def; }
    }

    private long TryGetLong(Message msg, string key, long def)
    {
        try { return msg.GetLong(key); }
        catch { return def; }
    }

    private string TryGetString(Message msg, string key, string def)
    {
        try { return msg.GetString(key); }
        catch { return def; }
    }
}