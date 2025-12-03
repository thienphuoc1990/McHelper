using AutoVPT.Objects;
using System;
using System.Collections.Generic;

namespace AutoVPT.Domain
{
    /// <summary>
    /// Adapter that converts between legacy Character and new CharacterAggregate.
    /// Provides bidirectional mapping for backward compatibility.
    /// </summary>
    public static class CharacterAdapter
    {
        /// <summary>
        /// Convert legacy Character to new CharacterAggregate
        /// </summary>
        public static CharacterAggregate ToAggregate(Character legacyCharacter)
        {
            if (legacyCharacter == null)
                throw new ArgumentNullException(nameof(legacyCharacter));

            var aggregate = new CharacterAggregate();

            // Map Identity
            aggregate.Identity.Id = legacyCharacter.ID;
            aggregate.Identity.Link = legacyCharacter.Link;
            aggregate.Identity.Group = legacyCharacter.Group;
            aggregate.Identity.VipLevel = legacyCharacter.VipLevel;
            aggregate.Identity.IsChinese = (legacyCharacter.IsChinese == 1);
            aggregate.Identity.Position = legacyCharacter.ViTriNhanVat;
            aggregate.Identity.IncreaseFps = (legacyCharacter.IncreaseFPS == 1);

            // Map Feature Config
            MapFeatureConfig(legacyCharacter, aggregate.FeatureConfig);

            // Map Runtime State
            aggregate.RuntimeState.CharacterId = legacyCharacter.ID;
            aggregate.RuntimeState.Mode = (RunningMode)legacyCharacter.Running;
            aggregate.RuntimeState.RunToLast = (legacyCharacter.RunToLast == 1);

            // Parse date
            if (!string.IsNullOrEmpty(legacyCharacter.Date))
            {
                if (DateTime.TryParse(legacyCharacter.Date, out DateTime parsedDate))
                {
                    aggregate.RuntimeState.LastUpdated = parsedDate;
                }
            }

            // Map Feature Statuses
            MapFeatureStatuses(legacyCharacter, aggregate.RuntimeState);

            return aggregate;
        }

        /// <summary>
        /// Convert new CharacterAggregate back to legacy Character
        /// </summary>
        public static Character ToLegacy(CharacterAggregate aggregate)
        {
            if (aggregate == null)
                throw new ArgumentNullException(nameof(aggregate));

            var character = new Character();

            // Map Identity
            character.ID = aggregate.Identity.Id;
            character.Link = aggregate.Identity.Link;
            character.Group = aggregate.Identity.Group;
            character.VipLevel = aggregate.Identity.VipLevel;
            character.IsChinese = aggregate.Identity.IsChinese ? 1 : 0;
            character.ViTriNhanVat = aggregate.Identity.Position;
            character.IncreaseFPS = aggregate.Identity.IncreaseFps ? 1 : 0;

            // Map Runtime State
            character.Running = (int)aggregate.RuntimeState.Mode;
            character.RunToLast = aggregate.RuntimeState.RunToLast ? 1 : 0;
            character.Date = aggregate.RuntimeState.LastUpdated.ToString("yyyy-MM-dd");

            // Map Feature Config back to legacy properties
            MapToLegacyFeatureConfig(aggregate.FeatureConfig, character);

            // Map Feature Statuses back to legacy properties
            MapToLegacyStatuses(aggregate.RuntimeState, character);

            return character;
        }

