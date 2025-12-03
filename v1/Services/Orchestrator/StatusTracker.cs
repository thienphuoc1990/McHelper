using AutoVPT.Domain;
using AutoVPT.Services.Orchestrator.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoVPT.Services.Orchestrator
{
    /// <summary>
    /// Tracks execution status for all characters
    /// </summary>
    public class StatusTracker : IStatusTracker
    {
        private readonly Dictionary<string, CharacterOrchestrationStatus> _characterStatuses;
        private readonly object _lock = new object();

        public StatusTracker()
        {
            _characterStatuses = new Dictionary<string, CharacterOrchestrationStatus>();
        }

        /// <summary>
        /// Update character status
        /// </summary>
        public void UpdateCharacterStatus(string characterId, CharacterOrchestrationStatus status)
        {
            if (string.IsNullOrWhiteSpace(characterId))
                return;

            lock (_lock)
            {
                status.CharacterId = characterId;
                status.LastUpdate = DateTime.Now;
                _characterStatuses[characterId] = status;
            }
        }

        /// <summary>
        /// Get current status for character
        /// </summary>
        public CharacterOrchestrationStatus GetCharacterStatus(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
                return null;

            lock (_lock)
            {
                if (_characterStatuses.TryGetValue(characterId, out var status))
                {
                    return status;
                }
            }

            return null;
        }

        /// <summary>
        /// Get overall statistics
        /// </summary>
        public OrchestrationStatistics GetStatistics()
        {
            lock (_lock)
            {
                var stats = new OrchestrationStatistics();
                var featureStats = new Dictionary<FeatureType, FeatureStatistics>();

                foreach (var charStatus in _characterStatuses.Values)
                {
                    stats.TotalFeaturesEnabled += charStatus.TotalEnabledFeatures;
                    stats.TotalFeaturesCompleted += charStatus.CompletedFeatures;

                    // Aggregate feature-level statistics from execution log
                    foreach (var log in charStatus.ExecutionLog)
                    {
                        if (!featureStats.ContainsKey(log.Feature))
                        {
                            featureStats[log.Feature] = new FeatureStatistics
                            {
                                Feature = log.Feature
                            };
                        }

                        var featureStat = featureStats[log.Feature];
                        featureStat.TotalEnabled++;

                        switch (log.Status)
                        {
                            case FeatureExecutionStatus.Completed:
                                featureStat.TotalCompleted++;
                                break;
                            case FeatureExecutionStatus.Failed:
                                featureStat.TotalFailed++;
                                break;
                            case FeatureExecutionStatus.Skipped:
                                featureStat.TotalSkipped++;
                                break;
                        }

                        if (log.Duration.HasValue)
                        {
                            // Simple average calculation
                            var totalTime = featureStat.AverageExecutionTime.TotalMilliseconds * (featureStat.TotalCompleted - 1) + log.Duration.Value.TotalMilliseconds;
                            featureStat.AverageExecutionTime = TimeSpan.FromMilliseconds(totalTime / featureStat.TotalCompleted);
                        }

                        if (log.EndTime.HasValue && log.EndTime.Value > featureStat.LastExecution)
                        {
                            featureStat.LastExecution = log.EndTime.Value;
                        }
                    }
                }

                // Calculate completion percentages
                if (stats.TotalFeaturesEnabled > 0)
                {
                    stats.OverallCompletionPercentage = (double)stats.TotalFeaturesCompleted / stats.TotalFeaturesEnabled * 100;
                }

                foreach (var featureStat in featureStats.Values)
                {
                    if (featureStat.TotalEnabled > 0)
                    {
                        featureStat.CompletionPercentage = (double)featureStat.TotalCompleted / featureStat.TotalEnabled * 100;
                    }
                }

                stats.FeatureStats = featureStats;
                return stats;
            }
        }

        /// <summary>
        /// Get completion percentage for character
        /// </summary>
        public double GetCompletionPercentage(string characterId)
        {
            var status = GetCharacterStatus(characterId);
            if (status == null || status.TotalEnabledFeatures == 0)
                return 0;

            return status.CompletionPercentage;
        }

        /// <summary>
        /// Get overall orchestration status
        /// </summary>
        public OrchestrationStatus GetOrchestrationStatus()
        {
            lock (_lock)
            {
                var status = new OrchestrationStatus
                {
                    TotalCharacters = _characterStatuses.Count,
                    ActiveCharacters = _characterStatuses.Values.Count(s => s.IsActive),
                    CompletedCharacters = _characterStatuses.Values.Count(s => 
                        s.TotalEnabledFeatures > 0 && s.CompletedFeatures >= s.TotalEnabledFeatures),
                    FailedCharacters = _characterStatuses.Values.Count(s => 
                        s.CurrentFeatureStatus == FeatureExecutionStatus.Failed),
                    CharacterStatuses = new Dictionary<string, CharacterOrchestrationStatus>(_characterStatuses),
                    Statistics = GetStatistics(),
                    LastUpdate = DateTime.Now
                };

                return status;
            }
        }

        /// <summary>
        /// Clear all status (reset)
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _characterStatuses.Clear();
            }
        }
    }
}

