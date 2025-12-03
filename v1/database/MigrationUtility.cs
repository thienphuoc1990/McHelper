using AutoVPT.Libs;
using AutoVPT.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace AutoVPT.Database
{
    /// <summary>
    /// Utility to migrate character data from XML files to SQLite database
    /// </summary>
    public static class MigrationUtility
    {
        /// <summary>
        /// Migrate all XML character files to SQLite database
        /// </summary>
        /// <param name="backupXmlFiles">If true, backs up XML files before migration</param>
        /// <returns>Migration result with statistics</returns>
        public static MigrationResult MigrateXmlToSqlite(bool backupXmlFiles = true)
        {
            var result = new MigrationResult();

            try
            {
                var dbPath = Path.Combine(Application.StartupPath, "database");

                if (!Directory.Exists(dbPath))
                {
                    result.ErrorMessage = "Database directory not found";
                    return result;
                }

                // Initialize SQLite database
                DatabaseHelper.Initialize();

                // Get all XML files in database directory
                var xmlFiles = Directory.GetFiles(dbPath, "*.xml");

                foreach (var xmlFile in xmlFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(xmlFile);

                    // Skip the data.xml (character list) file
                    if (fileName.Equals("data", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    try
                    {
                        // Load character from XML file (not database!)
                        var character = Libs.Helper.LoadFromXmlFile(fileName);

                        if (character != null && !string.IsNullOrEmpty(character.ID))
                        {
                            // Save to SQLite
                            DatabaseHelper.SaveCharacter(character);

                            result.CharactersMigrated++;
                            result.MigratedCharacterIds.Add(character.ID);
                        }
                        else
                        {
                            result.Errors.Add($"Failed to load character from {fileName}.xml");
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Error migrating {fileName}: {ex.Message}");
                    }
                }

                // Backup XML files if requested
                if (backupXmlFiles && result.CharactersMigrated > 0)
                {
                    BackupXmlFiles();
                }

                result.Success = result.CharactersMigrated > 0;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Migration failed: {ex.Message}";
                result.Success = false;
            }

            return result;
        }

        /// <summary>
        /// Backup XML files to a timestamped folder
        /// </summary>
        private static void BackupXmlFiles()
        {
            var dbPath = Path.Combine(Application.StartupPath, "database");
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupPath = Path.Combine(Application.StartupPath, "database_backup_" + timestamp);

            Directory.CreateDirectory(backupPath);

            var xmlFiles = Directory.GetFiles(dbPath, "*.xml");

            foreach (var xmlFile in xmlFiles)
            {
                var fileName = Path.GetFileName(xmlFile);
                var destFile = Path.Combine(backupPath, fileName);
                File.Copy(xmlFile, destFile, true);
            }
        }

        /// <summary>
        /// Export SQLite data back to XML files (for backup or rollback)
        /// </summary>
        public static void ExportSqliteToXml()
        {
            var characters = DatabaseHelper.LoadAllCharacters();

            foreach (var character in characters)
            {
                Helper.saveSettingsToXML(character);
            }
        }

        /// <summary>
        /// Verify migration by comparing XML and SQLite data
        /// </summary>
        public static VerificationResult VerifyMigration()
        {
            var result = new VerificationResult();

            var dbPath = Path.Combine(Application.StartupPath, "database");
            var xmlFiles = Directory.GetFiles(dbPath, "*.xml");

            foreach (var xmlFile in xmlFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(xmlFile);

                if (fileName.Equals("data", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    // Load from XML file
                    var xmlCharacter = Libs.Helper.LoadFromXmlFile(fileName);

                    // Load from SQLite
                    var dbCharacter = DatabaseHelper.LoadCharacter(fileName);

                    // Compare
                    if (!CompareCharacters(xmlCharacter, dbCharacter))
                    {
                        result.Mismatches.Add(fileName);
                    }
                    else
                    {
                        result.MatchedCharacters++;
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error verifying {fileName}: {ex.Message}");
                }
            }

            result.Success = result.Mismatches.Count == 0 && result.Errors.Count == 0;

            return result;
        }

        /// <summary>
        /// Compare two character objects for equality
        /// </summary>
        private static bool CompareCharacters(Character c1, Character c2)
        {
            if (c1 == null && c2 == null) return true;
            if (c1 == null || c2 == null) return false;

            // Compare all relevant properties
            return c1.ID == c2.ID &&
                   c1.Link == c2.Link &&
                   c1.Group == c2.Group &&
                   c1.VipLevel == c2.VipLevel &&
                   c1.IncreaseFPS == c2.IncreaseFPS &&
                   c1.Date == c2.Date &&
                   c1.IsChinese == c2.IsChinese &&
                   c1.VipPromotion == c2.VipPromotion &&
                   c1.DoiNangNo == c2.DoiNangNo &&
                   c1.DoiNangNoNL4 == c2.DoiNangNoNL4 &&
                   c1.DoiNangNoLoai == c2.DoiNangNoLoai &&
                   c1.TrongNL == c2.TrongNL &&
                   c1.TrongNLLoai == c2.TrongNLLoai &&
                   c1.TriAn == c2.TriAn &&
                   c1.LatTheBai == c2.LatTheBai &&
                   c1.RutBo == c2.RutBo &&
                   c1.DoiKGDK == c2.DoiKGDK &&
                   c1.TuHanh == c2.TuHanh &&
                   c1.TruMa == c2.TruMa &&
                   c1.AoMaThap == c2.AoMaThap &&
                   c1.TrongCay == c2.TrongCay &&
                   c1.CheMatBao == c2.CheMatBao &&
                   c1.CheMatBaoLoai == c2.CheMatBaoLoai &&
                   c1.CheMatBaoCap == c2.CheMatBaoCap &&
                   c1.AutoPhuBan == c2.AutoPhuBan &&
                   c1.AutoPhuBanDanhSach == c2.AutoPhuBanDanhSach &&
                   c1.DanhSTMTDanhSach == c2.DanhSTMTDanhSach &&
                   c1.UocNguyen == c2.UocNguyen &&
                   c1.DauPet == c2.DauPet &&
                   c1.NhanThuongHLVT == c2.NhanThuongHLVT &&
                   c1.NhanHoiPhuc == c2.NhanHoiPhuc &&
                   c1.MeTran == c2.MeTran &&
                   c1.HaiThuoc == c2.HaiThuoc &&
                   c1.CauCa == c2.CauCa &&
                   c1.ViTriNhanVat == c2.ViTriNhanVat &&
                   c1.AutoThanTu == c2.AutoThanTu &&
                   c1.RunToLast == c2.RunToLast &&
                   c1.StatusVipPromotion == c2.StatusVipPromotion &&
                   c1.StatusDoiNangNo == c2.StatusDoiNangNo &&
                   c1.StatusTriAn == c2.StatusTriAn &&
                   c1.StatusLatTheBai == c2.StatusLatTheBai &&
                   c1.StatusRutBo == c2.StatusRutBo &&
                   c1.StatusDoiKGDK == c2.StatusDoiKGDK &&
                   c1.StatusTuHanh == c2.StatusTuHanh &&
                   c1.StatusTruMa == c2.StatusTruMa &&
                   c1.StatusAoMaThap == c2.StatusAoMaThap &&
                   c1.StatusTrongCay == c2.StatusTrongCay &&
                   c1.StatusCheMatBao == c2.StatusCheMatBao &&
                   c1.StatusAutoPhuBan == c2.StatusAutoPhuBan &&
                   c1.StatusUocNguyen == c2.StatusUocNguyen &&
                   c1.StatusNhanThuongHLVT == c2.StatusNhanThuongHLVT &&
                   c1.StatusNhanHoiPhuc == c2.StatusNhanHoiPhuc &&
                   c1.StatusMeTran == c2.StatusMeTran &&
                   c1.StatusHaiThuoc == c2.StatusHaiThuoc &&
                   c1.StatusCauCa == c2.StatusCauCa &&
                   c1.StatusAutoThanTu == c2.StatusAutoThanTu;
        }
    }

    public class MigrationResult
    {
        public bool Success { get; set; }
        public int CharactersMigrated { get; set; }
        public List<string> MigratedCharacterIds { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();
        public string ErrorMessage { get; set; }

        public override string ToString()
        {
            if (Success)
            {
                return $"Migration successful! {CharactersMigrated} character(s) migrated to SQLite.";
            }
            else
            {
                return $"Migration failed: {ErrorMessage ?? "Unknown error"}. Errors: {Errors.Count}";
            }
        }
    }

    public class VerificationResult
    {
        public bool Success { get; set; }
        public int MatchedCharacters { get; set; }
        public List<string> Mismatches { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();

        public override string ToString()
        {
            if (Success)
            {
                return $"Verification successful! {MatchedCharacters} character(s) matched.";
            }
            else
            {
                return $"Verification failed. Mismatches: {Mismatches.Count}, Errors: {Errors.Count}";
            }
        }
    }
}
