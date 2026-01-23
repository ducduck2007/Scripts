using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using UnityEngine;
using Ping = UnityEngine.Ping;

public class PingPongGame : ManualSingleton<PingPongGame>
{
    public void PingPong ()
    {
        if (_enumConnection != null)
        {
            StopCoroutine (_enumConnection);
            _enumConnection = CheckConnection();
            StartCoroutine(_enumConnection);
        }
        else
        {
            _enumConnection = CheckConnection();
            StartCoroutine(_enumConnection);
        }
    }
    
    public void StopPingPong ()
    {
        if (_enumConnection != null)
        {
            StopCoroutine (_enumConnection);
        }
    }

    private IEnumerator _enumConnection;

    public float pingTime;

    IEnumerator CheckConnection()
    {
        const float timeout = 5f;
        float startTime = Time.timeSinceLevelLoad;
        while (true)
        {
            var ping = new Ping(B.Instance.linkPing);
            yield return new WaitForSeconds(2);
            if (ping.isDone)
            {
                pingTime = ping.time;
                if (OnOffDialog.Instance.isOnLoadMang)
                {
                    ThongBaoController.Instance.LoadMang.Show(false);
                }
                if (ping.time < 200)
                {
                    B.Instance.DemMangYeu = 0;
                }
                else
                {
                    B.Instance.DemMangYeu++;
                    // if (DemTimeControl.Instance.GetTimeDlBaoMangYeu() <= 0)
                    {
                        if (B.Instance.DemMangYeu >= 5)
                        {
                            ThongBaoController.Instance.PopupOneButton.ShowPopupThongBao("Kết nối mạng kém. Bạn vui lòng kiểm tra lại kết nối mạng Wifi/3G/4G.");
                            B.Instance.DemMangYeu = 0;
                            // DemTimeControl.Instance.StartDemTimeDlBaoMangYeu();
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    if (ping.isDone)
                    {
                        if (OnOffDialog.Instance.isOnLoadMang)
                        {
                            ThongBaoController.Instance.LoadMang.Show(false);
                        }
                    }
                    else
                    {
                        if (i == 9)
                        {
                            if (!OnOffDialog.Instance.isOnLoadMang)
                            {
                                ThongBaoController.Instance.LoadMang.Show();
                            }
                        }
                    }
                    yield return new WaitForSeconds(0.3f);
                }
            }
            
            if (Time.timeSinceLevelLoad - startTime > timeout)
            {
                //yield break;
            }
            yield return new WaitForSeconds(2);
        }
    }

    public void OnDisable()
    {
        StopPingPong();
    }
}