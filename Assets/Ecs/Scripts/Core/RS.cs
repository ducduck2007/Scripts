using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RS : MonoBehaviour
{
    public static RS Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    public Sprite[] imgBgLoading;
    public Sprite[] imgBannerGame;
    public Sprite[] imgHangBxh;
    public Sprite[] imgKhungItemBxh;  // 0 cá nhân, 1 bạn bè;
    public Sprite[] imgKhungItemBxhEvent;  // 0 cá nhân, 1 bạn bè;
    public Sprite[] imgTickDonHang;  // 0 thuong thieu, 1 thuong du, 2 vip thieu, 3 vip du, 4 dang cho;
    public Sprite imgVangGianHang;
    public Sprite imgKcGianHang;
    public Sprite imgTrang;
    public Sprite[] sprTenDauTruong;  // 0 tv, 1 ta;
    public Sprite[] sprDaiGia;  // 0 tv, 1 ta;
    public Sprite[] sprNongDan;  // 0 tv, 1 ta;
    public Sprite[] sprThu;  // 0 chưa đọc, 1 đã đọc;
    public Sprite[] sprHopQuaNv;  // 0 chưa được mở, 1 chưa mở, 2 đã mở
    public Sprite[] sprHopQuaEvent1;  // 0 chưa được mở, 1 chưa mở, 2 đã mở
    public Sprite[] sprHopQuaEvent2;  // 0 chưa được mở, 1 chưa mở, 2 đã mở
}
