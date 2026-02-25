using System;
using System.Collections.Generic;
using UnityEngine;

public class PartyDataBase
{
    // ========== SINGLETON (giống FriendDataBase) ==========
    private static PartyDataBase instance;

    public bool PendingOpenPopupTimTran = false;

    public static PartyDataBase Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new PartyDataBase();
                Debug.Log("[PartyDataBase] Instance created");
            }
            return instance;
        }
    }

    // ========== PARTY MEMBER CLASS ==========
    [Serializable]
    public class PartyMember
    {
        public long idThanhVien;
        public string tenThanhVien;
        public int capDoThanhVien;
        public int anhDaiDienThanhVien;
        public bool laTruongNhom;
    }

    // ========== PARTY STATE ==========
    public bool IsInParty { get; private set; }
    public int PartyId { get; private set; } = -1;
    public long LeaderId { get; private set; }
    public string LeaderName { get; private set; } = "";
    public int MemberCount { get; private set; }
    public int MaxMembers { get; private set; } = 5;
    public string PartyState { get; private set; } = "IDLE";

    public int SelectedModeId { get; private set; } = -1;
    public string SelectedMapName { get; private set; } = "";

    public List<PartyMember> Members = new List<PartyMember>();

    // ========== EVENTS ==========
    public event Action OnPartyChanged;
    public event Action OnPartyCreated;
    public event Action OnPartyDisbanded;
    public event Action OnMemberJoined;
    public event Action OnMemberLeft;

    // ========== CONSTRUCTOR (private) ==========
    private PartyDataBase()
    {
        Debug.Log("[PartyDataBase] Initialized");
        ClearParty();
    }

    // ========== CLEAR ==========
    public void ClearParty()
    {
        bool wasInParty = IsInParty;

        IsInParty = false;
        PartyId = -1;
        LeaderId = 0;
        LeaderName = "";
        MemberCount = 0;
        MaxMembers = 5;
        PartyState = "IDLE";
        SelectedModeId = -1;
        SelectedMapName = "";
        Members.Clear();

        OnPartyChanged?.Invoke();
        if (wasInParty) OnPartyDisbanded?.Invoke();
    }

    // ========== APPLY FULL INFO (CMD 92, 98) ==========
    public void ApplyFullInfo(Message msg)
    {
        try
        {
            bool wasInParty = IsInParty;

            // Basic info
            PartyId = TryGetInt(msg, "idNhom", -1);
            LeaderId = TryGetLong(msg, "idTruongNhom", 0);
            LeaderName = TryGetString(msg, "tenTruongNhom", "");
            MemberCount = TryGetInt(msg, "soThanhVienNhom", 0);
            MaxMembers = TryGetInt(msg, "soThanhVienToiDaNhom", 5);
            PartyState = TryGetString(msg, "trangThaiNhom", "IDLE");

            Debug.Log($"[PartyDataBase] ApplyFullInfo: PartyId={PartyId}, Leader={LeaderName}, Members={MemberCount}/{MaxMembers}, State={PartyState}");

            // Selected mode (optional)
            SelectedModeId = TryGetInt(msg, "idCheDoChoiNhom", -1);
            SelectedMapName = TryGetString(msg, "tenBanDoNhom", "");

            // Parse members
            Members.Clear();
            try
            {
                var arr = msg.GetArrayJson("danhSachThanhVienNhom");
                if (arr != null && arr.Count > 0)
                {
                    Debug.Log($"[PartyDataBase] Parsing {arr.Count} members");

                    for (int i = 0; i < arr.Count; i++)
                    {
                        try
                        {
                            var memberToken = arr[i];
                            var memberObj = memberToken as Newtonsoft.Json.Linq.JObject;

                            if (memberObj != null)
                            {
                                var member = new PartyMember
                                {
                                    idThanhVien = SafeGetLong(memberObj, "idThanhVien", 0),
                                    tenThanhVien = SafeGetString(memberObj, "tenThanhVien", ""),
                                    capDoThanhVien = SafeGetInt(memberObj, "capDoThanhVien", 1),
                                    anhDaiDienThanhVien = SafeGetInt(memberObj, "anhDaiDienThanhVien", 0),
                                    laTruongNhom = SafeGetBool(memberObj, "laTruongNhom", false)
                                };

                                Members.Add(member);
                                Debug.Log($"[PartyDataBase] Added member: {member.tenThanhVien} (Leader: {member.laTruongNhom})");
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"[PartyDataBase] Parse member {i} error: {e.Message}");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("[PartyDataBase] No members array or empty");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PartyDataBase] Parse members error: {e.Message}");
            }

            IsInParty = (PartyId > 0);

            Debug.Log($"[PartyDataBase] IsInParty={IsInParty}, PartyId={PartyId}");

            OnPartyChanged?.Invoke();
            if (!wasInParty && IsInParty) OnPartyCreated?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"[PartyDataBase] ApplyFullInfo error: {e}");
        }
    }

    // ========== SAFE PARSE HELPERS ==========
    private static long SafeGetLong(Newtonsoft.Json.Linq.JObject obj, string key, long defaultValue)
    {
        try
        {
            var token = obj[key];
            if (token == null) return defaultValue;
            return token.ToObject<long>();
        }
        catch
        {
            return defaultValue;
        }
    }

    private static int SafeGetInt(Newtonsoft.Json.Linq.JObject obj, string key, int defaultValue)
    {
        try
        {
            var token = obj[key];
            if (token == null) return defaultValue;
            return token.ToObject<int>();
        }
        catch
        {
            return defaultValue;
        }
    }

    private static string SafeGetString(Newtonsoft.Json.Linq.JObject obj, string key, string defaultValue)
    {
        try
        {
            var token = obj[key];
            if (token == null) return defaultValue;
            return token.ToObject<string>() ?? defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    private static bool SafeGetBool(Newtonsoft.Json.Linq.JObject obj, string key, bool defaultValue)
    {
        try
        {
            var token = obj[key];
            if (token == null) return defaultValue;
            return token.ToObject<bool>();
        }
        catch
        {
            return defaultValue;
        }
    }

    // ========== UPDATE STATE (CMD 101, 102) ==========
    public void UpdateState(string newState)
    {
        PartyState = newState;
        OnPartyChanged?.Invoke();
    }

    // ========== ADD MEMBER (CMD 99) ==========
    public void AddMember(long id, string name, int level, int avatar)
    {
        var existing = Members.Find(m => m.idThanhVien == id);
        if (existing != null) return;

        Members.Add(new PartyMember
        {
            idThanhVien = id,
            tenThanhVien = name,
            capDoThanhVien = level,
            anhDaiDienThanhVien = avatar,
            laTruongNhom = (id == LeaderId)
        });

        MemberCount = Members.Count;
        OnMemberJoined?.Invoke();
        OnPartyChanged?.Invoke();
    }

    // ========== REMOVE MEMBER (CMD 100) ==========
    public void RemoveMember(long id)
    {
        int removed = Members.RemoveAll(m => m.idThanhVien == id);
        if (removed > 0)
        {
            MemberCount = Members.Count;
            OnMemberLeft?.Invoke();
            OnPartyChanged?.Invoke();
        }
    }

    // ========== UPDATE LEADER (CMD 100) ==========
    public void UpdateLeader(long newLeaderId, string newLeaderName)
    {
        LeaderId = newLeaderId;
        LeaderName = newLeaderName;

        foreach (var m in Members)
        {
            m.laTruongNhom = (m.idThanhVien == LeaderId);
        }

        OnPartyChanged?.Invoke();
    }

    // ========== HELPERS ==========
    public bool IsLeader(long userId) => (userId == LeaderId);
    public bool IsFull() => (MemberCount >= MaxMembers);
    public bool CanStartQueue() => IsInParty && PartyState == "IDLE" && SelectedModeId > 0;

    // ========== TRY GET HELPERS ==========
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