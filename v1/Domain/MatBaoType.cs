namespace AutoVPT.Domain
{
    /// <summary>
    /// Types of Mat Bao (Secret Treasure) for crafting.
    /// Used for type-safe Mat Bao selection in CheMatBao feature.
    /// </summary>
    public enum MatBaoType
    {
        /// <summary>Unknown or not set</summary>
        None = 0,

        /// <summary>Pháp Sức (Magic Power) - Attack-focused</summary>
        PhapSuc,

        /// <summary>Thể Lực (Physical Power) - Defense-focused</summary>
        TheLuc,

        /// <summary>Tinh Thần (Spirit) - Support-focused</summary>
        TinhThan,

        /// <summary>Sinh Lực (Vitality) - HP-focused</summary>
        SinhLuc
    }

    /// <summary>
    /// Mat Bao crafting levels
    /// </summary>
    public enum MatBaoLevel
    {
        /// <summary>Not set</summary>
        None = 0,

        /// <summary>Level 1 Mat Bao</summary>
        Level1 = 1,

        /// <summary>Level 2 Mat Bao</summary>
        Level2 = 2,

        /// <summary>Level 3 Mat Bao</summary>
        Level3 = 3,

        /// <summary>Level 4 Mat Bao</summary>
        Level4 = 4,

        /// <summary>Level 5 Mat Bao</summary>
        Level5 = 5,

        /// <summary>Level 6 Mat Bao</summary>
        Level6 = 6
    }

    /// <summary>
    /// Extension methods for MatBaoType
    /// </summary>
    public static class MatBaoTypeExtensions
    {
        /// <summary>
        /// Get the display name (Vietnamese) for a Mat Bao type
        /// </summary>
        public static string GetDisplayName(this MatBaoType type)
        {
            switch (type)
            {
                case MatBaoType.PhapSuc: return "Pháp Sức";
                case MatBaoType.TheLuc: return "Thể Lực";
                case MatBaoType.TinhThan: return "Tinh Thần";
                case MatBaoType.SinhLuc: return "Sinh Lực";
                default: return "Không xác định";
            }
        }

        /// <summary>
        /// Parse Mat Bao type from string (legacy compatibility)
        /// </summary>
        public static MatBaoType FromString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return MatBaoType.None;

            // Try to match display names
            foreach (MatBaoType type in System.Enum.GetValues(typeof(MatBaoType)))
            {
                if (type.GetDisplayName().Equals(value, System.StringComparison.OrdinalIgnoreCase))
                    return type;
            }

            // Try to parse as enum name
            if (System.Enum.TryParse<MatBaoType>(value, true, out var result))
                return result;

            return MatBaoType.None;
        }
    }
}

