using AutoVPT.Interfaces;
using System;
using System.IO;
using System.Xml.Serialization;

namespace AutoVPT.Configuration
{
    /// <summary>
    /// Central configuration manager for the application.
    /// Provides strongly-typed access to application settings.
    /// </summary>
    public class ConfigurationManager
    {
        private static ConfigurationManager _instance;
        private static readonly object _lock = new object();

        private AppConfiguration _appConfig;
        private readonly string _configFilePath;

        private ConfigurationManager(string configFilePath = "config/appsettings.xml")
        {
            _configFilePath = configFilePath;
            LoadConfiguration();
        }

        public static ConfigurationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ConfigurationManager();
                        }
                    }
                }
                return _instance;
            }
        }

        public AppConfiguration AppConfig => _appConfig;

        public TimingSettings Timing => _appConfig.Timing;
        public PathSettings Paths => _appConfig.Paths;
        public ImageRecognitionSettings ImageRecognition => _appConfig.ImageRecognition;
        public FeatureStatusSettings FeatureStatus => _appConfig.FeatureStatus;
        public WindowSettings Window => _appConfig.Window;
        public LoopSettings Loop => _appConfig.Loop;

        private void LoadConfiguration()
        {
            try
            {
                if (File.Exists(_configFilePath))
                {
                    var serializer = new XmlSerializer(typeof(AppConfiguration));
                    using (var reader = new StreamReader(_configFilePath))
                    {
                        _appConfig = serializer.Deserialize(reader) as AppConfiguration;
                    }
                }
                else
                {
                    // Create default configuration
                    _appConfig = new AppConfiguration();
                    SaveConfiguration();
                }
            }
            catch (Exception)
            {
                // If loading fails, use default configuration
                _appConfig = new AppConfiguration();
            }
        }

        public void SaveConfiguration()
        {
            try
            {
                var directory = Path.GetDirectoryName(_configFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var serializer = new XmlSerializer(typeof(AppConfiguration));
                using (var writer = new StreamWriter(_configFilePath))
                {
                    serializer.Serialize(writer, _appConfig);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save configuration to {_configFilePath}", ex);
            }
        }

        public void ReloadConfiguration()
        {
            LoadConfiguration();
        }

        /// <summary>
        /// Initialize configuration manager with custom path (for testing)
        /// </summary>
        public static void Initialize(string configFilePath)
        {
            lock (_lock)
            {
                _instance = new ConfigurationManager(configFilePath);
            }
        }
    }
}
