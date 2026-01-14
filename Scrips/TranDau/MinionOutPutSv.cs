using UnityEngine;

public class MinionOutPutSv
{
    public long id;          // minionId từ server
    public int teamId;       // 1 hoặc 2
    public int laneId;       // 0=top, 1=mid, 2=bot
    public float x;
    public float y;
    public int hp;
    public int maxHp;
}