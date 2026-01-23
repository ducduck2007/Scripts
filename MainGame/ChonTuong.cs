using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChonTuong : ScaleScreen
{
    public TextMeshProUGUI txtTenPlayer, txtTenTuong, txtTenTuong2, txtTrangThai;
    public Image imgTuongChon;
    public Button btnChon;
    public Toggle tg1, tg2, tg3, tg4;
    public GameObject[] tuong;
    public GameObject objChieuThuc, huBtn, objMoiChonTuong;
    public Sprite[] sprAvtTuong;

    private int heroType;

    protected override void Start()
    {
        base.Start();
        tg1.onValueChanged.AddListener(a =>
        {
            if (tg1.isOn)
            {
                heroType = 1;
                SelectHero(1);
            }
        });
        tg2.onValueChanged.AddListener(a =>
        {
            if (tg2.isOn)
            {
                heroType = 2;
                SelectHero(2);
            }
        });
        tg3.onValueChanged.AddListener(a =>
        {
            if (tg3.isOn)
            {
                heroType = 3;
                SelectHero(3);
            }
        });
        tg4.onValueChanged.AddListener(a =>
        {
            if (tg4.isOn)
            {
                heroType = 4;
                SelectHero(4);
            }
        });
        btnChon.onClick.AddListener(KhoaTuong);
    }

    public void SetData()
    {
        heroType = 0;
        txtTenPlayer.text = UserData.Instance.UserName;
        txtTenTuong.text = "";
        txtTenTuong2.text = "";
        imgTuongChon.sprite = sprAvtTuong[0];
        objChieuThuc.SetActive(false);
        objMoiChonTuong.SetActive(true);
        StatusBtnChon(true);
    }

    public void StatusBtnChon(bool val)
    {
        if (val)
        {
            txtTrangThai.text = "Chọn Tướng";
            btnChon.interactable = true;
            huBtn.SetActive(true);
        }
        else
        {
            txtTrangThai.text = "Đã Chọn";
            btnChon.interactable = false;
            huBtn.SetActive(false);
        }
    }

    private void SelectHero(int heroType)
    {
        objMoiChonTuong.SetActive(false);
        objChieuThuc.SetActive(true);
        for (int i = 0; i < tuong.Length; i++)
        {
            if (i == heroType - 1)
            {
                tuong[i].SetActive(true);
            }
            else
            {
                tuong[i].SetActive(false);
            }
        }
        imgTuongChon.sprite = sprAvtTuong[heroType];
        txtTenTuong.text = B.Instance.GetNameTuong(heroType);
        txtTenTuong2.text = B.Instance.GetNameTuong(heroType);
    }

    private void KhoaTuong()
    {
        if (heroType != 0)
        {
            SendData.SelectHero(heroType - 1);
        }
        else
        {
            ThongBaoController.Instance.Toast.ShowToast("Bạn chưa chọn tướng.");
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        DialogController.Instance.PopupTimTran.Show(false);
        SetData();
    }

    public void Show(bool val = true)
    {
        gameObject.SetActive(val);
    }
}
