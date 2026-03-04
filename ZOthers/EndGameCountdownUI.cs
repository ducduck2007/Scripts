using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameCountdownUI : ScaleScreen
{
    public TMP_Text txtCountdown;

    private void OnEnable()
    {
        if (txtCountdown == null) return;

        // Không đếm giây nữa, chỉ hiển thị text clickable
        txtCountdown.text = "Nhấn vào đây để thoát";
    }

    // Gắn hàm này vào sự kiện OnClick của Button / Text
    public void OnClickExit()
    {
        // Dừng nhạc trước khi load lại (AudioManager sẽ tự mở lại sau khi scene load xong)
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopAudioBg();

        SceneManager.LoadScene("Game");
    }
}
