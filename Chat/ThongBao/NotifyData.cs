using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotifyData
{
    public int index; // Vị trí
    public string content; // Nội dung Notify
    public int totalTime; // Thời gian chạy Notify
    public int time; // Thời gian chạy Notify

    public NotifyData(int index, string content, int totalTime, int time)
    {
        this.index = index;
        this.content = content;
        this.totalTime = totalTime;
        this.time = time;
    }
}
