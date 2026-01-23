using UnityEngine;

public class Skill2EndBehaviour : StateMachineBehaviour
{
    // Khi animation state kết thúc (thoát khỏi state)
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Tắt bool đánh thường
        animator.SetBool("isSkill2", false);
        Debug.LogError("Skill2 ended -> isSkill2 = false");
    }
}
