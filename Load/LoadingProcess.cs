using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LoadingProcess : ManualSingleton<LoadingProcess>
{
//     public TextMeshProUGUI txtLoading;
//     public Image imgLoad;
//     public GameObject bgIos, bgAndroid;
//     public ThongBaoLoading thongBao;
    
//     private void Start()
//     {
//         Init();
//         Application.targetFrameRate = C.TARGET_FRAME;
//         Screen.sleepTimeout = SleepTimeout.NeverSleep;

//         if (AgentUnity.CheckNetWork())
//         {
//             StartCoroutine(SetLoading());
//             StartCoroutine(ProcessInstallGame());
//         }
//         else
//         {
//             thongBao.ShowPopupThongBao("Không thể kết nối đến máy chủ !");
//             if (_ieCheckMang != null)
//             {
//                 StopCoroutine(_ieCheckMang);
//                 _ieCheckMang = CheckMang();
//                 StartCoroutine(_ieCheckMang);
//             }
//             else
//             {
//                 _ieCheckMang = CheckMang();
//                 StartCoroutine(_ieCheckMang);
//             }
//         }
//     }

//     private IEnumerator _ieCheckMang;

//     IEnumerator CheckMang()
//     {
//         while (true)
//         {
//             if (AgentUnity.CheckNetWork())
//             {
//                 StartCoroutine(SetLoading());
//                 // StartCoroutine(ProcessInstallGame());
//                 StopCoroutine(_ieCheckMang);
//             }
//             else
//             {
//                 thongBao.ShowPopupThongBao("Không thể kết nối đến máy chủ !");
//             }

//             yield return new WaitForSeconds(2);
//         }
//     }

//     private void Update()
//     {
//         if (Input.GetKeyUp(KeyCode.Escape))
//         {
//             // Application.Quit();
//         }
//     }

//     private bool _isReadDataSuccess = false;
//     private int _numberConnect = 0;

//     private void Init()
//     {
// #if UNITY_IOS
//         bgIos.gameObject.SetActive(true);
//         bgAndroid.gameObject.SetActive(false);
// #else
//         bgIos.gameObject.SetActive(false);
//         bgAndroid.gameObject.SetActive(true);
// #endif
//         if (!AgentUnity.HasKey(KeyLocalSave.PP_FIRST_INSTALL_GAME))
//         {
//             // lần đầu cài game
//             AgentUnity.SetInt(KeyLocalSave.PP_FIRST_INSTALL_GAME, C.ONE);
//             AgentUnity.SetInt(KeyLocalSave.PP_AudioBg, 1);
//             AgentUnity.SetInt(KeyLocalSave.PP_AudioSound, 1);
//             AgentUnity.SetInt(KeyLocalSave.PP_CheDoRung, 1);
//             AgentUnity.SetInt(KeyLocalSave.PP_SavePassword, 1);
//             AgentUnity.SetInt(KeyLocalSave.PP_ID_LANGUAGE, C.IdTiengViet);
//         }
//     }

//     // private IEnumerator ProcessInstallGame()
//     // {
//     //     _numberConnect++;
//     //     string serverIp = AgentUnity.GetLocalIPAddress();
//     //     ApiSend re = new ApiSend(CMDApi.API);
//     //     try
//     //     {
//     //         re.Put("uuid",AgentUnity.GetUUID());
//     //         re.Put("platform", CMD.TYPE_FLATFORM);
//     //         re.Put("provider", CMD.PROVIDER_ID);
//     //         re.Put("imei", AgentUnity.GetImeiDevice());
//     //         re.Put("macAddress",AgentUnity.GetMacAddress());
//     //         re.Put("ip", serverIp);
//     //         re.Put("deviceName", SystemInfo.deviceName);
//     //     }
//     //     catch (Exception e)
//     //     {
//     //         AgentUnity.LogError(e);
//     //     }
        
//     //     UnityWebRequest www = AgentUnity.GetHttpPost2(CMDApi.LINK_GATEWAY, re.GetJson());
//     //     // www.timeout = 10; // Kết nối sau 10 giây
//     //     yield return www.SendWebRequest();

//     //     try
//     //     {
//     //         if (www.isNetworkError)
//     //         {
//     //             AgentUnity.LogError(www.error);
//     //             SuperDialog.Instance.DialogMatKetNoi.ShowMessage("Thông báo",
//     //                 "Mạng đang bị lỗi, bạn vui lòng kiểm tra lại kết nối Wifi hoặc 3G/4G", "Thử lại");
//     //             AutoConnect();
//     //         }
//     //         else if (www.isHttpError)
//     //         {
//     //             AgentUnity.LogError(www.error);
//     //         }
//     //         else
//     //         {
//     //             if (OnOffDialog.Instance.isOnDialogMatKetNoi)
//     //             {
//     //                 // nếu đang hiển thị dialog thông báo mạng lỗi thì tắt đi
//     //                 SuperDialog.Instance.DialogMatKetNoi.Show(false);
//     //             }

//     //             JObjectCustom j = JObjectCustom.From(www.downloadHandler.text);
//     //             AgentUnity.LogError("j = " + www.downloadHandler.text);
//     //             CMDApi.LINK_GATEWAY_REGISTER = j.GetString("urlClientRegister");
//     //             CMDApi.LINK_GATEWAY_LOGIN = j.GetString("urlClientLogin");
//     //             CMDApi.LINK_GATEWAY_GET_TEXT = j.GetString("urlClientGetText");
//     //             CMDApi.LINK_GATEWAY_GET_THONG_TIN = j.GetString("urlClientGetAccount");
//     //             CMDApi.LINK_GATEWAY_UPDATE_THONG_TIN = j.GetString("urlClientUpdateAccount");
//     //             CMDApi.LINK_GATEWAY_PAY_INAPP = j.GetString("urlClientPayInapp");
//     //             CMDApi.LINK_GATEWAY_PAY_INAPP_ANDROID = j.GetString("urlClientPayInappAndroid");
//     //             B.Instance.loginBannerLink = j.GetString("linkImgBanner");
//     //             B.Instance.linkBannerGame = j.GetString("linkBanner");
//     //             B.Instance.loginLinkFanpage = j.GetString("linkFanpage");
//     //             B.Instance.loginLinkGroup = j.GetString("linkGroup");
//     //             B.Instance.loginHotline = j.GetString("hotline");
//     //             B.Instance.loginThongBao = j.GetString("thongBao");
//     //             B.Instance.loginLinkWebGame = j.GetString("linkWebGame");
//     //             B.Instance.loginEmail = j.GetString("email");
//     //             B.Instance.linkUpdateThongTinCaNhan = j.GetString("linkUpdateUser");
//     //             B.Instance.linkDoiMatKhau = j.GetString("linkChangePassword");
//     //             B.Instance.loginLinkVote5Sao = j.GetString("linkVote");
                
//     //             B.Instance.ShowIcon18 = j.GetInt("showIcon");
//     //             B.Instance.thoiTiet = j.GetInt("thoitiet");
//     //             B.Instance.linkPing = j.GetString("linksCheckInternet");
//     //             _isReadDataSuccess = true;
//     //         }
//     //     }
//     //     catch (Exception e)
//     //     {
//     //         AgentUnity.LogError(e);
//     //     }

//     //     if (!_isReadDataSuccess)
//     //     {
//     //         // nếu chưa có dữ liệu thì lấy lại
//     //         if (_numberConnect <= 3)
//     //         {
//     //             StartCoroutine(ProcessInstallGame());
//     //         }
//     //     }
//     // }
    
//     private IEnumerator _ieAutoConnect = null;

//     private void AutoConnect()
//     {
//         if (_ieAutoConnect != null)
//         {
//             StopCoroutine(_ieAutoConnect);
//             _ieAutoConnect = IeAutoConnect();
//             StartCoroutine(_ieAutoConnect);
//         }
//         else
//         {
//             _ieAutoConnect = IeAutoConnect();
//             StartCoroutine(_ieAutoConnect);
//         }
//     }

//     private IEnumerator IeAutoConnect()
//     {
//         yield return new WaitForSeconds(3);
//         StartCoroutine(ProcessInstallGame());
//         yield return null;
//     }
    

//     private IEnumerator SetLoading()
//     {
//         int t = 0;
//         imgLoad.fillAmount = 0;

//         while (t < 100)
//         {
//             t++;
//             imgLoad.fillAmount += 0.01f;
//             txtLoading.text = "Loading " + t + "%";
//             yield return new WaitForSeconds(0.3f);
//             if (_isReadDataSuccess)
//             {
//                 txtLoading.text = "Loading " + 100 + "%";
//                 imgLoad.fillAmount = 1;
//                 yield return new WaitForSeconds(1f);
//                 t += 100;
//             }
//         }

//         t = 0;
//         while (t < 100)
//         {
//             t++;
//             yield return new WaitForSeconds(1f);
//             if (_isReadDataSuccess)
//             {
//                 t += 100;
//             }
//         }

//         SceneManager.LoadScene(MoveScene.MAIN_GAME);
//     }
}