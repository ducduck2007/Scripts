using UnityEngine;

public class SkillEndBehaviour : StateMachineBehaviour
{
    [Tooltip("Tên skill để xác định animation nào đang kết thúc")]
    public string skillName = "Skill1";

    // Được gọi khi ENTER vào state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log($"=== ENTER {skillName} State ===");
    }

    // Được gọi khi EXIT khỏi state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log($"=== EXIT {skillName} State ===");

        ISkillHandler skillHandler = animator.GetComponent<ISkillHandler>();
        if (skillHandler != null)
        {
            skillHandler.OnSkillAnimationEnd(skillName);
            Debug.Log($">>> Skill {skillName} completed callback called");
        }
        else
        {
            Debug.LogError("!!! ISkillHandler NOT FOUND on " + animator.gameObject.name);
        }
    }
}