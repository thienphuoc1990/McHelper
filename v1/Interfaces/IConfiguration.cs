using System;

namespace AutoVPT.Interfaces
{
    /// <summary>
    /// Configuration abstraction for application settings.
    /// Provides type-safe access to configuration values.
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        /// Get configuration value by key with optional default
        /// </summary>
        T GetValue<T>(string key, T defaultValue = default);

        /// <summary>
        /// Set configuration value by key
        /// </summary>
        void SetValue<T>(string key, T value);

        /// <summary>
        /// Check if configuration key exists
        /// </summary>
        bool HasKey(string key);

        /// <summary>
        /// Save configuration changes to persistent storage
        /// </summary>
        void Save();

        /// <summary>
        /// Reload configuration from persistent storage
        /// </summary>
        void Reload();
    }
}