        private static void MapFeatureConfig(Character legacy, CharacterFeatureConfig config)
        {
            // Map each feature enable flag and parameters
            config.Features[FeatureType.VipPromotion].Enabled = (legacy.VipPromotion == 1);

            config.Features[FeatureType.DoiNangNo].Enabled = (legacy.DoiNangNo == 1);
            config.Features[FeatureType.DoiNangNo].SetParameter("Loai", legacy.DoiNangNoLoai ?? "");

            config.Features[FeatureType.DoiNangNoNL4].Enabled = (legacy.DoiNangNoNL4 == 1);

            config.Features[FeatureType.TrongNL].Enabled = (legacy.TrongNL == 1);
            config.Features[FeatureType.TrongNL].SetParameter("Loai", legacy.TrongNLLoai ?? "");

            config.Features[FeatureType.TriAn].Enabled = (legacy.TriAn == 1);
            config.Features[FeatureType.LatTheBai].Enabled = (legacy.LatTheBai == 1);
            config.Features[FeatureType.RutBo].Enabled = (legacy.RutBo == 1);
            config.Features[FeatureType.DoiKGDK].Enabled = (legacy.DoiKGDK == 1);
            config.Features[FeatureType.TuHanh].Enabled = (legacy.TuHanh == 1);
            config.Features[FeatureType.TruMa].Enabled = (legacy.TruMa == 1);
            config.Features[FeatureType.AoMaThap].Enabled = (legacy.AoMaThap == 1);
            config.Features[FeatureType.TrongCay].Enabled = (legacy.TrongCay == 1);

            config.Features[FeatureType.CheMatBao].Enabled = (legacy.CheMatBao == 1);
            config.Features[FeatureType.CheMatBao].SetParameter("Loai", legacy.CheMatBaoLoai ?? "");
            config.Features[FeatureType.CheMatBao].SetParameter("Cap", legacy.CheMatBaoCap.ToString());

            config.Features[FeatureType.AutoPhuBan].Enabled = (legacy.AutoPhuBan == 1);
            config.Features[FeatureType.AutoPhuBan].SetParameter("DanhSach", legacy.AutoPhuBanDanhSach ?? "");
            config.Features[FeatureType.AutoPhuBan].SetParameter("DanhSTMT", legacy.DanhSTMTDanhSach ?? "");

            config.Features[FeatureType.UocNguyen].Enabled = (legacy.UocNguyen == 1);
            config.Features[FeatureType.DauPet].Enabled = (legacy.DauPet == 1);
            config.Features[FeatureType.NhanThuongHLVT].Enabled = (legacy.NhanThuongHLVT == 1);
            // config.Features[FeatureType.BugOnline].Enabled = (legacy.BugOnline == 1); // Property may not exist
            config.Features[FeatureType.MeTran].Enabled = (legacy.MeTran == 1);
            config.Features[FeatureType.HaiThuoc].Enabled = (legacy.HaiThuoc == 1);
            config.Features[FeatureType.CauCa].Enabled = (legacy.CauCa == 1);
            config.Features[FeatureType.AutoThanTu].Enabled = (legacy.AutoThanTu == 1);
        }

        private static void MapFeatureStatuses(Character legacy, CharacterRuntimeState state)
        {
            // Map each status
            state.FeatureStatuses[FeatureType.VipPromotion] = (FeatureExecutionStatus)legacy.StatusVipPromotion;
            state.FeatureStatuses[FeatureType.DoiNangNo] = (FeatureExecutionStatus)legacy.StatusDoiNangNo;
            state.FeatureStatuses[FeatureType.TriAn] = (FeatureExecutionStatus)legacy.StatusTriAn;
            state.FeatureStatuses[FeatureType.LatTheBai] = (FeatureExecutionStatus)legacy.StatusLatTheBai;
            state.FeatureStatuses[FeatureType.RutBo] = (FeatureExecutionStatus)legacy.StatusRutBo;
            state.FeatureStatuses[FeatureType.DoiKGDK] = (FeatureExecutionStatus)legacy.StatusDoiKGDK;
            state.FeatureStatuses[FeatureType.TuHanh] = (FeatureExecutionStatus)legacy.StatusTuHanh;
            state.FeatureStatuses[FeatureType.TruMa] = (FeatureExecutionStatus)legacy.StatusTruMa;
            state.FeatureStatuses[FeatureType.AoMaThap] = (FeatureExecutionStatus)legacy.StatusAoMaThap;
            state.FeatureStatuses[FeatureType.TrongCay] = (FeatureExecutionStatus)legacy.StatusTrongCay;
            state.FeatureStatuses[FeatureType.CheMatBao] = (FeatureExecutionStatus)legacy.StatusCheMatBao;
            state.FeatureStatuses[FeatureType.AutoPhuBan] = (FeatureExecutionStatus)legacy.StatusAutoPhuBan;
            state.FeatureStatuses[FeatureType.UocNguyen] = (FeatureExecutionStatus)legacy.StatusUocNguyen;
            state.FeatureStatuses[FeatureType.NhanThuongHLVT] = (FeatureExecutionStatus)legacy.StatusNhanThuongHLVT;
            state.FeatureStatuses[FeatureType.MeTran] = (FeatureExecutionStatus)legacy.StatusMeTran;
            state.FeatureStatuses[FeatureType.HaiThuoc] = (FeatureExecutionStatus)legacy.StatusHaiThuoc;
            state.FeatureStatuses[FeatureType.CauCa] = (FeatureExecutionStatus)legacy.StatusCauCa;
            state.FeatureStatuses[FeatureType.AutoThanTu] = (FeatureExecutionStatus)legacy.StatusAutoThanTu;
            state.FeatureStatuses[FeatureType.NhanHoiPhuc] = (FeatureExecutionStatus)legacy.StatusNhanHoiPhuc;
        }

