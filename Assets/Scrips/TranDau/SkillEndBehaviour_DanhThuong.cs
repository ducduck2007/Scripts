using UnityEngine;

public class NormalAttackEndBehaviour : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Sử dụng interface thay vì trực tiếp PlayerMove
        ISkillHandler skillHandler = animator.GetComponent<ISkillHandler>();
        if (skillHandler != null)
        {
            skillHandler.OnNormalAttackEnd();
            Debug.Log(">>> Normal attack end callback called");
        }
        else
        {
            Debug.LogError("!!! ISkillHandler NOT FOUND on " + animator.gameObject.name);
        }
    }
}