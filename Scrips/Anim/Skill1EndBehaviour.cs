using UnityEngine;

public class Skill1EndBehaviour : StateMachineBehaviour
{
    // Khi animation state kết thúc (thoát khỏi state)
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Tắt bool đánh thường
        animator.SetBool("isTungChieu", false);
        Debug.Log("Skill1 ended -> isTungChieu = false");
    }
}
