using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

[Serializable]
public class ItemInfoData
{
    public int idItem;
    public string nameItem;
    public string moTaNgan;
    public string moTa;

    public int tier;
    public int giaMua;

    public int dmgVatLy, dmgPhep;
    public int giap, khangPhep;
    public int mauToiDa, manaToiDa;

    public int tocDanh, tocChay;
    public int chiMang;

    public int hutMau, hutMauPhep;
    public int xuyenGiap, xuyenKhangPhep;

    public int hoiMauGiay, hoiManaGiay;
    public int giamHoiChieu;

    public int tamDanh;
    public int hasRecipe;
}

public static class ItemInfoCache
{
    private static readonly object _lock = new object();
    private static readonly Dictionary<int, ItemInfoData> _byId = new Dictionary<int, ItemInfoData>(256);

    public static int Version { get; private set; } = 0;
    public static event Action OnUpdated;

    private static float _lastRequestTime = -999f;
    private static bool _hasEverRequested = false;

    private const float REQUEST_COOLDOWN_SECONDS = 10f;

    private static bool _diskLoaded = false;
    private static string CachePath => Path.Combine(Application.persistentDataPath, "item_info_cache.json");

    public static void EnsureDiskLoaded(bool debugLog = false)
    {
        if (_diskLoaded) return;
        _diskLoaded = true;
        TryLoadFromDisk(debugLog);
    }

    public static void EnsureRequested(Action sendRequest, bool force = false, bool debugLog = false)
    {
        EnsureDiskLoaded(debugLog);

        if (sendRequest == null) return;

        float now = Time.realtimeSinceStartup;

        bool hasData = Count > 0;

        if (!force)
        {
            if (_hasEverRequested && (now - _lastRequestTime) < REQUEST_COOLDOWN_SECONDS)
            {
                if (debugLog) Debug.Log($"[ItemInfoCache] EnsureRequested skip (cooldown) hasData={hasData} count={Count}");
                return;
            }
        }

        _hasEverRequested = true;
        _lastRequestTime = now;

        if (debugLog) Debug.Log($"[ItemInfoCache] EnsureRequested SEND force={force} hasData={hasData} count={Count}");
        sendRequest.Invoke();
    }

    public static int Count
    {
        get { lock (_lock) return _byId.Count; }
    }

    public static List<ItemInfoData> GetAllSorted()
    {
        EnsureDiskLoaded(false);

        lock (_lock)
        {
            var list = new List<ItemInfoData>(_byId.Values);
            list.Sort((a, b) =>
            {
                int t = a.tier.CompareTo(b.tier);
                if (t != 0) return t;
                return a.idItem.CompareTo(b.idItem);
            });
            return list;
        }
    }

    public static void PutFromMsg(JArray arr, bool debugLog = true)
    {
        EnsureDiskLoaded(debugLog);

        if (arr == null)
        {
            if (debugLog) Debug.LogWarning("[ItemInfoCache] PutFromMsg arr=null");
            return;
        }

        int added = 0;
        int updated = 0;

        lock (_lock)
        {
            for (int i = 0; i < arr.Count; i++)
            {
                if (!(arr[i] is JObject o)) continue;

                var d = ParseOne(o);
                if (d == null || d.idItem <= 0) continue;

                if (_byId.ContainsKey(d.idItem)) { _byId[d.idItem] = d; updated++; }
                else { _byId.Add(d.idItem, d); added++; }
            }

            Version++;
        }

        TrySaveToDisk(arr, debugLog);

        if (debugLog) Debug.Log($"[ItemInfoCache] updated Version={Version} added={added} updated={updated} total={Count}");
        OnUpdated?.Invoke();
    }

    private static ItemInfoData ParseOne(JObject o)
    {
        int I(string k)
        {
            if (!o.TryGetValue(k, out var v) || v.Type == JTokenType.Null) return 0;
            try { return v.Value<int>(); } catch { return 0; }
        }

        string S(string k)
        {
            if (!o.TryGetValue(k, out var v) || v.Type == JTokenType.Null) return "";
            try { return v.Value<string>() ?? ""; } catch { return ""; }
        }

        return new ItemInfoData
        {
            idItem = I("idItem"),
            nameItem = S("nameItem"),
            moTaNgan = S("moTaNgan"),
            moTa = S("moTa"),

            tier = I("tier"),
            giaMua = I("giaMua"),

            dmgVatLy = I("dmgVatLy"),
            dmgPhep = I("dmgPhep"),
            giap = I("giap"),
            khangPhep = I("khangPhep"),

            mauToiDa = I("mauToiDa"),
            manaToiDa = I("manaToiDa"),

            tocDanh = I("tocDanh"),
            tocChay = I("tocChay"),
            chiMang = I("chiMang"),

            hutMau = I("hutMau"),
            hutMauPhep = I("hutMauPhep"),

            xuyenGiap = I("xuyenGiap"),
            xuyenKhangPhep = I("xuyenKhangPhep"),

            hoiMauGiay = I("hoiMauGiay"),
            hoiManaGiay = I("hoiManaGiay"),
            giamHoiChieu = I("giamHoiChieu"),

            tamDanh = I("tamDanh"),
            hasRecipe = I("hasRecipe"),
        };
    }

    private static void TryLoadFromDisk(bool debugLog)
    {
        try
        {
            if (!File.Exists(CachePath))
            {
                if (debugLog) Debug.Log($"[ItemInfoCache] No disk cache at {CachePath}");
                return;
            }

            string json = File.ReadAllText(CachePath);
            if (string.IsNullOrWhiteSpace(json)) return;

            var root = JObject.Parse(json);
            var arr = root["item"] as JArray;
            if (arr == null)
            {
                if (debugLog) Debug.LogWarning("[ItemInfoCache] Disk cache missing key 'item'");
                return;
            }

            int added = 0;
            int updated = 0;

            lock (_lock)
            {
                for (int i = 0; i < arr.Count; i++)
                {
                    if (!(arr[i] is JObject o)) continue;

                    var d = ParseOne(o);
                    if (d == null || d.idItem <= 0) continue;

                    if (_byId.ContainsKey(d.idItem)) { _byId[d.idItem] = d; updated++; }
                    else { _byId.Add(d.idItem, d); added++; }
                }

                Version++;
            }

            if (debugLog) Debug.Log($"[ItemInfoCache] Loaded disk cache added={added} updated={updated} total={Count} Version={Version}");
            OnUpdated?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[ItemInfoCache] TryLoadFromDisk error: {e.Message}");
        }
    }

    private static void TrySaveToDisk(JArray arr, bool debugLog)
    {
        try
        {
            var root = new JObject
            {
                ["savedAtUtc"] = DateTime.UtcNow.ToString("o"),
                ["item"] = arr
            };

            string json = root.ToString(Formatting.None);
            File.WriteAllText(CachePath, json);

            if (debugLog) Debug.Log($"[ItemInfoCache] Saved disk cache -> {CachePath}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[ItemInfoCache] TrySaveToDisk error: {e.Message}");
        }
    }

    public static bool TryGet(int id, out ItemInfoData d)
    {
        EnsureDiskLoaded(false);
        lock (_lock) return _byId.TryGetValue(id, out d);
    }
}
