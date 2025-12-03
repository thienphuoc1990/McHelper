using AutoVPT.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace AutoVPT.Configuration
{
    /// <summary>
    /// XML-based configuration implementation.
    /// Stores configuration in an XML file with automatic persistence.
    /// Uses XmlSerializer for consistency with existing Character serialization.
    /// </summary>
    public class XmlConfigurationStore : IConfiguration
    {
        private readonly string _configFilePath;
        private Dictionary<string, string> _values;
        private readonly object _lock = new object();

        public XmlConfigurationStore(string configFilePath)
        {
            if (string.IsNullOrWhiteSpace(configFilePath))
                throw new ArgumentNullException(nameof(configFilePath));

            _configFilePath = configFilePath;
            _values = new Dictionary<string, string>();

            // Load existing configuration or create new
            if (File.Exists(_configFilePath))
            {
                Reload();
            }
            else
            {
                // Create default configuration
                Save();
            }
        }

        public T GetValue<T>(string key, T defaultValue = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            lock (_lock)
            {
                if (!_values.ContainsKey(key))
                    return defaultValue;

                try
                {
                    var stringValue = _values[key];

                    // Handle null or empty
                    if (string.IsNullOrEmpty(stringValue))
                        return defaultValue;

                    // Special handling for common types
                    var targetType = typeof(T);

                    if (targetType == typeof(bool))
                    {
                        return (T)(object)bool.Parse(stringValue);
                    }
                    if (targetType == typeof(int))
                    {
                        return (T)(object)int.Parse(stringValue);
                    }
                    if (targetType == typeof(double))
                    {
                        return (T)(object)double.Parse(stringValue);
                    }
                    if (targetType == typeof(string))
                    {
                        return (T)(object)stringValue;
                    }

                    return (T)Convert.ChangeType(stringValue, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
        }

        public void SetValue<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            lock (_lock)
            {
                _values[key] = value?.ToString() ?? string.Empty;
            }
        }

        public bool HasKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            lock (_lock)
            {
                return _values.ContainsKey(key);
            }
        }

        public void Save()
        {
            lock (_lock)
            {
                try
                {
                    var directory = Path.GetDirectoryName(_configFilePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    var serializer = new XmlSerializer(typeof(SerializableDictionary));
                    var serializableDict = new SerializableDictionary { Items = _values };

                    using (var writer = new StreamWriter(_configFilePath))
                    {
                        serializer.Serialize(writer, serializableDict);
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to save configuration to {_configFilePath}", ex);
                }
            }
        }

        public void Reload()
        {
            lock (_lock)
            {
                try
                {
                    if (!File.Exists(_configFilePath))
                    {
                        _values = new Dictionary<string, string>();
                        return;
                    }

                    var serializer = new XmlSerializer(typeof(SerializableDictionary));

                    using (var reader = new StreamReader(_configFilePath))
                    {
                        var serializableDict = serializer.Deserialize(reader) as SerializableDictionary;
                        _values = serializableDict?.Items ?? new Dictionary<string, string>();
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to load configuration from {_configFilePath}", ex);
                }
            }
        }

        [Serializable]
        public class SerializableDictionary
        {
            public Dictionary<string, string> Items { get; set; } = new Dictionary<string, string>();
        }
    }
}
