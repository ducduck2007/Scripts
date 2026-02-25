using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class PlayLoadGate
{
    public static bool Ready { get; private set; } = false;

    private static Message _lastPlayersSnapshot;
    private static Message _lastMinionSnapshot;
    private static Message _lastMonsterSnapshot;

    private static Message _lastResourceSnapshot;
    private static Message _lastMinionResSnapshot;
    private static Message _lastMonsterResSnapshot;

    public static void Reset()
    {
        Ready = false;

        _lastPlayersSnapshot = null;
        _lastMinionSnapshot = null;
        _lastMonsterSnapshot = null;

        _lastResourceSnapshot = null;
        _lastMinionResSnapshot = null;
        _lastMonsterResSnapshot = null;
    }

    public static void MarkReady()
    {
        Ready = true;
    }

    public static bool ShouldProcess(int cmd)
    {
        switch (cmd)
        {
            case CMD.GAME_SNAPSHOT:
            case CMD.RESOURCE_SNAPSHOT:
            case CMD.MINION_SNAPSHOT:
            case CMD.MONSTER_SNAPSHOT:
            case CMD.RESOURCE_MINION_SNAPSHOT:
            case CMD.RESOURCE_MONSTER_SNAPSHOT:
                return Ready;
            default:
                return true;
        }
    }

    public static void Buffer(Message msg)
    {
        if (msg == null) return;

        switch (msg.cmd)
        {
            case CMD.GAME_SNAPSHOT:
                _lastPlayersSnapshot = msg;
                break;
            case CMD.MINION_SNAPSHOT:
                _lastMinionSnapshot = msg;
                break;
            case CMD.MONSTER_SNAPSHOT:
                _lastMonsterSnapshot = msg;
                break;

            case CMD.RESOURCE_SNAPSHOT:
                _lastResourceSnapshot = msg;
                break;
            case CMD.RESOURCE_MINION_SNAPSHOT:
                _lastMinionResSnapshot = msg;
                break;
            case CMD.RESOURCE_MONSTER_SNAPSHOT:
                _lastMonsterResSnapshot = msg;
                break;
        }
    }

    public static void FlushTo(TranDauControl tdc)
    {
        if (!Ready) return;
        if (tdc == null) return;

        if (_lastPlayersSnapshot != null)
        {
            TryApplyPlayers(_lastPlayersSnapshot, tdc);
            _lastPlayersSnapshot = null;
        }

        if (_lastMonsterSnapshot != null)
        {
            TryApplyMonsters(_lastMonsterSnapshot, tdc);
            _lastMonsterSnapshot = null;
        }

        if (_lastMinionSnapshot != null)
        {
            TryApplyMinions(_lastMinionSnapshot, tdc);
            _lastMinionSnapshot = null;
        }

        if (_lastResourceSnapshot != null)
        {
            TryApplyPlayerRes(_lastResourceSnapshot);
            _lastResourceSnapshot = null;
        }

        if (_lastMinionResSnapshot != null)
        {
            TryApplyMinionRes(_lastMinionResSnapshot, tdc);
            _lastMinionResSnapshot = null;
        }

        if (_lastMonsterResSnapshot != null)
        {
            TryApplyMonsterRes(_lastMonsterResSnapshot, tdc);
            _lastMonsterResSnapshot = null;
        }
    }

    private static void TryApplyPlayers(Message msg, TranDauControl tdc)
    {
        var serverPlayers = msg.GetClassList<ServerPlayerData>("players");
        if (serverPlayers == null || serverPlayers.Count == 0) return;

        var players = new List<PlayerOutPutSv>(serverPlayers.Count);
        for (int i = 0; i < serverPlayers.Count; i++)
        {
            var sp = serverPlayers[i];
            players.Add(new PlayerOutPutSv
            {
                userId = sp.userId,
                teamId = sp.teamId,
                x = sp.x / 2f,
                y = sp.y / 2f,
                heading = sp.heading,
                isMoving = sp.isMoving,
                isAlive = sp.isAlive,
                heroType = tdc.ResolveAndCacheHeroType(sp.userId, sp.teamId, sp.heroType)
            });
        }
        tdc.Init(players);
    }

    private static void TryApplyMinions(Message msg, TranDauControl tdc)
    {
        JArray minionsArray = msg.GetJArray("m");
        if (minionsArray == null || minionsArray.Count == 0) return;

        var minionList = new List<MinionOutPutSv>(minionsArray.Count);

        for (int i = 0; i < minionsArray.Count; i++)
        {
            JArray m = (JArray)minionsArray[i];
            if (m == null || m.Count < 5) continue;

            minionList.Add(new MinionOutPutSv
            {
                id = (long)m[0],
                teamId = (int)m[1],
                laneId = (int)m[2],
                x = (int)m[3] / 2f,
                y = (int)m[4] / 2f
            });
        }

        tdc.InitMinions(minionList);
    }
    private static void TryApplyMonsters(Message msg, TranDauControl tdc)
    {
        var monsters = msg.GetClassList<JungleMonsterOutPutSv>("monsters");
        if (monsters == null || monsters.Count == 0) return;

        var list = new List<JungleMonsterOutPutSv>(monsters.Count);
        for (int i = 0; i < monsters.Count; i++)
        {
            var mo = monsters[i];
            list.Add(new JungleMonsterOutPutSv
            {
                id = mo.id,
                campId = mo.campId,
                x = mo.x / 2,
                y = mo.y / 2,
                hp = mo.hp,
                hpMax = mo.hpMax
            });
        }
        tdc.InitMonster(list);
    }

    private static void TryApplyPlayerRes(Message msg)
    {
    }

    private static void TryApplyMinionRes(Message msg, TranDauControl tdc)
    {
        var minions = msg.GetClassList<MinionResourceData>("minions");
        if (minions == null || minions.Count == 0) return;

        for (int i = 0; i < minions.Count; i++)
        {
            var m = minions[i];
            tdc.UpdateMinionResource(m.id, m.hp, m.maxHp);
        }
    }

    private static void TryApplyMonsterRes(Message msg, TranDauControl tdc)
    {
        var monsters = msg.GetClassList<MonsterResourceData>("monsters");
        if (monsters == null || monsters.Count == 0) return;

        for (int i = 0; i < monsters.Count; i++)
        {
            var m = monsters[i];
            if (m.x != 0 || m.y != 0)
                tdc.UpdateMonsterResourceWithPosition(m.id, m.campId, m.hp, m.maxHp, m.x / 2f, m.y / 2f);
            else
                tdc.UpdateMonsterResource(m.id, m.campId, m.hp, m.maxHp);
        }
    }
}
