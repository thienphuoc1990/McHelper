using System;
using System.Collections.Generic;

namespace AutoVPT.Services.Orchestrator.Models
{
    /// <summary>
    /// Overall orchestration status
    /// </summary>
    public class OrchestrationStatus
    {
        public bool IsRunning { get; set; }
        public int TotalCharacters { get; set; }
        public int ActiveCharacters { get; set; }
        public int CompletedCharacters { get; set; }
        public int FailedCharacters { get; set; }
        public Dictionary<string, CharacterOrchestrationStatus> CharacterStatuses { get; set; }
        public OrchestrationStatistics Statistics { get; set; }
        public DateTime LastUpdate { get; set; }

        public OrchestrationStatus()
        {
            CharacterStatuses = new Dictionary<string, CharacterOrchestrationStatus>();
            Statistics = new OrchestrationStatistics();
            LastUpdate = DateTime.Now;
        }
    }
}

