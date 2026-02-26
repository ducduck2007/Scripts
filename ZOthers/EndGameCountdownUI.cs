using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameCountdownUI : MonoBehaviour
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
        // CMD_GAME_END đã gọi CleanupUdpBeforeExit rồi, ở đây chỉ cần load lại scene
        SceneManager.LoadScene("Game");
    }
}
