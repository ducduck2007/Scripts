using UnityEngine;

public enum GameState
{
    DISCONNECTED = 0,
    CONNECTING = 1,
    LOBBY = 2,
    FINDING_MATCH = 3,
    HERO_SELECTION = 4,
    IN_GAME = 5,
    GAME_ENDED = 6
}

public class GameStateManager
{
    public static GameState CurrentState = GameState.DISCONNECTED;

    public static void SetState(GameState newState)
    {
        Debug.Log($"GameState: {CurrentState} → {newState}");
        CurrentState = newState;
    }

    public static bool IsInGame()
    {
        return CurrentState == GameState.IN_GAME;
    }
}