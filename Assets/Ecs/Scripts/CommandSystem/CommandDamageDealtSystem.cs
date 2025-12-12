using System;
using UnityEngine;

public class CommandDamageDealtSystem : BaseCommandSystem
{
    public CommandDamageDealtSystem(Contexts contexts) : base(contexts)
    {
    }

    protected override int GetProcessCommand()
    {
        return CMD.DAMAGE_DEALT;
    }

    protected override void Execute(Message msg)
    {
        try
        {
            // AgentUnity.LogError("CMD: DAMAGE_DEALT = " + msg.GetJson());
            long targetId = msg.GetLong("targetId");
            long attackerId = msg.GetLong("attackerId");
            int damage = msg.GetInt("damage");
            int remainingHp = msg.GetInt("remainingHp");
            int skillId = msg.GetInt("skillId");

            if (attackerId != B.Instance.UserID)
            {
                if (targetId != B.Instance.UserID)
                {
                    if (skillId == 0)
                    {
                        TranDauControl.Instance.SetAttackState(true, false);
                    }
                    else
                    {
                        TranDauControl.Instance.SetCastSkillState(skillId, false);
                    }
                }
                else
                {

                    if (skillId == 0)
                    {
                        TranDauControl.Instance.SetAttackState(true, true);
                    }
                    else
                    {
                        TranDauControl.Instance.SetCastSkillState(skillId, true);
                    }
                }
            }
        }
        catch (Exception e)
        {
            AgentUnity.LogError(e);
        }
    }
}