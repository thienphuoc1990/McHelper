using AutoVPT.Domain;
using System;
using System.Collections.Generic;

namespace AutoVPT.Services.Orchestrator.Models
{
    /// <summary>
    /// Overall orchestration statistics
    /// </summary>
    public class OrchestrationStatistics
    {
        public int TotalFeaturesEnabled { get; set; }
        public int TotalFeaturesCompleted { get; set; }
        public int TotalFeaturesFailed { get; set; }
        public int TotalFeaturesSkipped { get; set; }
        public double OverallCompletionPercentage { get; set; }
        public TimeSpan AverageExecutionTime { get; set; }
        public Dictionary<FeatureType, FeatureStatistics> FeatureStats { get; set; }

        public OrchestrationStatistics()
        {
            FeatureStats = new Dictionary<FeatureType, FeatureStatistics>();
        }
    }
}

