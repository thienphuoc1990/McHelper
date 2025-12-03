-- McHelper SQLite Database Schema
-- Migration from XML to SQLite storage

-- Main Characters table
-- Stores all character properties in a single normalized table
CREATE TABLE IF NOT EXISTS Characters (
    -- Primary key
    ID TEXT PRIMARY KEY NOT NULL,

    -- Basic info
    Link TEXT NOT NULL DEFAULT '',
    GroupName TEXT DEFAULT '',
    VipLevel INTEGER DEFAULT 0,
    IncreaseFPS INTEGER DEFAULT 0,
    Date TEXT DEFAULT '',
    IsChinese INTEGER DEFAULT 0,

    -- Feature enable flags (1 = enabled, 0 = disabled)
    VipPromotion INTEGER DEFAULT 0,
    DoiNangNo INTEGER DEFAULT 0,
    DoiNangNoNL4 INTEGER DEFAULT 0,
    DoiNangNoLoai TEXT DEFAULT '',
    TrongNL INTEGER DEFAULT 0,
    TrongNLLoai TEXT DEFAULT '',
    TriAn INTEGER DEFAULT 0,
    LatTheBai INTEGER DEFAULT 0,
    RutBo INTEGER DEFAULT 0,
    DoiKGDK INTEGER DEFAULT 0,
    TuHanh INTEGER DEFAULT 0,
    TruMa INTEGER DEFAULT 0,
    AoMaThap INTEGER DEFAULT 0,
    TrongCay INTEGER DEFAULT 0,
    CheMatBao INTEGER DEFAULT 0,
    CheMatBaoLoai TEXT DEFAULT '',
    CheMatBaoCap INTEGER DEFAULT 0,
    AutoPhuBan INTEGER DEFAULT 0,
    AutoPhuBanDanhSach TEXT DEFAULT '',
    DanhSTMTDanhSach TEXT DEFAULT '',
    UocNguyen INTEGER DEFAULT 0,
    DauPet INTEGER DEFAULT 0,
    NhanThuongHLVT INTEGER DEFAULT 0,
    NhanHoiPhuc INTEGER DEFAULT 0,
    MeTran INTEGER DEFAULT 0,
    HaiThuoc INTEGER DEFAULT 0,
    CauCa INTEGER DEFAULT 0,
    ViTriNhanVat INTEGER DEFAULT 1,
    AutoThanTu INTEGER DEFAULT 0,
    RunToLast INTEGER DEFAULT 0,

    -- Runtime state (non-persistent, for in-memory use)
    Running INTEGER DEFAULT 0,

    -- Feature status flags (0 = not done, 1 = completed today)
    StatusVipPromotion INTEGER DEFAULT 0,
    StatusDoiNangNo INTEGER DEFAULT 0,
    StatusTriAn INTEGER DEFAULT 0,
    StatusLatTheBai INTEGER DEFAULT 0,
    StatusRutBo INTEGER DEFAULT 0,
    StatusDoiKGDK INTEGER DEFAULT 0,
    StatusTuHanh INTEGER DEFAULT 0,
    StatusTruMa INTEGER DEFAULT 0,
    StatusAoMaThap INTEGER DEFAULT 0,
    StatusTrongCay INTEGER DEFAULT 0,
    StatusCheMatBao INTEGER DEFAULT 0,
    StatusAutoPhuBan INTEGER DEFAULT 0,
    StatusUocNguyen INTEGER DEFAULT 0,
    StatusNhanThuongHLVT INTEGER DEFAULT 0,
    StatusNhanHoiPhuc INTEGER DEFAULT 0,
    StatusMeTran INTEGER DEFAULT 0,
    StatusHaiThuoc INTEGER DEFAULT 0,
    StatusCauCa INTEGER DEFAULT 0,
    StatusAutoThanTu INTEGER DEFAULT 0,

    -- Metadata
    CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TEXT DEFAULT CURRENT_TIMESTAMP
);

-- Index for common queries
CREATE INDEX IF NOT EXISTS idx_characters_group ON Characters(GroupName);
CREATE INDEX IF NOT EXISTS idx_characters_date ON Characters(Date);

-- Trigger to update UpdatedAt timestamp
CREATE TRIGGER IF NOT EXISTS update_characters_timestamp
AFTER UPDATE ON Characters
BEGIN
    UPDATE Characters SET UpdatedAt = CURRENT_TIMESTAMP WHERE ID = NEW.ID;
