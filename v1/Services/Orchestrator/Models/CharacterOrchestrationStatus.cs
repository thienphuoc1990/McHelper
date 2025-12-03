using AutoVPT.Domain;
using System;
using System.Collections.Generic;

namespace AutoVPT.Services.Orchestrator.Models
{
    /// <summary>
    /// Execution status for a specific character
    /// </summary>
    public class CharacterOrchestrationStatus
    {
        public string CharacterId { get; set; }
        public bool IsActive { get; set; }
        public FeatureType? CurrentFeature { get; set; }
        public FeatureExecutionStatus? CurrentFeatureStatus { get; set; }
        public int CompletedFeatures { get; set; }
        public int TotalEnabledFeatures { get; set; }
        public double CompletionPercentage { get; set; }
        public NextAction NextAction { get; set; }
        public DateTime LastUpdate { get; set; }
        public List<FeatureExecutionLog> ExecutionLog { get; set; }

        public CharacterOrchestrationStatus()
        {
            ExecutionLog = new List<FeatureExecutionLog>();
            LastUpdate = DateTime.Now;
        }
    }

    /// <summary>
    /// Log entry for feature execution
    /// </summary>
    public class FeatureExecutionLog
    {
        public FeatureType Feature { get; set; }
        public FeatureExecutionStatus Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;
        public string Message { get; set; }
    }
}

