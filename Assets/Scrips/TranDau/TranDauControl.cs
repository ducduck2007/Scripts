using System.Collections.Generic;
using UnityEngine;

public class TranDauControl : ManualSingleton<TranDauControl>
{
    public PlayerMove player1, player2;
    public PlayerMove playerMove
    {
        get
        {
            if (B.Instance.teamId == 1)
            {
                return player1;
            }
            else
            {
                return player2;
            }
        }
    }
    public PlayerOther otherPlayer1, otherPlayer2;
    public PlayerOther playerOther
    {
        get
        {
            if (B.Instance.teamId == 2)
            {
                return otherPlayer1;
            }
            else
            {
                return otherPlayer2;
            }
        }
    }
    public JungleMonster[] jungleMonsters;

    public virtual void Start()
    {
        if (B.Instance.teamId == 1)
        {
            player1.gameObject.SetActive(true);
            player2.gameObject.SetActive(false);
            otherPlayer1.gameObject.SetActive(false);
            otherPlayer2.gameObject.SetActive(true);
        }
        else
        {
            player1.gameObject.SetActive(false);
            player2.gameObject.SetActive(true);
            otherPlayer1.gameObject.SetActive(true);
            otherPlayer2.gameObject.SetActive(false);
        }
        playerMove.SetPotion();
    }

    public void Init(List<PlayerOutPutSv> playersData)
    {
        if (playerMove == null || playerOther == null)
        {
            return;
        }
        foreach (var pdata in playersData)
        {
            if (pdata.userId == B.Instance.UserID)
            {
                playerMove.ApplyServerData(pdata);
                if (pdata.teamId == 1)
                {
                    int layer1 = LayerMask.NameToLayer("player1");
                    if (playerMove.controller.gameObject.layer != layer1)
                    {
                        playerMove.controller.gameObject.layer = layer1;
                    }
                    if (playerOther.enemyLayer != layer1)
                    {
                        playerOther.enemyLayer = layer1;
                    }
                    int layer2 = LayerMask.GetMask("player2");
                    if (playerMove.enemyLayer != layer2)
                    {
                        playerMove.enemyLayer = layer2;
                    }
                }
                else
                {
                    int layer2 = LayerMask.NameToLayer("player2");
                    if (playerMove.controller.gameObject.layer != layer2)
                    {
                        playerMove.controller.gameObject.layer = layer2;
                    }
                    if (playerOther.enemyLayer != layer2)
                    {
                        playerOther.enemyLayer = layer2;
                    }
                    int layer1 = LayerMask.GetMask("player1");
                    if (playerMove.enemyLayer != layer1)
                    {
                        playerMove.enemyLayer = layer1;
                    }
                }
            }
            else
            {
                playerOther.ApplyServerData(pdata);
                if (pdata.teamId == 1)
                {
                    int layer = LayerMask.NameToLayer("player1");
                    if (playerOther.animator.gameObject.layer != layer)
                    {
                        playerOther.animator.gameObject.layer = layer;
                    }
                }
                else
                {
                    int layer = LayerMask.NameToLayer("player2");
                    if (playerOther.animator.gameObject.layer != layer)
                    {
                        playerOther.animator.gameObject.layer = layer;
                    }
                }
            }
        }
    }

    public void InitMonster(List<JungleMonsterOutPutSv> monstersData)
    {
        foreach (var mdata in monstersData)
        {
            foreach (var monster in jungleMonsters)
            {
                if (monster.id == mdata.id)
                {
                    monster.UpdateFromServer(mdata.x, mdata.y, mdata.hp, mdata.hpMax);
                    break;
                }
            }
        }
    }

    public void SetAttackState(bool isAttack, bool hasTarget)
    {
        playerOther.SetAttackState(isAttack, hasTarget);
    }

    public void SetCastSkillState(int skillId, bool hasTarget)
    {
        if (skillId == 1)
        {
            playerOther.CastSkillFromServer(1, hasTarget);
        }
        else if (skillId == 2)
        {
            playerOther.CastSkillFromServer(2, hasTarget);
        }
        else if (skillId == 3)
        {
            playerOther.CastSkillFromServer(3, hasTarget);
        }
    }


}
