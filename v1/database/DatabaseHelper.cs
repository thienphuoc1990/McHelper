using AutoVPT.Objects;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;

namespace AutoVPT.Database
{
    /// <summary>
    /// SQLite database helper for character storage and retrieval
    /// Replaces XML-based storage with SQLite for better performance and query capabilities
    /// </summary>
    public static class DatabaseHelper
    {
        private static string _connectionString;
        private static readonly object _dbLock = new object();

        /// <summary>
        /// Initialize the database connection
        /// Call this once on application startup
        /// </summary>
        public static void Initialize()
        {
            var dbPath = Path.Combine(Application.StartupPath, "database", "mchelper.db");
            var dbDir = Path.GetDirectoryName(dbPath);

            Directory.CreateDirectory(dbDir);

            _connectionString = $"Data Source={dbPath};Version=3;";

            // Create tables if they don't exist
            CreateTables();
        }

        /// <summary>
        /// Create database tables from schema
        /// </summary>
        private static void CreateTables()
        {
            var schemaPath = Path.Combine(Application.StartupPath, "Database", "schema.sql");

            if (!File.Exists(schemaPath))
            {
                throw new FileNotFoundException($"Database schema file not found: {schemaPath}");
            }

            var schema = File.ReadAllText(schemaPath);

            lock (_dbLock)
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand(schema, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Save or update a character in the database
        /// </summary>
        public static void SaveCharacter(Character character)
        {
            if (character == null || string.IsNullOrEmpty(character.ID))
            {
                throw new ArgumentException("Character ID cannot be null or empty");
            }

            lock (_dbLock)
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();

                    // Use INSERT OR REPLACE for upsert functionality
                    var sql = @"
                        INSERT OR REPLACE INTO Characters (
                            ID, Link, GroupName, VipLevel, IncreaseFPS, Date, IsChinese,
                            VipPromotion, DoiNangNo, DoiNangNoNL4, DoiNangNoLoai,
                            TrongNL, TrongNLLoai, TriAn, LatTheBai, RutBo, DoiKGDK,
                            TuHanh, TruMa, AoMaThap, TrongCay, CheMatBao, CheMatBaoLoai,
                            CheMatBaoCap, AutoPhuBan, AutoPhuBanDanhSach, DanhSTMTDanhSach,
                            UocNguyen, DauPet, NhanThuongHLVT, NhanHoiPhuc, MeTran,
                            HaiThuoc, CauCa, ViTriNhanVat, AutoThanTu, RunToLast,
                            Running,
                            StatusVipPromotion, StatusDoiNangNo, StatusTriAn, StatusLatTheBai,
                            StatusRutBo, StatusDoiKGDK, StatusTuHanh, StatusTruMa,
                            StatusAoMaThap, StatusTrongCay, StatusCheMatBao, StatusAutoPhuBan,
                            StatusUocNguyen, StatusNhanThuongHLVT, StatusNhanHoiPhuc,
                            StatusMeTran, StatusHaiThuoc, StatusCauCa, StatusAutoThanTu
                        ) VALUES (
                            @ID, @Link, @GroupName, @VipLevel, @IncreaseFPS, @Date, @IsChinese,
                            @VipPromotion, @DoiNangNo, @DoiNangNoNL4, @DoiNangNoLoai,
                            @TrongNL, @TrongNLLoai, @TriAn, @LatTheBai, @RutBo, @DoiKGDK,
                            @TuHanh, @TruMa, @AoMaThap, @TrongCay, @CheMatBao, @CheMatBaoLoai,
                            @CheMatBaoCap, @AutoPhuBan, @AutoPhuBanDanhSach, @DanhSTMTDanhSach,
                            @UocNguyen, @DauPet, @NhanThuongHLVT, @NhanHoiPhuc, @MeTran,
                            @HaiThuoc, @CauCa, @ViTriNhanVat, @AutoThanTu, @RunToLast,
                            @Running,
                            @StatusVipPromotion, @StatusDoiNangNo, @StatusTriAn, @StatusLatTheBai,
                            @StatusRutBo, @StatusDoiKGDK, @StatusTuHanh, @StatusTruMa,
                            @StatusAoMaThap, @StatusTrongCay, @StatusCheMatBao, @StatusAutoPhuBan,
                            @StatusUocNguyen, @StatusNhanThuongHLVT, @StatusNhanHoiPhuc,
                            @StatusMeTran, @StatusHaiThuoc, @StatusCauCa, @StatusAutoThanTu
                        )";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        AddCharacterParameters(cmd, character);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Load a character from the database by ID
        /// </summary>
        public static Character LoadCharacter(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new Character();
            }

            lock (_dbLock)
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();

                    var sql = "SELECT * FROM Characters WHERE ID = @ID";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", id);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return MapReaderToCharacter(reader);
                            }
                        }
                    }
                }
            }

            return new Character { ID = id };
        }

        /// <summary>
        /// Load all characters from the database
        /// </summary>
        public static List<Character> LoadAllCharacters()
        {
            var characters = new List<Character>();

            lock (_dbLock)
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();

                    var sql = "SELECT * FROM Characters ORDER BY ID";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            characters.Add(MapReaderToCharacter(reader));
                        }
                    }
                }
            }

            return characters;
        }

        /// <summary>
        /// Delete a character from the database
        /// </summary>
        public static void DeleteCharacter(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            lock (_dbLock)
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();

                    var sql = "DELETE FROM Characters WHERE ID = @ID";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Get characters by group name
        /// </summary>
        public static List<Character> GetCharactersByGroup(string groupName)
        {
            var characters = new List<Character>();

            lock (_dbLock)
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();

                    var sql = "SELECT * FROM Characters WHERE GroupName = @GroupName ORDER BY ID";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@GroupName", groupName ?? "");

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                characters.Add(MapReaderToCharacter(reader));
                            }
                        }
                    }
                }
            }

            return characters;
        }

        /// <summary>
        /// Reset all status flags for characters with a different date
        /// </summary>
        public static void ResetDailyStatusForDate(string currentDate)
        {
            lock (_dbLock)
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();

                    var sql = @"
                        UPDATE Characters
                        SET
                            StatusVipPromotion = 0,
                            StatusDoiNangNo = 0,
                            StatusTriAn = 0,
                            StatusLatTheBai = 0,
                            StatusRutBo = 0,
                            StatusDoiKGDK = 0,
                            StatusTuHanh = 0,
                            StatusTruMa = 0,
                            StatusAoMaThap = 0,
                            StatusTrongCay = 0,
                            StatusCheMatBao = 0,
                            StatusAutoPhuBan = 0,
                            StatusUocNguyen = 0,
                            StatusNhanThuongHLVT = 0,
                            StatusNhanHoiPhuc = 0,
                            StatusMeTran = 0,
                            StatusHaiThuoc = 0,
                            StatusCauCa = 0,
                            StatusAutoThanTu = 0,
                            Date = @CurrentDate
                        WHERE Date != @CurrentDate";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CurrentDate", currentDate);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Get characters with incomplete tasks (for reporting)
        /// </summary>
        public static List<Character> GetCharactersWithIncompleteTasks()
        {
            var characters = new List<Character>();

            lock (_dbLock)
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();

                    var sql = @"
                        SELECT * FROM Characters
                        WHERE
                            (VipPromotion = 1 AND StatusVipPromotion = 0) OR
                            (DoiNangNo = 1 AND StatusDoiNangNo = 0) OR
                            (TriAn = 1 AND StatusTriAn = 0) OR
                            (LatTheBai = 1 AND StatusLatTheBai = 0) OR
                            (RutBo = 1 AND StatusRutBo = 0) OR
                            (DoiKGDK = 1 AND StatusDoiKGDK = 0) OR
                            (TuHanh = 1 AND StatusTuHanh = 0) OR
                            (TruMa = 1 AND StatusTruMa = 0) OR
                            (AoMaThap = 1 AND StatusAoMaThap = 0) OR
                            (TrongCay = 1 AND StatusTrongCay = 0) OR
                            (CheMatBao = 1 AND StatusCheMatBao = 0) OR
                            (AutoPhuBan = 1 AND StatusAutoPhuBan = 0) OR
                            (UocNguyen = 1 AND StatusUocNguyen = 0) OR
                            (NhanThuongHLVT = 1 AND StatusNhanThuongHLVT = 0) OR
                            (NhanHoiPhuc = 1 AND StatusNhanHoiPhuc = 0) OR
                            (MeTran = 1 AND StatusMeTran = 0) OR
                            (HaiThuoc = 1 AND StatusHaiThuoc = 0) OR
                            (CauCa = 1 AND StatusCauCa = 0) OR
                            (AutoThanTu = 1 AND StatusAutoThanTu = 0)
                        ORDER BY ID";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            characters.Add(MapReaderToCharacter(reader));
                        }
                    }
                }
            }

            return characters;
        }

        #region Helper Methods

        private static void AddCharacterParameters(SQLiteCommand cmd, Character c)
        {
            cmd.Parameters.AddWithValue("@ID", c.ID ?? "");
            cmd.Parameters.AddWithValue("@Link", c.Link ?? "");
            cmd.Parameters.AddWithValue("@GroupName", c.Group ?? "");
            cmd.Parameters.AddWithValue("@VipLevel", c.VipLevel);
            cmd.Parameters.AddWithValue("@IncreaseFPS", c.IncreaseFPS);
            cmd.Parameters.AddWithValue("@Date", c.Date ?? "");
            cmd.Parameters.AddWithValue("@IsChinese", c.IsChinese);

            cmd.Parameters.AddWithValue("@VipPromotion", c.VipPromotion);
            cmd.Parameters.AddWithValue("@DoiNangNo", c.DoiNangNo);
            cmd.Parameters.AddWithValue("@DoiNangNoNL4", c.DoiNangNoNL4);
            cmd.Parameters.AddWithValue("@DoiNangNoLoai", c.DoiNangNoLoai ?? "");
            cmd.Parameters.AddWithValue("@TrongNL", c.TrongNL);
            cmd.Parameters.AddWithValue("@TrongNLLoai", c.TrongNLLoai ?? "");
            cmd.Parameters.AddWithValue("@TriAn", c.TriAn);
            cmd.Parameters.AddWithValue("@LatTheBai", c.LatTheBai);
            cmd.Parameters.AddWithValue("@RutBo", c.RutBo);
            cmd.Parameters.AddWithValue("@DoiKGDK", c.DoiKGDK);
            cmd.Parameters.AddWithValue("@TuHanh", c.TuHanh);
            cmd.Parameters.AddWithValue("@TruMa", c.TruMa);
            cmd.Parameters.AddWithValue("@AoMaThap", c.AoMaThap);
            cmd.Parameters.AddWithValue("@TrongCay", c.TrongCay);
            cmd.Parameters.AddWithValue("@CheMatBao", c.CheMatBao);
            cmd.Parameters.AddWithValue("@CheMatBaoLoai", c.CheMatBaoLoai ?? "");
            cmd.Parameters.AddWithValue("@CheMatBaoCap", c.CheMatBaoCap);
            cmd.Parameters.AddWithValue("@AutoPhuBan", c.AutoPhuBan);
            cmd.Parameters.AddWithValue("@AutoPhuBanDanhSach", c.AutoPhuBanDanhSach ?? "");
            cmd.Parameters.AddWithValue("@DanhSTMTDanhSach", c.DanhSTMTDanhSach ?? "");
            cmd.Parameters.AddWithValue("@UocNguyen", c.UocNguyen);
            cmd.Parameters.AddWithValue("@DauPet", c.DauPet);
            cmd.Parameters.AddWithValue("@NhanThuongHLVT", c.NhanThuongHLVT);
            cmd.Parameters.AddWithValue("@NhanHoiPhuc", c.NhanHoiPhuc);
            cmd.Parameters.AddWithValue("@MeTran", c.MeTran);
            cmd.Parameters.AddWithValue("@HaiThuoc", c.HaiThuoc);
            cmd.Parameters.AddWithValue("@CauCa", c.CauCa);
            cmd.Parameters.AddWithValue("@ViTriNhanVat", c.ViTriNhanVat);
            cmd.Parameters.AddWithValue("@AutoThanTu", c.AutoThanTu);
            cmd.Parameters.AddWithValue("@RunToLast", c.RunToLast);
            cmd.Parameters.AddWithValue("@Running", c.Running);

            cmd.Parameters.AddWithValue("@StatusVipPromotion", c.StatusVipPromotion);
            cmd.Parameters.AddWithValue("@StatusDoiNangNo", c.StatusDoiNangNo);
            cmd.Parameters.AddWithValue("@StatusTriAn", c.StatusTriAn);
            cmd.Parameters.AddWithValue("@StatusLatTheBai", c.StatusLatTheBai);
            cmd.Parameters.AddWithValue("@StatusRutBo", c.StatusRutBo);
            cmd.Parameters.AddWithValue("@StatusDoiKGDK", c.StatusDoiKGDK);
            cmd.Parameters.AddWithValue("@StatusTuHanh", c.StatusTuHanh);
            cmd.Parameters.AddWithValue("@StatusTruMa", c.StatusTruMa);
            cmd.Parameters.AddWithValue("@StatusAoMaThap", c.StatusAoMaThap);
            cmd.Parameters.AddWithValue("@StatusTrongCay", c.StatusTrongCay);
            cmd.Parameters.AddWithValue("@StatusCheMatBao", c.StatusCheMatBao);
            cmd.Parameters.AddWithValue("@StatusAutoPhuBan", c.StatusAutoPhuBan);
            cmd.Parameters.AddWithValue("@StatusUocNguyen", c.StatusUocNguyen);
            cmd.Parameters.AddWithValue("@StatusNhanThuongHLVT", c.StatusNhanThuongHLVT);
            cmd.Parameters.AddWithValue("@StatusNhanHoiPhuc", c.StatusNhanHoiPhuc);
            cmd.Parameters.AddWithValue("@StatusMeTran", c.StatusMeTran);
            cmd.Parameters.AddWithValue("@StatusHaiThuoc", c.StatusHaiThuoc);
            cmd.Parameters.AddWithValue("@StatusCauCa", c.StatusCauCa);
            cmd.Parameters.AddWithValue("@StatusAutoThanTu", c.StatusAutoThanTu);
        }

        private static Character MapReaderToCharacter(SQLiteDataReader reader)
        {
            // Get date, use today's date if empty or invalid
            string dateStr = reader["Date"].ToString();
            if (string.IsNullOrEmpty(dateStr))
            {
                dateStr = DateTime.Today.ToString("dd/MM/yyyy");
            }

            return new Character
            {
                ID = reader["ID"].ToString(),
                Link = reader["Link"].ToString(),
                Group = reader["GroupName"].ToString(),
                VipLevel = Convert.ToInt32(reader["VipLevel"]),
                IncreaseFPS = Convert.ToInt32(reader["IncreaseFPS"]),
                Date = dateStr,
                IsChinese = Convert.ToInt32(reader["IsChinese"]),

                VipPromotion = Convert.ToInt32(reader["VipPromotion"]),
                DoiNangNo = Convert.ToInt32(reader["DoiNangNo"]),
                DoiNangNoNL4 = Convert.ToInt32(reader["DoiNangNoNL4"]),
                DoiNangNoLoai = reader["DoiNangNoLoai"].ToString(),
                TrongNL = Convert.ToInt32(reader["TrongNL"]),
                TrongNLLoai = reader["TrongNLLoai"].ToString(),
                TriAn = Convert.ToInt32(reader["TriAn"]),
                LatTheBai = Convert.ToInt32(reader["LatTheBai"]),
                RutBo = Convert.ToInt32(reader["RutBo"]),
                DoiKGDK = Convert.ToInt32(reader["DoiKGDK"]),
                TuHanh = Convert.ToInt32(reader["TuHanh"]),
                TruMa = Convert.ToInt32(reader["TruMa"]),
                AoMaThap = Convert.ToInt32(reader["AoMaThap"]),
                TrongCay = Convert.ToInt32(reader["TrongCay"]),
                CheMatBao = Convert.ToInt32(reader["CheMatBao"]),
                CheMatBaoLoai = reader["CheMatBaoLoai"].ToString(),
                CheMatBaoCap = Convert.ToInt32(reader["CheMatBaoCap"]),
                AutoPhuBan = Convert.ToInt32(reader["AutoPhuBan"]),
                AutoPhuBanDanhSach = reader["AutoPhuBanDanhSach"].ToString(),
                DanhSTMTDanhSach = reader["DanhSTMTDanhSach"].ToString(),
                UocNguyen = Convert.ToInt32(reader["UocNguyen"]),
                DauPet = Convert.ToInt32(reader["DauPet"]),
                NhanThuongHLVT = Convert.ToInt32(reader["NhanThuongHLVT"]),
                NhanHoiPhuc = Convert.ToInt32(reader["NhanHoiPhuc"]),
                MeTran = Convert.ToInt32(reader["MeTran"]),
                HaiThuoc = Convert.ToInt32(reader["HaiThuoc"]),
                CauCa = Convert.ToInt32(reader["CauCa"]),
                ViTriNhanVat = Convert.ToInt32(reader["ViTriNhanVat"]),
                AutoThanTu = Convert.ToInt32(reader["AutoThanTu"]),
                RunToLast = Convert.ToInt32(reader["RunToLast"]),
                Running = Convert.ToInt32(reader["Running"]),

                StatusVipPromotion = Convert.ToInt32(reader["StatusVipPromotion"]),
                StatusDoiNangNo = Convert.ToInt32(reader["StatusDoiNangNo"]),
                StatusTriAn = Convert.ToInt32(reader["StatusTriAn"]),
                StatusLatTheBai = Convert.ToInt32(reader["StatusLatTheBai"]),
                StatusRutBo = Convert.ToInt32(reader["StatusRutBo"]),
                StatusDoiKGDK = Convert.ToInt32(reader["StatusDoiKGDK"]),
                StatusTuHanh = Convert.ToInt32(reader["StatusTuHanh"]),
                StatusTruMa = Convert.ToInt32(reader["StatusTruMa"]),
                StatusAoMaThap = Convert.ToInt32(reader["StatusAoMaThap"]),
                StatusTrongCay = Convert.ToInt32(reader["StatusTrongCay"]),
                StatusCheMatBao = Convert.ToInt32(reader["StatusCheMatBao"]),
                StatusAutoPhuBan = Convert.ToInt32(reader["StatusAutoPhuBan"]),
                StatusUocNguyen = Convert.ToInt32(reader["StatusUocNguyen"]),
                StatusNhanThuongHLVT = Convert.ToInt32(reader["StatusNhanThuongHLVT"]),
                StatusNhanHoiPhuc = Convert.ToInt32(reader["StatusNhanHoiPhuc"]),
                StatusMeTran = Convert.ToInt32(reader["StatusMeTran"]),
                StatusHaiThuoc = Convert.ToInt32(reader["StatusHaiThuoc"]),
                StatusCauCa = Convert.ToInt32(reader["StatusCauCa"]),
                StatusAutoThanTu = Convert.ToInt32(reader["StatusAutoThanTu"])
            };
        }

        #endregion
    }
}
