using AutoVPT.Domain;
using System;

namespace AutoVPT.Services.Orchestrator.Models
{
    /// <summary>
    /// Statistics for a specific feature across all characters
    /// </summary>
    public class FeatureStatistics
    {
        public FeatureType Feature { get; set; }
        public int TotalEnabled { get; set; }
        public int TotalCompleted { get; set; }
        public int TotalFailed { get; set; }
        public int TotalSkipped { get; set; }
        public double CompletionPercentage { get; set; }
        public TimeSpan AverageExecutionTime { get; set; }
        public DateTime LastExecution { get; set; }
    }
}