END;

-- View for quick status overview
CREATE VIEW IF NOT EXISTS v_CharacterStatus AS
SELECT
    ID,
    GroupName,
    Date,
    -- Count completed features
    (StatusVipPromotion + StatusDoiNangNo + StatusTriAn + StatusLatTheBai +
     StatusRutBo + StatusDoiKGDK + StatusTuHanh + StatusTruMa + StatusAoMaThap +
     StatusTrongCay + StatusCheMatBao + StatusAutoPhuBan + StatusUocNguyen +
     StatusNhanThuongHLVT + StatusNhanHoiPhuc + StatusMeTran + StatusHaiThuoc +
     StatusCauCa + StatusAutoThanTu) as CompletedFeatures,
    -- Count enabled features
    (VipPromotion + DoiNangNo + TriAn + LatTheBai + RutBo + DoiKGDK + TuHanh +
     TruMa + AoMaThap + TrongCay + CheMatBao + AutoPhuBan + UocNguyen +
     NhanThuongHLVT + NhanHoiPhuc + MeTran + HaiThuoc + CauCa + AutoThanTu) as EnabledFeatures
FROM Characters;

-- View for reporting: Characters with incomplete daily tasks
CREATE VIEW IF NOT EXISTS v_IncompleteTasks AS
SELECT
    ID,
    GroupName,
    CASE WHEN VipPromotion = 1 AND StatusVipPromotion = 0 THEN 'VipPromotion, ' ELSE '' END ||
    CASE WHEN DoiNangNo = 1 AND StatusDoiNangNo = 0 THEN 'DoiNangNo, ' ELSE '' END ||
    CASE WHEN TriAn = 1 AND StatusTriAn = 0 THEN 'TriAn, ' ELSE '' END ||
    CASE WHEN LatTheBai = 1 AND StatusLatTheBai = 0 THEN 'LatTheBai, ' ELSE '' END ||
    CASE WHEN RutBo = 1 AND StatusRutBo = 0 THEN 'RutBo, ' ELSE '' END ||
    CASE WHEN DoiKGDK = 1 AND StatusDoiKGDK = 0 THEN 'DoiKGDK, ' ELSE '' END ||
    CASE WHEN TuHanh = 1 AND StatusTuHanh = 0 THEN 'TuHanh, ' ELSE '' END ||
    CASE WHEN TruMa = 1 AND StatusTruMa = 0 THEN 'TruMa, ' ELSE '' END ||
    CASE WHEN AoMaThap = 1 AND StatusAoMaThap = 0 THEN 'AoMaThap, ' ELSE '' END ||
    CASE WHEN TrongCay = 1 AND StatusTrongCay = 0 THEN 'TrongCay, ' ELSE '' END ||
    CASE WHEN CheMatBao = 1 AND StatusCheMatBao = 0 THEN 'CheMatBao, ' ELSE '' END ||
    CASE WHEN AutoPhuBan = 1 AND StatusAutoPhuBan = 0 THEN 'AutoPhuBan, ' ELSE '' END ||
    CASE WHEN UocNguyen = 1 AND StatusUocNguyen = 0 THEN 'UocNguyen, ' ELSE '' END ||
    CASE WHEN NhanThuongHLVT = 1 AND StatusNhanThuongHLVT = 0 THEN 'NhanThuongHLVT, ' ELSE '' END ||
    CASE WHEN NhanHoiPhuc = 1 AND StatusNhanHoiPhuc = 0 THEN 'NhanHoiPhuc, ' ELSE '' END ||
    CASE WHEN MeTran = 1 AND StatusMeTran = 0 THEN 'MeTran, ' ELSE '' END ||
    CASE WHEN HaiThuoc = 1 AND StatusHaiThuoc = 0 THEN 'HaiThuoc, ' ELSE '' END ||
    CASE WHEN CauCa = 1 AND StatusCauCa = 0 THEN 'CauCa, ' ELSE '' END ||
    CASE WHEN AutoThanTu = 1 AND StatusAutoThanTu = 0 THEN 'AutoThanTu, ' ELSE '' END AS PendingTasks
FROM Characters
WHERE PendingTasks != '';
