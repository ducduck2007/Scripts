using UnityEngine;

public class NormalAttackConfig
{
    public float attackRange = 3f;          // TẦM ĐÁNH RIÊNG CHO ĐÁNH THƯỜNG
    public int damage = 1;                  // Sát thương đánh thường
    public float duration = 1.2f;           // Thời lượng animation đánh thường
    public float damageDelay = 0.3f;        // Độ trễ gây sát thương
    public string animationBool = "isDanhThuong"; // Tên animation
}
