using UnityEngine;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    private readonly Dictionary<int, int> _teamScores = new Dictionary<int, int>();

    private readonly Dictionary<long, int> _lastKillsByUser = new Dictionary<long, int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        _teamScores[1] = 0;
        _teamScores[2] = 0;
    }

    public void AddScore(int teamId, int amount)
    {
        if (teamId <= 0) return;
        if (!_teamScores.ContainsKey(teamId)) _teamScores[teamId] = 0;
        _teamScores[teamId] += amount;
    }

    public int GetScoreByTeam(int teamId)
    {
        return _teamScores.TryGetValue(teamId, out var v) ? v : 0;
    }

    public void ResetScores()
    {
        _teamScores[1] = 0;
        _teamScores[2] = 0;
        _lastKillsByUser.Clear();
    }

    public void ApplyResourceSnapshot(PlayerResourceData data, int teamId)
    {
        if (data == null || teamId <= 0) return;

        if (_lastKillsByUser.TryGetValue(data.userId, out int lastKills))
        {
            if (data.kills < lastKills)
            {
                ResetScores();
                _lastKillsByUser[data.userId] = data.kills;
                return;
            }

            int delta = data.kills - lastKills;
            if (delta > 0) AddScore(teamId, delta);

            _lastKillsByUser[data.userId] = data.kills;
        }
        else
        {
            _lastKillsByUser[data.userId] = data.kills;
        }
    }
}
