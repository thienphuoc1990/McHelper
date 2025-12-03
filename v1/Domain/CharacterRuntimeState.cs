using System;
using System.Collections.Generic;

namespace AutoVPT.Domain
{
    /// <summary>
    /// Runtime execution mode
    /// </summary>
    public enum RunningMode
    {
        Stopped = 0,        // Not running
        Normal = 1,         // Normal daily automation
        Event = 2           // Event mode
    }

    /// <summary>
    /// Runtime state of a character.
    /// Tracks current execution status and feature completion.
    /// </summary>
    [Serializable]
    public class CharacterRuntimeState
    {
        /// <summary>
        /// Character ID this state belongs to
        /// </summary>
        public string CharacterId { get; set; }

        /// <summary>
        /// Current running mode
        /// </summary>
        public RunningMode Mode { get; set; }

        /// <summary>
        /// Last date settings were updated (for daily reset)
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Feature execution statuses mapped by type
        /// </summary>
        public Dictionary<FeatureType, FeatureExecutionStatus> FeatureStatuses { get; set; }

        /// <summary>
        /// Whether to run features to the last one
        /// </summary>
        public bool RunToLast { get; set; }

        public CharacterRuntimeState()
        {
            Mode = RunningMode.Stopped;
            LastUpdated = DateTime.Now;
            FeatureStatuses = new Dictionary<FeatureType, FeatureExecutionStatus>();
            InitializeStatuses();
        }

        public CharacterRuntimeState(string characterId)
        {
            CharacterId = characterId;
            Mode = RunningMode.Stopped;
            LastUpdated = DateTime.Now;
            FeatureStatuses = new Dictionary<FeatureType, FeatureExecutionStatus>();
            InitializeStatuses();
        }

        /// <summary>
        /// Initialize all feature statuses to NotStarted
        /// </summary>
        private void InitializeStatuses()
        {
            foreach (FeatureType featureType in Enum.GetValues(typeof(FeatureType)))
            {
                if (!FeatureStatuses.ContainsKey(featureType))
                {
                    FeatureStatuses[featureType] = FeatureExecutionStatus.NotStarted;
                }
            }
        }

        /// <summary>
        /// Check if feature has been completed today
        /// </summary>
        public bool IsCompleted(FeatureType feature)
        {
            return FeatureStatuses.ContainsKey(feature)
                && FeatureStatuses[feature] == FeatureExecutionStatus.Completed;
        }

        /// <summary>
        /// Mark feature as completed
        /// </summary>
        public void MarkCompleted(FeatureType feature)
        {
            FeatureStatuses[feature] = FeatureExecutionStatus.Completed;
        }

        /// <summary>
        /// Mark feature as failed
        /// </summary>
        public void MarkFailed(FeatureType feature)
        {
            FeatureStatuses[feature] = FeatureExecutionStatus.Failed;
        }

        /// <summary>
        /// Mark feature as skipped
        /// </summary>
        public void MarkSkipped(FeatureType feature)
        {
            FeatureStatuses[feature] = FeatureExecutionStatus.Skipped;
        }

        /// <summary>
        /// Reset feature status to NotStarted
        /// </summary>
        public void ResetFeature(FeatureType feature)
        {
            FeatureStatuses[feature] = FeatureExecutionStatus.NotStarted;
        }

        /// <summary>
        /// Reset all feature statuses (called on new day)
        /// </summary>
        public void ResetAllStatuses()
        {
            foreach (var key in new List<FeatureType>(FeatureStatuses.Keys))
            {
                FeatureStatuses[key] = FeatureExecutionStatus.NotStarted;
            }
            LastUpdated = DateTime.Now;
        }

        /// <summary>
        /// Check if a new day has started and reset if needed
        /// </summary>
        public bool CheckAndResetDaily()
        {
            var today = DateTime.Now.Date;
            var lastDate = LastUpdated.Date;

            if (today > lastDate)
            {
                ResetAllStatuses();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get feature execution status
        /// </summary>
        public FeatureExecutionStatus GetStatus(FeatureType feature)
        {
            return FeatureStatuses.ContainsKey(feature)
                ? FeatureStatuses[feature]
                : FeatureExecutionStatus.NotStarted;
        }

        /// <summary>
        /// Check if character is currently running
        /// </summary>
        public bool IsRunning => Mode != RunningMode.Stopped;

        /// <summary>
        /// Start automation in normal mode
        /// </summary>
        public void Start()
        {
            Mode = RunningMode.Normal;
        }

        /// <summary>
        /// Start automation in event mode
        /// </summary>
        public void StartEventMode()
        {
            Mode = RunningMode.Event;
        }

        /// <summary>
        /// Stop automation
        /// </summary>
        public void Stop()
        {
            Mode = RunningMode.Stopped;
        }
    }
}
