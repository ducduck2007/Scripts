
public sealed class CommandSystems : Feature
{
    public CommandSystems(Contexts contexts)
    {
        Add(new CommandLoginSystem(contexts));
        
        // Cleanup (Add CMD mới trên CLEANUP)
        Add(new CleanupCommandSystem(contexts));
        
    }
}