using AutoVPT.Domain;
using System;
using System.Collections.Generic;

namespace AutoVPT.Services.Orchestrator
{
    /// <summary>
    /// Defines priority levels for feature execution.
    /// Lower numbers = higher priority (executed first).
    /// </summary>
    public static class PriorityRules
    {
        private static readonly Dictionary<FeatureType, int> _priorities = new Dictionary<FeatureType, int>
        {
            // Critical Priority (1-3): Must-do daily tasks
            { FeatureType.VipPromotion, 1 },
            { FeatureType.AutoPhuBan, 2 },
            { FeatureType.TuHanh, 3 },

            // Important Priority (4-6): High-value features
            { FeatureType.DoiNangNo, 4 },
            { FeatureType.TrongNL, 5 },
            { FeatureType.CheMatBao, 6 },

            // Standard Priority (7-10): Regular features
            { FeatureType.TruMa, 7 },
            { FeatureType.RutBo, 8 },
            { FeatureType.DoiKGDK, 9 },
            { FeatureType.LatTheBai, 10 },

            // Optional Priority (11+): Nice-to-have features
            { FeatureType.CauCa, 11 },
            { FeatureType.HaiThuoc, 12 },
            { FeatureType.MeTran, 13 },
            { FeatureType.TriAn, 14 },
            { FeatureType.AoMaThap, 15 },
            { FeatureType.TrongCay, 16 },
            { FeatureType.UocNguyen, 17 },
            { FeatureType.NhanThuongHLVT, 18 },
            { FeatureType.NhanHoiPhuc, 19 },
            { FeatureType.AutoThanTu, 20 },
            { FeatureType.DauPet, 21 },
            { FeatureType.BugOnline, 22 },
            { FeatureType.DoiNangNoNL4, 23 } // Lower priority variant
        };

        /// <summary>
        /// Get priority for a feature. Lower number = higher priority.
        /// Returns 999 if feature not found (lowest priority).
        /// </summary>
        public static int GetPriority(FeatureType feature)
        {
            return _priorities.TryGetValue(feature, out int priority) 
                ? priority 
                : 999; // Default to lowest priority if not defined
        }

        /// <summary>
        /// Get priority level category name
        /// </summary>
        public static string GetPriorityCategory(FeatureType feature)
        {
            int priority = GetPriority(feature);
            
            if (priority >= 1 && priority <= 3)
                return "Critical";
            else if (priority >= 4 && priority <= 6)
                return "Important";
            else if (priority >= 7 && priority <= 10)
                return "Standard";
            else
                return "Optional";
        }

        /// <summary>
        /// Check if feature is in critical priority range (1-3)
        /// </summary>
        public static bool IsCritical(FeatureType feature)
        {
            int priority = GetPriority(feature);
            return priority >= 1 && priority <= 3;
        }

        /// <summary>
        /// Check if feature is in important priority range (4-6)
        /// </summary>
        public static bool IsImportant(FeatureType feature)
        {
            int priority = GetPriority(feature);
            return priority >= 4 && priority <= 6;
        }

        /// <summary>
        /// Check if feature is in standard priority range (7-10)
        /// </summary>
        public static bool IsStandard(FeatureType feature)
        {
            int priority = GetPriority(feature);
            return priority >= 7 && priority <= 10;
        }

        /// <summary>
        /// Check if feature is in optional priority range (11+)
        /// </summary>
        public static bool IsOptional(FeatureType feature)
        {
            int priority = GetPriority(feature);
            return priority >= 11;
        }

        /// <summary>
        /// Get all features sorted by priority (ascending - highest priority first)
        /// </summary>
        public static IEnumerable<FeatureType> GetFeaturesByPriority()
        {
            var sorted = new List<(FeatureType feature, int priority)>();
            
            foreach (var kvp in _priorities)
            {
                sorted.Add((kvp.Key, kvp.Value));
            }
            
            sorted.Sort((a, b) => a.priority.CompareTo(b.priority));
            
            foreach (var item in sorted)
            {
                yield return item.feature;
            }
        }

        /// <summary>
        /// Get all features in a specific priority range
        /// </summary>
        public static IEnumerable<FeatureType> GetFeaturesInRange(int minPriority, int maxPriority)
        {
            foreach (var kvp in _priorities)
            {
                if (kvp.Value >= minPriority && kvp.Value <= maxPriority)
                {
                    yield return kvp.Key;
                }
            }
        }
    }
}

