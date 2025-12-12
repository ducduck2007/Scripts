using System;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class AgentUnity : MonoBehaviour
{
    internal static string GetImeiDevice()
    {
        string imei = "";
        imei = SystemInfo.deviceUniqueIdentifier;
        return imei;
    }

    // Định dạng GUID chuẩn (dùng dấu gạch nối)
    //Console.WriteLine(newGuid.ToString("D"));  // d9b2d63d-4f56-4191-9bcf-5b7c6e7a2b77
    // Định dạng không có dấu gạch nối
    // Console.WriteLine(newGuid.ToString("N"));  // d9b2d63d4f5641919bcf5b7c6e7a2b77
    // Định dạng dạng UPPERCASE
    //Console.WriteLine(newGuid.ToString("B").ToUpper());  // {D9B2D63D-4F56-4191-9BCF-5B7C6E7A2B77}
    // internal static string GetUUID()
    // {
    //     if (HasKey(KeyLocalSave.PP_UUID))
    //     {
    //         // Lấy UUID đã lưu
    //         return GetString(KeyLocalSave.PP_UUID);
    //     }

    //     // Tạo UUID mới nếu chưa có
    //     string newUuid = Guid.NewGuid().ToString("N");
    //     SetString(KeyLocalSave.PP_UUID, newUuid); // Lưu UUID vào PlayerPrefs
    //     return newUuid;
    // }

    public static string GetMacAddress()
    {
        string macAddresses = "";
        foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.NetworkInterfaceType != NetworkInterfaceType.Ethernet) continue;
            if (nic.OperationalStatus == OperationalStatus.Up)
            {
                macAddresses += nic.GetPhysicalAddress().ToString();
                break;
            }
        }

        return macAddresses;
    }

    internal static string GetAndroidDeviceIdentifier()
    {
        string androidId = "";
        Debug.LogError("1");
        if (Application.platform == RuntimePlatform.Android)
        {
            Debug.LogError("2");
            using (AndroidJavaClass androidSettings = new AndroidJavaClass("android.provider.Settings$Secure"))
            {
                Debug.LogError("3");
                androidId = androidSettings.CallStatic<string>("getString", new AndroidJavaObject("android.content.ContentResolver"), "android_id");
                Debug.Log("Android ID: " + androidId);
            }
        }

        return androidId;
    }

    public static string GetLocalIPAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
        }
        catch (SocketException ex)
        {
            UnityEngine.Debug.LogError("Lỗi DNS: " + ex.Message);
        }

        return "127.0.0.1"; // fallback tạm thời
    }

    public static string GetGlobalIPAddress()
    {
        string myAddressGlobal = "";
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ipify.org");
        request.Method = "GET";
        request.Timeout = 5000; //time in ms
        try
        {
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                myAddressGlobal = reader.ReadToEnd();
            } //if
            else
            {
                //                LogError("Timed out? "+response.StatusDescription);
                // myAddressGlobal="127.0.0.1";
            } //else
        } //try
        catch (WebException ex)
        {
            //            Log("Likely no internet connection: "+ex.Message);
            // myAddressGlobal="127.0.0.1";
        }

        return myAddressGlobal;
    }

    /// <summary>
    /// Chuyển scene
    /// </summary>
    internal static void LoadScene(string nameScene)
    {
        SceneManager.LoadScene(nameScene);
    }

    /// <summary>
    /// LOG tất cả giá trị của 1 class
    /// </summary>
    /// <param name="obj"></param>
    public static void DebugFullClass(object obj)
    {
        string log = ObjectDumper.Dump(obj);
        Debug.LogError("CLASS : " + obj.GetType() + ".cs" + "\n" + log);
    }

    internal static Color GetColor(float _r, float _g, float _b)
    {
        Color color = Color.green;
        float max = 255F;
        float r = _r / max;
        float g = _g / max;
        float b = _b / max;
        float a = 1F;
        color = new Color(r, g, b, a);
        return color;
    }

    internal static Color GetColor(float _r, float _g, float _b, float _a)
    {
        Color color = Color.green;
        float max = 255F;
        float r = _r / max;
        float g = _g / max;
        float b = _b / max;

        color = new Color(r, g, b, _a);
        return color;
    }


    internal static void DestroyAllChilds(Transform parent)
    {
        foreach (Transform t in parent)
        {
            Object.Destroy(t.gameObject);
        }
    }

    internal static void DisableAllChilds(Transform parent)
    {
        foreach (Transform t in parent)
        {
            t.gameObject.SetActive(false);
        }
    }

    internal static void EnableAllChilds(Transform parent)
    {
        foreach (Transform t in parent)
        {
            t.gameObject.SetActive(true);
        }
    }

    #region DOTween

    private static float TIME_CLICK = 0.25F;

    /// <summary>
    /// Hiệu ứng nhấn nút
    /// </summary>
    private static void ReverseScale(Transform obj)
    {
        obj.DOScale(Vector3.one, TIME_CLICK);
    }

    private const float TIME_MOVE_TRANFORM = 0.4F;

    internal static void DoLocalMove(Transform tran, Vector3 target, float timeMove,
        UnityEngine.Events.UnityAction callBack = null)
    {
        if (callBack == null)
            tran.DOLocalMove(target, timeMove);
        else tran.DOLocalMove(target, timeMove).OnComplete(delegate { callBack.Invoke(); });
    }

    internal static void DoLocalMove(Transform tran, Vector3 target, float timeMove, float timeDelay,
        UnityEngine.Events.UnityAction callBack = null)
    {
        if (callBack == null)
            tran.DOLocalMove(target, timeMove).SetDelay(timeDelay);
        else tran.DOLocalMove(target, timeMove).OnComplete(delegate { callBack.Invoke(); });
    }

    internal static void DoLocalMove(Transform tran, Vector3 target, float timeMove)
    {
        tran.DOLocalMove(target, timeMove);
    }

    internal static void DoLocalMove(Transform tran, Vector3 target, float timeMove, int loop, LoopType loopType,
        Ease ease)
    {
        tran.DOLocalMove(target, timeMove).SetLoops(loop, loopType).SetEase(ease);
    }

    internal static void DoLocalAndScale(Transform tran, Vector3 target, Vector3 scale, float timeMove)
    {
        tran.DOLocalMove(target, timeMove);
        tran.DOScale(scale, timeMove);
    }

    #endregion

    /// <summary>
    /// Gọi trong hàm StartCoroutine()
    /// </summary>
    internal static IEnumerator DisableObject(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        obj.SetActive(false);
    }

    /// <summary>
    /// Rung theo theo số seconds
    /// </summary>
    /// <param name="time"></param>
    internal static void Vibrate()
    {
        Handheld.Vibrate();
    }

    public static bool CheckNetWork()
    {
        bool isNetwork = true;
        if (!CheckInternetConnection("https://www.google.com.vn"))
        {
            isNetwork = false;
            // ShowNetworkNotification(IdLanguage.KhongKetNoiMayChu);
        }

        Log("isNetwork : " + isNetwork);
        return isNetwork;
    }

    internal static void Log(object obj)
    {
#if UNITY_EDITOR
        Debug.Log(obj);
#endif
    }

    internal static void LogWarning(object obj)
    {
#if UNITY_EDITOR
        Debug.LogWarning(obj);
#endif
    }

    internal static void LogError(object obj)
    {
#if UNITY_EDITOR
        Debug.LogError(obj);
#endif
    }

    internal static GameObject InstanceObject(GameObject prefab, Transform parent)
    {
        GameObject tmp = Instantiate(prefab, parent);
        tmp.transform.localPosition = Vector3.zero;
        tmp.transform.localScale = Vector3.one;
        return tmp;
    }

    internal static T InstanceCayTrong<T>(GameObject prefab, Transform parent)
    {
        GameObject tmp = Instantiate(prefab, parent);
        tmp.transform.localPosition = Vector3.zero;
        tmp.transform.localScale = new Vector3(0.65f, 0.65f);
        return tmp.GetComponent<T>();
    }

    internal static GameObject InstanceObject(GameObject prefab, Transform parent, Vector3 position)
    {
        GameObject tmp = Object.Instantiate(prefab);
        tmp.transform.SetParent(parent);
        tmp.transform.localPosition = position;
        tmp.transform.localScale = Vector3.one;
        return tmp;
    }

    internal static T InstanceObject<T>(GameObject prefab, Transform parent)
    {
        GameObject tmp = Object.Instantiate(prefab);
        tmp.transform.SetParent(parent);
        tmp.transform.localPosition = Vector3.zero;
        tmp.transform.localScale = Vector3.one;
        return tmp.GetComponent<T>();
    }

    internal static T InstanceObject<T>(MonoBehaviour prefab, Transform parent)
    {
        GameObject tmp = Object.Instantiate(prefab.gameObject);
        tmp.transform.SetParent(parent);
        tmp.transform.localPosition = Vector3.zero;
        tmp.transform.localScale = Vector3.one;
        return tmp.GetComponent<T>();
    }

    /* Lấy thông tin đối tượng chạm trên màn hình */
    internal static GameObject TouchInfo(Camera camera, Vector3 mousePosition)
    {
        Vector3 wp = camera.ScreenToWorldPoint(mousePosition);
        Vector2 touchPos = new Vector2(wp.x, wp.y);
        GameObject ObjPointer = null;
        if (Physics2D.OverlapPoint(touchPos)) //OverlapPoint: trả về đối tượng va chạm
            ObjPointer = Physics2D.OverlapPoint(touchPos).gameObject;

        return ObjPointer;
    }

    internal static GameObject TouchInfo(Camera camera, Vector3 mousePos, float zCamera)
    {
        mousePos.z = zCamera;
        Vector3 screenPos = camera.ScreenToWorldPoint(mousePos);
        RaycastHit2D hit = Physics2D.Raycast(screenPos, Vector2.zero);
        if (hit)
        {
            return hit.collider.gameObject;
        }

        return null;
    }


    #region PlayerPrefs

    internal static bool HasKey(string key)
    {
        return PlayerPrefs.HasKey(key);
    }

    internal static void SetString(string key, string val)
    {
        PlayerPrefs.SetString(key, val);
        PlayerPrefs.Save();
    }

    internal static void SetInt(string key, int val)
    {
        PlayerPrefs.SetInt(key, val);
        PlayerPrefs.Save();
    }

    internal static void SetFloat(string key, float val)
    {
        PlayerPrefs.SetFloat(key, val);
        PlayerPrefs.Save();
    }

    internal static string GetString(string key, string defaultValue = "")
    {
        return PlayerPrefs.GetString(key, defaultValue);
    }

    internal static int GetInt(string key, int defaultValue = 0)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }

    internal static float GetFloat(string key, float defaultValue = 0F)
    {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }

    internal static void DeleteKey(string key)
    {
        PlayerPrefs.DeleteKey(key);
    }

    internal static void DeleteAllKey()
    {
        PlayerPrefs.DeleteAll();
    }

    public static void WriteFile(string fileName, string jsonString)
    {
#if UNITY_ANDROID
        string path = Application.persistentDataPath + fileName;
        File.WriteAllText(path, jsonString);
#elif UNITY_IOS
        string path = Application.persistentDataPath + fileName;
        File.WriteAllText(path, jsonString);
#elif UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh ();
#endif
        //Debug.LogError("---WriteFile--- fileName = " + fileName + ", jsonString = " + File.ReadAllText(path).Length);
    }

    public static string ReadFile(string fileName)
    {
        string path = Application.persistentDataPath + fileName;
        string jsonString = "";
        try
        {
            jsonString = File.ReadAllText(path);
        }
        catch (Exception e)
        {
            // Console.WriteLine(e);
            // throw;
        }

        //Debug.LogError("---READFILE--- fileName = " + fileName + ", jsonString = " + jsonString.Length);
        return jsonString;
    }

    #endregion

    internal static void SetNameButton(Button button, string name)
    {
        button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = name;
    }

    internal static void SetNameToggle(Toggle toggle, string name)
    {
        toggle.GetComponentInChildren<Text>().text = name;
    }

    internal static float AspectRatioSceen()
    {
        if (Screen.width > Screen.height)
            return (float)Screen.width / (float)Screen.height;
        else return (float)Screen.height / (float)Screen.width;
    }

    internal static void SetPositionGameObjectUI(Transform tran, Vector3 newPosition, bool isDoMove,
        float duration = 0.6f)
    {
        newPosition.z = 0;
        if (isDoMove) tran.GetComponent<RectTransform>().DOLocalMove(newPosition, duration);
        else tran.GetComponent<RectTransform>().anchoredPosition = newPosition;
    }

    internal static UnityWebRequest GetHttpPost(string url, string jsonData)
    {
        UnityWebRequest request = UnityWebRequest.PostWwwForm(url, "POST");
        request.useHttpContinue = false;
        request.chunkedTransfer = false;
        //       Debug.LogWarning("Khởi tạo kết nối HTTPpost");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        return request;
    }

    internal static UnityWebRequest GetHttpPost2(string url, string jsonData)
    {
        UnityWebRequest request = UnityWebRequest.PostWwwForm(url, "POST");
        request.useHttpContinue = false;
        request.chunkedTransfer = false;
        //       Debug.LogWarning("Khởi tạo kết nối HTTPpost");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "text/html");
        return request;
    }

    internal static UnityWebRequest GetHttpPost3(string url, string jsonData)
    {
        UnityWebRequest request = UnityWebRequest.PostWwwForm(url, "POST");
        request.useHttpContinue = false;
        request.chunkedTransfer = false;
        //       Debug.LogWarning("Khởi tạo kết nối HTTPpost");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "text/html");
        return request;
    }

    public static void SetNewListenerButton(Button btn, Action onClick)
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onClick());
    }

    public static void SetNewListenerToggle(Toggle toggle, Action onClick)
    {
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener(arg0 => onClick());
    }

    public static void SetImgSize(Image img, float width, float height)
    {
        img.rectTransform.sizeDelta = new Vector2(width, height);
    }

    public static void SetGameObjSize(GameObject gO, float width, float height)
    {
        gO.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
    }

    public static void SetDisableToggle(Toggle tg)
    {
        var newColorBlock = tg.colors;
        newColorBlock.disabledColor = Color.white;
        tg.interactable = false;
        tg.colors = newColorBlock;
    }

    private static string GetHtmlFromUri(string resource)
    {
        string html = string.Empty;
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(resource);
        try
        {
            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            {
                bool isSuccess = (int)resp.StatusCode < 299 && (int)resp.StatusCode >= 200;
                if (isSuccess)
                {
                    using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                    {
                        //We are limiting the array to 80 so we don't have
                        //to parse the entire html document feel free to 
                        //adjust (probably stay under 300)
                        char[] cs = new char[80];
                        reader.Read(cs, 0, cs.Length);
                        foreach (char ch in cs)
                        {
                            html += ch;
                        }
                    }
                }
            }
        }
        catch
        {
            return "";
        }

        return html;
    }

    public static bool CheckInternetConnection(string URL)
    {
        string HtmlText = GetHtmlFromUri(URL);
        if (HtmlText.Length > 0)
        {
            //Ok internet 
            return true;
        }

        return false;
    }

    public static void ShowNetworkNotification(string msg)
    {
        // if (!SuperDialog.Instance.PopupOneButton.gameObject.activeInHierarchy)
        // {
        //     SuperDialog.Instance.PopupOneButton.ShowPopupOneButton(Res.TITLE_THONG_BAO, msg,
        //         () =>
        //         {
        //             if (msg == LoginController.ERROR_1 || msg == LoginController.ERROR_2)
        //             {
        //                 // LoadScene(MoveScene.MAIN_GAME);
        //                 Resources.UnloadUnusedAssets();
        //                 SoundBG.Instance.PlayLoginGameBG();
        //             }
        //         }, 30);
        // }
    }




    public static string GetDeviceID()
    {
        // Get Android ID
        AndroidJavaClass clsUnity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject objActivity = clsUnity.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject objResolver = objActivity.Call<AndroidJavaObject>("getContentResolver");
        AndroidJavaClass clsSecure = new AndroidJavaClass("android.provider.Settings$Secure");
        string android_id = clsSecure.CallStatic<string>("getString", objResolver, "android_id");

        // Get bytes of Android ID
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = ue.GetBytes(android_id);

        // Encrypt bytes with md5
        System.Security.Cryptography.MD5CryptoServiceProvider md5 =
            new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);

        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }

        string device_id = hashString.PadLeft(32, '0');

        return device_id;
    }

    public static void ScaleBg(GameObject bg)
    {
        bg.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);
    }

    public static void ScaleTranform(Transform tranScale)
    {
        float scale = (float)Screen.width / 1952;
        if ((float)Screen.height / 900 < scale)
        {
            scale = (float)Screen.height / 900;
        }
        tranScale.transform.localScale = new Vector3(scale, scale);
    }

    public static void ScaleContent(Transform contentScale)
    {
        float scale1 = (float)Screen.width / 1952;
        if ((float)Screen.height / 900 > scale1)
        {
            scale1 = (float)Screen.height / 900;
        }
        contentScale.transform.localScale = new Vector3(scale1, scale1);
    }

    public static List<int> GetListByString(string chuoi)
    {
        List<int> list = new List<int>();
        string[] arrListStr = chuoi.Split(',');
        for (int i = 0; i < arrListStr.Length; i++)
        {
            if (arrListStr[i].Length > 0)
            {
                list.Add(int.Parse(arrListStr[i]));
            }
        }

        return list;
    }

    public static List<string> GetListStrByString(string chuoi)
    {
        List<string> list = new List<string>();
        string[] arrListStr = chuoi.Split(',');
        for (int i = 0; i < arrListStr.Length; i++)
        {
            if (arrListStr[i].Length > 0)
            {
                list.Add(arrListStr[i]);
            }
        }

        return list;
    }


}