using UnityEngine;

public class NormalAttackEndBehaviour : StateMachineBehaviour
{
    // Khi animation state kết thúc (thoát khỏi state)
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Tắt bool đánh thường
        animator.SetBool("isDanhThuong", false);
        Debug.Log("Normal Attack animation ended -> isDanhThuong = false");
    }
}
