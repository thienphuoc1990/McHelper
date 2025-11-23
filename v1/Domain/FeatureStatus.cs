using System;

namespace AutoVPT.Domain
{
    /// <summary>
    /// Status of a feature execution.
    /// Tracks whether a daily feature has been completed.
    /// </summary>
    public enum FeatureExecutionStatus
    {
        NotStarted = 0,     // Not yet run today
        Completed = 1,      // Successfully completed today
        Failed = 2,         // Attempted but failed
        Skipped = 3         // Skipped (conditions not met)
    }
}
