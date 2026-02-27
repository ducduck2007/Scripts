using UnityEngine;

public class DialogController : ManualSingleton<DialogController>
{
    private GameObject Load(string namePath)
    {
        return Resources.Load(namePath) as GameObject;
    }

    private DialogChonPhong _dialogChonPhong;
    public DialogChonPhong DialogChonPhong
    {
        get
        {
            if (_dialogChonPhong == null)
            {
                _dialogChonPhong = AgentUnity.InstanceObject<DialogChonPhong>(Load(PathResource.DialogChonPhong), transform);
            }

            return _dialogChonPhong;
        }
    }

    public void ShowDialogChonPhong()
    {
        DialogChonPhong.Show();
    }


    private PopupTimTran _dialogTimTran;
    public PopupTimTran PopupTimTran
    {
        get
        {
            if (_dialogTimTran == null)
            {
                _dialogTimTran = AgentUnity.InstanceObject<PopupTimTran>(Load(PathResource.PopupTimTran), transform);
            }

            return _dialogTimTran;
        }
    }

    public void ShowPopupTimTran()
    {
        PopupTimTran.Show();
    }


    private PopupBanBe _popupBanBe;
    public PopupBanBe DialogBanBe
    {
        get
        {
            if (_popupBanBe == null)
            {
                _popupBanBe = AgentUnity.InstanceObject<PopupBanBe>(Load(PathResource.DialogBanBe), transform);
            }

            return _popupBanBe;
        }
    }

    public void ShowDialogBanBe()
    {
        DialogBanBe.Show();
    }


    private DialogHomThu _dialogHomThu;
    public DialogHomThu DialogHomThu
    {
        get
        {
            if (_dialogHomThu == null)
            {
                _dialogHomThu = AgentUnity.InstanceObject<DialogHomThu>(Load(PathResource.DialogHomThu), transform);
            }

            return _dialogHomThu;
        }
    }

    public void ShowDialogHomThu()
    {
        DialogHomThu.Show();
    }


    private DialogTuong _dialogTuong;
    public DialogTuong DialogTuong
    {
        get
        {
            if (_dialogTuong == null)
            {
                _dialogTuong = AgentUnity.InstanceObject<DialogTuong>(Load(PathResource.DialogTuong), transform);
            }

            return _dialogTuong;
        }
    }

    public void ShowDialogTuong()
    {
        DialogTuong.Show();
    }

    private DialogChiTietTuong _dialogChiTietTuong;
    public DialogChiTietTuong DialogChiTietTuong
    {
        get
        {
            if (_dialogChiTietTuong == null)
            {
                _dialogChiTietTuong = AgentUnity.InstanceObject<DialogChiTietTuong>(Load(PathResource.DialogChiTietTuong), transform);
            }

            return _dialogChiTietTuong;
        }
    }

    public void ShowDialogSuKien()
    {
        DialogSuKien.Show();
    }

    private DialogSuKien _dialogSuKien;
    public DialogSuKien DialogSuKien
    {
        get
        {
            if (_dialogSuKien == null)
            {
                _dialogSuKien = AgentUnity.InstanceObject<DialogSuKien>(Load(PathResource.DialogSuKien), transform);
            }

            return _dialogSuKien;
        }
    }

    public void ShowDialogNhiemVu()
    {
        DialogNhiemVu.Show();
    }

    private DialogNhiemVu _dialogNhiemVu;
    public DialogNhiemVu DialogNhiemVu
    {
        get
        {
            if (_dialogNhiemVu == null)
            {
                _dialogNhiemVu = AgentUnity.InstanceObject<DialogNhiemVu>(Load(PathResource.DialogNhiemVu), transform);
            }

            return _dialogNhiemVu;
        }
    }

    public void ShowDialogTrangBi()
    {
        DialogTrangBi.Show();
    }

    private DialogTrangBi _dialogTrangBi;
    public DialogTrangBi DialogTrangBi
    {
        get
        {
            if (_dialogTrangBi == null)
            {
                _dialogTrangBi = AgentUnity.InstanceObject<DialogTrangBi>(Load(PathResource.DialogTrangBi), transform);
            }

            return _dialogTrangBi;
        }
    }

    private PopupGhepTran _popupGhepTran;
    public PopupGhepTran PopupGhepTran
    {
        get
        {
            if (_popupGhepTran == null)
            {
                _popupGhepTran = AgentUnity.InstanceObject<PopupGhepTran>(Load(PathResource.PopupGhepTran), transform);
            }
            return _popupGhepTran;
        }
    }

    public void ShowPopupGhepTran()
    {
        PopupGhepTran.Show();
    }
}
