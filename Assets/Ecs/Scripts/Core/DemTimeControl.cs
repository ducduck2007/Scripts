using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemTimeControl : ManualSingleton<DemTimeControl>
{
    #region Thời gian hiện tại

    private DateTime _currentTime;
    private string _timeCurrent = "";
    private IEnumerator _iEOneSecond;

    // so sánh thời gian hiện tại với thời gian bất kỳ, nếu > 0 thời gian so sánh > thời gian hiện tại
    public long GetDifferenceSecond(string timeCheck)
    {
        DateTime dateCheck = DateTime.Parse(timeCheck);
        TimeSpan difference = dateCheck - _currentTime;
        return (int)(difference.TotalSeconds);
    }

    public void StartDemTime(string time)
    {
        _timeCurrent = time;
        _currentTime = DateTime.Parse(time);
        StartDemTimeOneSecond();
    }

    private void StartDemTimeOneSecond()
    {
        DemTimeOneSecond();
    }

    private void DemTimeOneSecond()
    {
        if (_iEOneSecond != null)
        {
            StopCoroutine(_iEOneSecond);
            _iEOneSecond = ShowTimeRemainingOneSecond();
            StartCoroutine(_iEOneSecond);
        }
        else
        {
            _iEOneSecond = ShowTimeRemainingOneSecond();
            StartCoroutine(_iEOneSecond);
        }
    }

    IEnumerator ShowTimeRemainingOneSecond()
    {
        while (true)
        {
            yield return new WaitForSeconds(C.ONE);
            _currentTime = _currentTime.AddSeconds(1); // Cộng 1 giây vào thời gian hiện tại
            _timeCurrent = _currentTime.ToString("yyyy-MM-dd HH:mm:ss");
//            Debug.LogError("timeCurrent= " + _timeCurrent);
            StartDemTimeOneSecond();
        }
    }

    public string AddTime(int seconds)
    {
        // Tăng thêm n giây
        DateTime newTime = _currentTime.AddSeconds(seconds);
        return newTime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /*
     * Độ chênh lệch của thời gian so sánh với thời gian hiện tại, nếu > 0 thời gian so sánh > thời gian hiện tại
     */
    public int GetCompareTime(string timeCheck)
    {
        DateTime dateCheck = DateTime.Parse(timeCheck);
        // Tính độ chênh lệch giữa time2 và time1
        TimeSpan difference = dateCheck - _currentTime;
        return (int)(difference.TotalSeconds);
        //       Debug.Log("Độ chênh lệch theo giây: " + difference.TotalSeconds);
        //       Debug.Log("Độ chênh lệch theo phút: " + difference.TotalMinutes);
        //       Debug.Log("Độ chênh lệch theo giờ: " + difference.TotalHours);
    }

    #endregion

    #region Khoảng cách Chat Thế giới 10s

    public long timeChatTheGioi;

    public void StartDemTimeChatTheGioi(long time)
    {
        timeChatTheGioi = time * 1000;
        GlobalCoroutine.Invoke(ShowTimeRemainingChatTheGioi());
    }

    IEnumerator ShowTimeRemainingChatTheGioi()
    {
        while (timeChatTheGioi > C.ZERO_LONG)
        {
            yield return new WaitForSeconds(C.ONE_FLOAT);
            timeChatTheGioi -= C.ONE_SECOND_TO_MILISECONDS;
        }

        timeChatTheGioi = C.ZERO_LONG;
        yield return null;
    }

    public long GetTimeChatTheGioi()
    {
        return (timeChatTheGioi / 1000);
    }

    #endregion

    

    public void StopAllTime()
    {
        this.StopAllCoroutines();
    }
}