using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class MatchFoundDataBase
{
    // ========== SINGLETON ==========
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

    // ========== MATCH PLAYER INFO ==========
    public class MatchPlayer
    {
        public long UserId;
        public bool Accepted;
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

    public List<MatchPlayer> Team1Players { get; private set; } = new List<MatchPlayer>();
    public List<MatchPlayer> Team2Players { get; private set; } = new List<MatchPlayer>();

    private float _remainingSeconds;
    private bool _isCounting;

    // ========== EVENTS ==========
    public event Action OnMatchFound;
    public event Action OnAcceptProgressUpdated;
    public event Action OnMatchReady;
    public event Action OnMatchCancelled;
    public event Action<float> OnTimerTick;

    // ========== CONSTRUCTOR ==========
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
        Team1Players.Clear();
        Team2Players.Clear();
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

            // Parse đội 1
            Team1Players.Clear();
            if (msg.ConstainsKey("doi1"))
            {
                JArray doi1 = msg.GetJArray("doi1");
                for (int i = 0; i < doi1.Count; i++)
                {
                    JObject json = (JObject)doi1[i];
                    Team1Players.Add(new MatchPlayer
                    {
                        UserId = json.Value<long>("userId"),
                        Accepted = json.Value<bool>("accepted")
                    });
                }
            }

            // Parse đội 2
            Team2Players.Clear();
            if (msg.ConstainsKey("doi2"))
            {
                JArray doi2 = msg.GetJArray("doi2");
                for (int i = 0; i < doi2.Count; i++)
                {
                    JObject json = (JObject)doi2[i];
                    Team2Players.Add(new MatchPlayer
                    {
                        UserId = json.Value<long>("userId"),
                        Accepted = json.Value<bool>("accepted")
                    });
                }
            }

            Debug.Log($"[MatchFoundDataBase] Match found: MatchId={MatchId}, {TotalPlayers} players, {TeamSize}v{TeamSize}, doi1={Team1Players.Count}, doi2={Team2Players.Count}");

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

            if (msg.ConstainsKey("danhSachTrangThaiChapNhan"))
            {
                JArray danhSach = msg.GetJArray("danhSachTrangThaiChapNhan");
                for (int i = 0; i < danhSach.Count; i++)
                {
                    JObject json = (JObject)danhSach[i];
                    long userId = json.Value<long>("userId");
                    bool accepted = json.Value<bool>("accepted");

                    foreach (var p in Team1Players)
                    {
                        if (p.UserId == userId) { p.Accepted = accepted; break; }
                    }
                    foreach (var p in Team2Players)
                    {
                        if (p.UserId == userId) { p.Accepted = accepted; break; }
                    }
                }
            }

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
        if (_isCounting)
        {
            _remainingSeconds -= Time.deltaTime;

            if (_remainingSeconds <= -1f)
            {
                _remainingSeconds = 0;
                _isCounting = false;
                SendData.PartyDeclineMatch();
            }

            OnTimerTick?.Invoke(Mathf.Max(_remainingSeconds, 0f));
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