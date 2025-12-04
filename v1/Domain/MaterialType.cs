namespace AutoVPT.Domain
{
    /// <summary>
    /// Types of materials for crafting/farming.
    /// Used for type-safe material selection in TrongNL (planting) and other features.
    /// </summary>
    public enum MaterialType
    {
        /// <summary>Unknown or not set</summary>
        None = 0,

        // Level 1 Materials (Nguyên liệu cấp 1)
        /// <summary>Thiên Tinh Thảo (Celestial Grass)</summary>
        ThienTinhThao,
        /// <summary>Lục Ngọc Tinh (Green Jade Crystal)</summary>
        LucNgocTinh,
        /// <summary>Hồng Ngọc (Ruby)</summary>
        HongNgoc,

        // Level 2 Materials (Nguyên liệu cấp 2)
        /// <summary>Bạch Ngọc (White Jade)</summary>
        BachNgoc,
        /// <summary>Tử Ngọc (Purple Jade)</summary>
        TuNgoc,
        /// <summary>Hắc Ngọc (Black Jade)</summary>
        HacNgoc,

        // Level 3 Materials (Nguyên liệu cấp 3)
        /// <summary>Long Ngọc (Dragon Jade)</summary>
        LongNgoc,
        /// <summary>Phượng Ngọc (Phoenix Jade)</summary>
        PhuongNgoc,
        /// <summary>Kỳ Lân Ngọc (Unicorn Jade)</summary>
        KyLanNgoc,

        // Level 4 Materials (Nguyên liệu cấp 4)
        /// <summary>Thiên Long Ngọc (Celestial Dragon Jade)</summary>
        ThienLongNgoc,
        /// <summary>Thần Phượng Ngọc (Divine Phoenix Jade)</summary>
        ThanPhuongNgoc,

        // Level 5 Materials (Nguyên liệu cấp 5)
        /// <summary>Chân Long Ngọc (True Dragon Jade)</summary>
        ChanLongNgoc,
        /// <summary>Kim Phượng Ngọc (Golden Phoenix Jade)</summary>
        KimPhuongNgoc
    }

    /// <summary>
    /// Extension methods for MaterialType
    /// </summary>
    public static class MaterialTypeExtensions
    {
        /// <summary>
        /// Get the display name (Vietnamese) for a material type
        /// </summary>
        public static string GetDisplayName(this MaterialType type)
        {
            switch (type)
            {
                case MaterialType.ThienTinhThao: return "Thiên Tinh Thảo";
                case MaterialType.LucNgocTinh: return "Lục Ngọc Tinh";
                case MaterialType.HongNgoc: return "Hồng Ngọc";
                case MaterialType.BachNgoc: return "Bạch Ngọc";
                case MaterialType.TuNgoc: return "Tử Ngọc";
                case MaterialType.HacNgoc: return "Hắc Ngọc";
                case MaterialType.LongNgoc: return "Long Ngọc";
                case MaterialType.PhuongNgoc: return "Phượng Ngọc";
                case MaterialType.KyLanNgoc: return "Kỳ Lân Ngọc";
                case MaterialType.ThienLongNgoc: return "Thiên Long Ngọc";
                case MaterialType.ThanPhuongNgoc: return "Thần Phượng Ngọc";
                case MaterialType.ChanLongNgoc: return "Chân Long Ngọc";
                case MaterialType.KimPhuongNgoc: return "Kim Phượng Ngọc";
                default: return "Không xác định";
            }
        }

        /// <summary>
        /// Get the level of a material type
        /// </summary>
        public static int GetLevel(this MaterialType type)
        {
            switch (type)
            {
                case MaterialType.ThienTinhThao:
                case MaterialType.LucNgocTinh:
                case MaterialType.HongNgoc:
                    return 1;
                case MaterialType.BachNgoc:
                case MaterialType.TuNgoc:
                case MaterialType.HacNgoc:
                    return 2;
                case MaterialType.LongNgoc:
                case MaterialType.PhuongNgoc:
                case MaterialType.KyLanNgoc:
                    return 3;
                case MaterialType.ThienLongNgoc:
                case MaterialType.ThanPhuongNgoc:
                    return 4;
                case MaterialType.ChanLongNgoc:
                case MaterialType.KimPhuongNgoc:
                    return 5;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Parse material type from string (legacy compatibility)
        /// </summary>
        public static MaterialType FromString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return MaterialType.None;

            // Try to match display names
            foreach (MaterialType type in System.Enum.GetValues(typeof(MaterialType)))
            {
                if (type.GetDisplayName().Equals(value, System.StringComparison.OrdinalIgnoreCase))
                    return type;
            }

            // Try to parse as enum name
            if (System.Enum.TryParse<MaterialType>(value, true, out var result))
                return result;

            return MaterialType.None;
        }
    }
}

