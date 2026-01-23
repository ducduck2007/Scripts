using UnityEngine;

public class Skill3EndBehaviour : StateMachineBehaviour
{
    // Khi animation state kết thúc (thoát khỏi state)
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Tắt bool đánh thường
        animator.SetBool("isSkill3", false);
        Debug.Log("Skill3 ended -> isSkill3 = false");
    }
}
