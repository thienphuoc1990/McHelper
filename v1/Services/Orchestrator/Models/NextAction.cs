using AutoVPT.Domain;
using System;

namespace AutoVPT.Services.Orchestrator.Models
{
    /// <summary>
    /// Represents the next action to take for a character
    /// </summary>
    public class NextAction
    {
        public string CharacterId { get; set; }
        public FeatureType? Feature { get; set; }  // null = no action needed
        public ActionType ActionType { get; set; } // Execute, Wait, Skip, Stop
        public int Priority { get; set; }
        public string Reason { get; set; }
        public DateTime Timestamp { get; set; }

        public NextAction()
        {
            Timestamp = DateTime.Now;
        }

        public static NextAction Execute(string characterId, FeatureType feature, int priority, string reason = null)
        {
            return new NextAction
            {
                CharacterId = characterId,
                Feature = feature,
                ActionType = Models.ActionType.ExecuteFeature,
                Priority = priority,
                Reason = reason ?? $"Execute {feature}",
                Timestamp = DateTime.Now
            };
        }

        public static NextAction Wait(string characterId, string reason)
        {
            return new NextAction
            {
                CharacterId = characterId,
                Feature = null,
                ActionType = Models.ActionType.Wait,
                Priority = 0,
                Reason = reason,
                Timestamp = DateTime.Now
            };
        }

        public static NextAction Skip(string characterId, string reason)
        {
            return new NextAction
            {
                CharacterId = characterId,
                Feature = null,
                ActionType = Models.ActionType.Skip,
                Priority = 0,
                Reason = reason,
                Timestamp = DateTime.Now
            };
        }

        public static NextAction Stop(string characterId, string reason)
        {
            return new NextAction
            {
                CharacterId = characterId,
                Feature = null,
                ActionType = Models.ActionType.Stop,
                Priority = 0,
                Reason = reason,
                Timestamp = DateTime.Now
            };
        }

        public static NextAction Complete(string characterId)
        {
            return new NextAction
            {
                CharacterId = characterId,
                Feature = null,
                ActionType = Models.ActionType.Complete,
                Priority = 0,
                Reason = "All features completed",
                Timestamp = DateTime.Now
            };
        }
    }
}

