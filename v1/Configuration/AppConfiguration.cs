using System;

namespace AutoVPT.Configuration
{
    /// <summary>
    /// Main application configuration with strongly-typed properties.
    /// Contains all timing, path, and feature settings.
    /// </summary>
    public class AppConfiguration
    {
        public string Version { get; set; } = "AutoVPT Version 1.0";

        public TimingSettings Timing { get; set; } = new TimingSettings();
        public PathSettings Paths { get; set; } = new PathSettings();
        public ImageRecognitionSettings ImageRecognition { get; set; } = new ImageRecognitionSettings();
        public FeatureStatusSettings FeatureStatus { get; set; } = new FeatureStatusSettings();
        public WindowSettings Window { get; set; } = new WindowSettings();
        public LoopSettings Loop { get; set; } = new LoopSettings();
    }

    public class TimingSettings
    {
        public int VeryShortDelayMs { get; set; } = 100;
        public int ShortDelayMs { get; set; } = 1000;
        public int MediumDelayMs { get; set; } = 3000;
        public int LongDelayMs { get; set; } = 5000;
        public int ImageSearchTimeoutMs { get; set; } = 2000;
    }

    public class PathSettings
    {
        public string ResourceBasePath { get; set; } = "resources";
        public string ChineseResourceBasePath { get; set; } = "cn_resources";
        public string DatabasePath { get; set; } = "database";
        public string LogPath { get; set; } = "logs";
        public string TrackingPath { get; set; } = "tracking";

        // Image folder paths
        public string PhuBanFolder { get; set; } = "/phu_ban/";
        public string MatBaoFolder { get; set; } = "/mat_bao/";
        public string CharNameFolder { get; set; } = "/char_name/";
        public string InMapFolder { get; set; } = "/in_map/";
        public string GlobalFolder { get; set; } = "/global/";
        public string TriAnFolder { get; set; } = "/tri_an/";
        public string TruMaFolder { get; set; } = "/tru_ma/";
        public string EventFolder { get; set; } = "/event/";
        public string DoiThoaiFolder { get; set; } = "/doi_thoai/";
        public string ViTriNPCFolder { get; set; } = "/vi_tri_npc/";
        public string BatPetFolder { get; set; } = "/bat_pet/";
        public string NVHNFolder { get; set; } = "/nvhn/";
        public string STMTFolder { get; set; } = "/stmt/";
        public string MapsFolder { get; set; } = "/maps/";

        /// <summary>
        /// Get full image path with server localization
        /// </summary>
        public string GetImagePath(string relativePath, bool isChinese = false)
        {
            var basePath = isChinese ? ChineseResourceBasePath : ResourceBasePath;
            return System.IO.Path.Combine(basePath, relativePath.TrimStart('/'));
        }
    }

    public class ImageRecognitionSettings
    {
        public double DefaultThreshold { get; set; } = 0.95;
        public double HighThreshold { get; set; } = 0.98;
        public double LowThreshold { get; set; } = 0.80;
        public bool EnableImageCache { get; set; } = true;
        public int MaxCacheSize { get; set; } = 100; // Max number of cached images
    }

    public class FeatureStatusSettings
    {
        public int Inactive { get; set; } = 0;
        public int Active { get; set; } = 1;
        public int Running { get; set; } = 2;
        public int Completed { get; set; } = 3;
    }

    public class WindowSettings
    {
        public int TopBarHeight { get; set; } = 25;
        public string FlashPlayerExecutable { get; set; } = "flash.exe";
        public string ForceBindIPExecutable { get; set; } = "ForceBindIP.exe";
    }

    public class LoopSettings
    {
        public int MaxLoopShort { get; set; } = 3;
        public int MaxLoopQ { get; set; } = 10;
        public int MaxLoop { get; set; } = 20;
    }
}