        private static void MapToLegacyFeatureConfig(CharacterFeatureConfig config, Character character)
        {
            // Map each feature back
            character.VipPromotion = config.IsEnabled(FeatureType.VipPromotion) ? 1 : 0;

            character.DoiNangNo = config.IsEnabled(FeatureType.DoiNangNo) ? 1 : 0;
            character.DoiNangNoLoai = config.GetConfig(FeatureType.DoiNangNo).GetParameter("Loai");

            character.DoiNangNoNL4 = config.IsEnabled(FeatureType.DoiNangNoNL4) ? 1 : 0;

            character.TrongNL = config.IsEnabled(FeatureType.TrongNL) ? 1 : 0;
            character.TrongNLLoai = config.GetConfig(FeatureType.TrongNL).GetParameter("Loai");

            character.TriAn = config.IsEnabled(FeatureType.TriAn) ? 1 : 0;
            character.LatTheBai = config.IsEnabled(FeatureType.LatTheBai) ? 1 : 0;
            character.RutBo = config.IsEnabled(FeatureType.RutBo) ? 1 : 0;
            character.DoiKGDK = config.IsEnabled(FeatureType.DoiKGDK) ? 1 : 0;
            character.TuHanh = config.IsEnabled(FeatureType.TuHanh) ? 1 : 0;
            character.TruMa = config.IsEnabled(FeatureType.TruMa) ? 1 : 0;
            character.AoMaThap = config.IsEnabled(FeatureType.AoMaThap) ? 1 : 0;
            character.TrongCay = config.IsEnabled(FeatureType.TrongCay) ? 1 : 0;

            character.CheMatBao = config.IsEnabled(FeatureType.CheMatBao) ? 1 : 0;
            character.CheMatBaoLoai = config.GetConfig(FeatureType.CheMatBao).GetParameter("Loai");
            int.TryParse(config.GetConfig(FeatureType.CheMatBao).GetParameter("Cap", "0"), out int cap);
            character.CheMatBaoCap = cap;

            character.AutoPhuBan = config.IsEnabled(FeatureType.AutoPhuBan) ? 1 : 0;
            character.AutoPhuBanDanhSach = config.GetConfig(FeatureType.AutoPhuBan).GetParameter("DanhSach");
            character.DanhSTMTDanhSach = config.GetConfig(FeatureType.AutoPhuBan).GetParameter("DanhSTMT");

            character.UocNguyen = config.IsEnabled(FeatureType.UocNguyen) ? 1 : 0;
            character.DauPet = config.IsEnabled(FeatureType.DauPet) ? 1 : 0;
            character.NhanThuongHLVT = config.IsEnabled(FeatureType.NhanThuongHLVT) ? 1 : 0;
            // character.BugOnline = config.IsEnabled(FeatureType.BugOnline) ? 1 : 0; // Property may not exist
            character.MeTran = config.IsEnabled(FeatureType.MeTran) ? 1 : 0;
            character.HaiThuoc = config.IsEnabled(FeatureType.HaiThuoc) ? 1 : 0;
            character.CauCa = config.IsEnabled(FeatureType.CauCa) ? 1 : 0;
            character.AutoThanTu = config.IsEnabled(FeatureType.AutoThanTu) ? 1 : 0;
        }

        private static void MapToLegacyStatuses(CharacterRuntimeState state, Character character)
        {
            // Map each status back
            character.StatusVipPromotion = (int)state.GetStatus(FeatureType.VipPromotion);
            character.StatusDoiNangNo = (int)state.GetStatus(FeatureType.DoiNangNo);
            character.StatusTriAn = (int)state.GetStatus(FeatureType.TriAn);
            character.StatusLatTheBai = (int)state.GetStatus(FeatureType.LatTheBai);
            character.StatusRutBo = (int)state.GetStatus(FeatureType.RutBo);
            character.StatusDoiKGDK = (int)state.GetStatus(FeatureType.DoiKGDK);
            character.StatusTuHanh = (int)state.GetStatus(FeatureType.TuHanh);
            character.StatusTruMa = (int)state.GetStatus(FeatureType.TruMa);
            character.StatusAoMaThap = (int)state.GetStatus(FeatureType.AoMaThap);
            character.StatusTrongCay = (int)state.GetStatus(FeatureType.TrongCay);
            character.StatusCheMatBao = (int)state.GetStatus(FeatureType.CheMatBao);
            character.StatusAutoPhuBan = (int)state.GetStatus(FeatureType.AutoPhuBan);
            character.StatusUocNguyen = (int)state.GetStatus(FeatureType.UocNguyen);
            character.StatusNhanThuongHLVT = (int)state.GetStatus(FeatureType.NhanThuongHLVT);
            character.StatusMeTran = (int)state.GetStatus(FeatureType.MeTran);
            character.StatusHaiThuoc = (int)state.GetStatus(FeatureType.HaiThuoc);
            character.StatusCauCa = (int)state.GetStatus(FeatureType.CauCa);
            character.StatusAutoThanTu = (int)state.GetStatus(FeatureType.AutoThanTu);
            character.StatusNhanHoiPhuc = (int)state.GetStatus(FeatureType.NhanHoiPhuc);
        }
    }
}
