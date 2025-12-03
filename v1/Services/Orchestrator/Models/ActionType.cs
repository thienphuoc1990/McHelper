namespace AutoVPT.Services.Orchestrator.Models
{
    /// <summary>
    /// Type of action to take for a character
    /// </summary>
    public enum ActionType
    {
        ExecuteFeature,    // Execute a feature
        Wait,              // Wait (dependencies not met, cooldown, etc.)
        Skip,              // Skip (not enabled, already completed)
        Stop,              // Stop (error, user request)
        Complete           // All features completed
    }
}

