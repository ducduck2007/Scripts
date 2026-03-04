using System.Collections;
using UnityEngine;
using Ping = UnityEngine.Ping;

public class PingPongGame : ManualSingleton<PingPongGame>
{
    private IEnumerator _enumConnection;

    public float pingTime;

    private int _demMangKhongOnDinh = 0;
    private const int NGUONG_KHONG_ON_DINH = 3;
    private const int NGUONG_PING_TOT = 70;       // <70ms: mạng tốt
    private const int NGUONG_PING_YEU = 150;      // >150ms: mạng yếu, cảnh báo ngay

    private float _lastWarningKhongOnDinhTime = -180f;
    private const float COOLDOWN_WARNING = 180f; // 3 phút cho mức không ổn định

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
                    _demMangKhongOnDinh = 0;
                    B.Instance.DemMangYeu = 0;

                    ThongBaoController.Instance.PopupOneButton.ShowPopupThongBao(
                        "Kết nối mạng kém. Bạn vui lòng kiểm tra lại kết nối mạng Wifi/5G."
                    );
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
                        B.Instance.DemMangYeu = 0;

                        ThongBaoController.Instance.PopupOneButton.ShowPopupThongBao(
                            "Kết nối mạng kém. Bạn vui lòng kiểm tra lại kết nối mạng Wifi/5G."
                        );

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