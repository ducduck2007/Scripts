using System.Collections;
using UnityEngine;
using Ping = UnityEngine.Ping;

public class PingPongGame : ManualSingleton<PingPongGame>
{
    private IEnumerator _enumConnection;

    public float pingTime;

    private int _demMangKhongOnDinh = 0;
    private const int NGUONG_KHONG_ON_DINH = 3;
    private const int NGUONG_PING_KHONG_ON_DINH = 90;
    private const int NGUONG_PING_YEU = 190;

    private float _lastWarningKhongOnDinhTime = -180f;
    private const float COOLDOWN_WARNING = 180f; // 3 phút

    public void PingPong()
    {
        if (_enumConnection != null)
        {
            StopCoroutine(_enumConnection);
        }
        _enumConnection = CheckConnection();
        StartCoroutine(_enumConnection);
    }

    public void StopPingPong()
    {
        if (_enumConnection != null)
        {
            StopCoroutine(_enumConnection);
            _enumConnection = null;
        }
    }

    IEnumerator CheckConnection()
    {
        while (true)
        {
            var ping = new Ping("149.28.152.90");
            yield return new WaitForSeconds(2);

            if (ping.isDone)
            {
                if (ping.time < 0)
                    pingTime = 9999;
                else
                    pingTime = ping.time;

                if (pingTime < 9999 && OnOffDialog.Instance.isOnLoadMang)
                    ThongBaoController.Instance.LoadMang.Show(false);

                if (pingTime > NGUONG_PING_YEU)
                {
                    B.Instance.DemMangYeu++;
                    _demMangKhongOnDinh = 0;

                    if (B.Instance.DemMangYeu >= 5)
                    {
                        ThongBaoController.Instance.PopupOneButton.ShowPopupThongBao(
                            "Kết nối mạng kém. Bạn vui lòng kiểm tra lại kết nối mạng Wifi/5G."
                        );
                        B.Instance.DemMangYeu = 0;
                    }
                }
                else if (pingTime > NGUONG_PING_KHONG_ON_DINH)
                {
                    B.Instance.DemMangYeu = 0;
                    _demMangKhongOnDinh++;

                    if (_demMangKhongOnDinh >= NGUONG_KHONG_ON_DINH)
                    {
                        if (Time.time - _lastWarningKhongOnDinhTime >= COOLDOWN_WARNING)
                        {
                            ThongBaoController.Instance.PopupOneButton.ShowPopupThongBao(
                                "Mạng không ổn định. Bạn vui lòng kiểm tra lại kết nối."
                            );
                            _lastWarningKhongOnDinhTime = Time.time;
                        }
                        _demMangKhongOnDinh = 0;
                    }
                }
                else
                {
                    B.Instance.DemMangYeu = 0;
                    _demMangKhongOnDinh = 0;
                }
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    if (ping.isDone)
                    {
                        if (OnOffDialog.Instance.isOnLoadMang)
                            ThongBaoController.Instance.LoadMang.Show(false);
                        break;
                    }
                    else if (i == 9)
                    {
                        pingTime = 9999;
                        B.Instance.DemMangYeu++;

                        if (B.Instance.DemMangYeu >= 5)
                        {
                            ThongBaoController.Instance.PopupOneButton.ShowPopupThongBao(
                                "Kết nối mạng kém. Bạn vui lòng kiểm tra lại kết nối mạng Wifi/5G."
                            );
                            B.Instance.DemMangYeu = 0;
                        }

                        if (!OnOffDialog.Instance.isOnLoadMang)
                            ThongBaoController.Instance.LoadMang.Show();
                    }
                    yield return new WaitForSeconds(0.3f);
                }
            }

            yield return new WaitForSeconds(2);
        }
    }

    public void OnDisable()
    {
        StopPingPong();
    }
}