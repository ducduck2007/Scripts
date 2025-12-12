
public sealed class CommandSystems : Feature
{
    public CommandSystems(Contexts contexts)
    {
        Add(new CommandLoginSystem(contexts));

        // Matchmaking

        Add(new CommandFindMatchResponseSystem(contexts));
        Add(new CommandMatchFoundSystem(contexts));
        Add(new CommandGameStartSystem(contexts));


        // Game updates
        Add(new CommandGameSnapshotSystem(contexts));
        Add(new CommandPlayerLeftSystem(contexts));

        // Combat
        Add(new CommandDamageDealtSystem(contexts));
        Add(new CommandDeathSystem(contexts));
        Add(new CommandRespawnSystem(contexts));

        // Cleanup (Add CMD mới trên CLEANUP)
        Add(new CleanupCommandSystem(contexts));
    }
}