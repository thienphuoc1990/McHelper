using AutoVPT.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AutoVPT.Services
{
    /// <summary>
    /// Validates feature configurations before execution.
    /// Ensures all required parameters are present and valid.
    /// </summary>
    public class ConfigurationValidator
    {
        /// <summary>
        /// Validation result for a single feature
        /// </summary>
        public class ValidationResult
        {
            public FeatureType FeatureType { get; set; }
            public bool IsValid { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
            public List<string> Warnings { get; set; } = new List<string>();

            public static ValidationResult Valid(FeatureType type)
            {
                return new ValidationResult { FeatureType = type, IsValid = true };
            }

            public static ValidationResult Invalid(FeatureType type, params string[] errors)
            {
                return new ValidationResult
                {
                    FeatureType = type,
                    IsValid = false,
                    Errors = errors.ToList()
                };
            }
        }

        /// <summary>
        /// Validates all enabled features for a character
        /// </summary>
        public Dictionary<FeatureType, ValidationResult> ValidateAll(CharacterAggregate character)
        {
            var results = new Dictionary<FeatureType, ValidationResult>();

            foreach (FeatureType featureType in Enum.GetValues(typeof(FeatureType)))
            {
                if (character.FeatureConfig.IsEnabled(featureType))
                {
                    results[featureType] = ValidateFeature(featureType, character);
                }
            }

            return results;
        }

        /// <summary>
        /// Validates a specific feature's configuration
        /// </summary>
        public ValidationResult ValidateFeature(FeatureType featureType, CharacterAggregate character)
        {
            switch (featureType)
            {
                case FeatureType.AutoPhuBan:
                    return ValidateAutoPhuBan(character);

                case FeatureType.TrongNL:
                    return ValidateTrongNL(character);

                case FeatureType.CheMatBao:
                    return ValidateCheMatBao(character);

                case FeatureType.TriAn:
                    return ValidateTriAn(character);

                case FeatureType.TruMa:
                    return ValidateTruMa(character);

                case FeatureType.DauPet:
                    return ValidateDauPet(character);

                // Features with no special configuration requirements
                case FeatureType.VipPromotion:
                case FeatureType.DoiNangNo:
                case FeatureType.DoiNangNoNL4:
                case FeatureType.DoiKGDK:
                case FeatureType.RutBo:
                case FeatureType.TuHanh:
                case FeatureType.NhanHoiPhuc:
                case FeatureType.NhanThuongHLVT:
                case FeatureType.LatTheBai:
                case FeatureType.UocNguyen:
                case FeatureType.AoMaThap:
                case FeatureType.TrongCay:
                case FeatureType.BugOnline:
                case FeatureType.MeTran:
                case FeatureType.HaiThuoc:
                case FeatureType.CauCa:
                case FeatureType.AutoThanTu:
                    return ValidationResult.Valid(featureType);

                default:
                    return ValidationResult.Valid(featureType);
            }
        }

        #region Feature-Specific Validators

        private ValidationResult ValidateAutoPhuBan(CharacterAggregate character)
        {
            var result = new ValidationResult
            {
                FeatureType = FeatureType.AutoPhuBan,
                IsValid = true
            };

            // Check if dungeon list is configured
            string dungeonList = character.FeatureConfig.GetParameter("DanhSach", "");
            if (string.IsNullOrWhiteSpace(dungeonList))
            {
                result.Errors.Add("Dungeon list (DanhSach) is empty. Please configure dungeons to run.");
                result.IsValid = false;
            }
            else
            {
                // Validate dungeon names
                var validDungeons = new[]
                {
                    "Liệt Diễm Thâm Uyên", "Trở Lại Lang Huyệt", "Kho Báu Đại Mạc",
                    "Lục Tiên Cảnh", "Mê Huyễn Động", "Quỷ Hút Máu", "Thám Hiểm"
                };

                var dungeons = dungeonList.Split(',').Select(d => d.Trim()).ToList();
                var invalidDungeons = dungeons.Where(d => !validDungeons.Contains(d)).ToList();

                if (invalidDungeons.Any())
                {
                    result.Warnings.Add($"Unknown dungeons: {string.Join(", ", invalidDungeons)}");
                }
            }

            return result;
        }

        private ValidationResult ValidateTrongNL(CharacterAggregate character)
        {
            var result = new ValidationResult
            {
                FeatureType = FeatureType.TrongNL,
                IsValid = true
            };

            // Validate material type if specified
            string materialType = character.FeatureConfig.GetParameter("LoaiNL", "");
            if (!string.IsNullOrEmpty(materialType))
            {
                var parsedType = MaterialTypeExtensions.FromString(materialType);
                if (parsedType == MaterialType.None)
                {
                    result.Warnings.Add($"Unknown material type: {materialType}. Will use default.");
                }
            }

            return result;
        }

        private ValidationResult ValidateCheMatBao(CharacterAggregate character)
        {
            var result = new ValidationResult
            {
                FeatureType = FeatureType.CheMatBao,
                IsValid = true
            };

            // Validate mat bao type
            string matBaoType = character.FeatureConfig.GetParameter("LoaiMB", "");
            if (string.IsNullOrWhiteSpace(matBaoType))
            {
                result.Warnings.Add("Mat Bao type not specified. Will use default.");
            }
            else
            {
                var parsedType = MatBaoTypeExtensions.FromString(matBaoType);
                if (parsedType == MatBaoType.None)
                {
                    result.Warnings.Add($"Unknown Mat Bao type: {matBaoType}. Will use default.");
                }
            }

            return result;
        }

        private ValidationResult ValidateTriAn(CharacterAggregate character)
        {
            var result = new ValidationResult
            {
                FeatureType = FeatureType.TriAn,
                IsValid = true
            };

            // Check if NPC paths exist in images folder
            string imagePath = Libs.Constant.ImagePathTriAnFolder;
            if (!Directory.Exists(imagePath))
            {
                result.Warnings.Add($"TriAn image folder not found: {imagePath}");
            }

            return result;
        }

        private ValidationResult ValidateTruMa(CharacterAggregate character)
        {
            var result = new ValidationResult
            {
                FeatureType = FeatureType.TruMa,
                IsValid = true
            };

            // Check if monster images exist
            string imagePath = Libs.Constant.ImagePathTruMaFolder;
            if (!Directory.Exists(imagePath))
            {
                result.Warnings.Add($"TruMa image folder not found: {imagePath}");
            }

            return result;
        }

        private ValidationResult ValidateDauPet(CharacterAggregate character)
        {
            var result = new ValidationResult
            {
                FeatureType = FeatureType.DauPet,
                IsValid = true
            };

            // Validate pet battle count if specified
            string countStr = character.FeatureConfig.GetParameter("SoLan", "");
            if (!string.IsNullOrEmpty(countStr))
            {
                if (!int.TryParse(countStr, out int count) || count < 1)
                {
                    result.Errors.Add($"Invalid battle count: {countStr}. Must be a positive number.");
                    result.IsValid = false;
                }
                else if (count > 100)
                {
                    result.Warnings.Add($"Battle count {count} is very high. This may take a long time.");
                }
            }

            return result;
        }

        #endregion

        #region Character Validation

        /// <summary>
        /// Validates character identity and account information
        /// </summary>
        public ValidationResult ValidateCharacterIdentity(CharacterAggregate character)
        {
            var result = new ValidationResult
            {
                FeatureType = FeatureType.VipPromotion, // Using as placeholder
                IsValid = true
            };

            if (character == null)
            {
                result.Errors.Add("Character is null");
                result.IsValid = false;
                return result;
            }

            if (character.Identity == null)
            {
                result.Errors.Add("Character identity is null");
                result.IsValid = false;
                return result;
            }

            if (string.IsNullOrEmpty(character.Identity.Id))
            {
                result.Errors.Add("Character ID is empty");
                result.IsValid = false;
            }

            if (string.IsNullOrEmpty(character.Identity.Username))
            {
                result.Warnings.Add("Username is empty - login may fail");
            }

            if (string.IsNullOrEmpty(character.Identity.Password))
            {
                result.Warnings.Add("Password is empty - login may fail");
            }

            return result;
        }

        #endregion

        #region Summary Methods

        /// <summary>
        /// Gets a summary of all validation results
        /// </summary>
        public static string GetValidationSummary(Dictionary<FeatureType, ValidationResult> results)
        {
            var validCount = results.Count(r => r.Value.IsValid);
            var invalidCount = results.Count(r => !r.Value.IsValid);
            var warningCount = results.Sum(r => r.Value.Warnings.Count);

            var summary = $"Validation: {validCount} valid, {invalidCount} invalid, {warningCount} warnings";

            if (invalidCount > 0)
            {
                summary += "\n\nErrors:";
                foreach (var result in results.Where(r => !r.Value.IsValid))
                {
                    summary += $"\n  {result.Key}: {string.Join("; ", result.Value.Errors)}";
                }
            }

            if (warningCount > 0)
            {
                summary += "\n\nWarnings:";
                foreach (var result in results.Where(r => r.Value.Warnings.Any()))
                {
                    summary += $"\n  {result.Key}: {string.Join("; ", result.Value.Warnings)}";
                }
            }

            return summary;
        }

        #endregion
    }
}

