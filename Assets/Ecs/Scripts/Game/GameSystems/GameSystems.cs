public sealed class GameSystems : Feature
{
    public GameSystems(Contexts contexts)
    {
        Add(new InitGameSystem(contexts));
        Add(new LoginSuccessSystem(contexts));
    }
}