using AutoVPT.Configuration;
using AutoVPT.Infrastructure;
using AutoVPT.Interfaces;
using AutoVPT.Objects;
using AutoVPT.Repositories;
using AutoVPT.Services;
using AutoVPT.Services.Executors;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AutoVPT.DependencyInjection
{
    /// <summary>
    /// Static service container for application-wide dependency injection.
    /// Provides easy access to configured services without Microsoft.Extensions.DependencyInjection.
    /// </summary>
    public static class ServiceContainer
    {
        private static SimpleServiceProvider _provider;
        private static readonly object _lock = new object();

        /// <summary>
        /// Initialize the service container (call once at application startup)
        /// </summary>
        public static void Initialize(TextBox statusTextBox = null)
        {
            lock (_lock)
            {
                if (_provider != null)
                {
                    throw new InvalidOperationException("Service container already initialized");
                }

                _provider = new SimpleServiceProvider();

                // Register configuration
                _provider.RegisterSingleton(sp => ConfigurationManager.Instance);

                // Register logger
                _provider.RegisterSingleton<ILogger>(sp =>
                {
                    var composite = new CompositeLogger();
                    var config = sp.GetService<ConfigurationManager>();

                    composite.AddLogger(new FileLogger(config.Paths.LogPath + "/automation.log"));

                    if (statusTextBox != null)
                    {
                        composite.AddLogger(new UiLogger(statusTextBox));
                    }

                    #if DEBUG
                    composite.AddLogger(new DebugLogger());
                    #endif

                    return composite;
                });

                // Register repositories
                _provider.RegisterTransient<ICharacterRepository>(sp =>
                {
                    var config = sp.GetService<ConfigurationManager>();
                    return new XmlCharacterRepository(config.Paths.DatabasePath);
                });

                // Register window manager
                _provider.RegisterSingleton<IWindowManager>(sp => new Win32WindowManager());

                // Register automation service
                _provider.RegisterTransient<IAutomationService>(sp =>
                {
                    var characterRepo = sp.GetService<ICharacterRepository>();
                    var windowManager = sp.GetService<IWindowManager>();
                    var logger = sp.GetService<ILogger>();

                    // Create empty list of executors for now
                    // In practice, you'd register executors here
                    var executors = new List<IFeatureExecutor>();

                    return new AutomationService(characterRepo, windowManager, logger, executors);
                });
            }
        }

        /// <summary>
        /// Get a service from the container
        /// </summary>
        public static TService GetService<TService>() where TService : class
        {
            if (_provider == null)
            {
                throw new InvalidOperationException("Service container not initialized. Call Initialize() first.");
            }

            return _provider.GetService<TService>();
        }

        /// <summary>
        /// Create services for a specific window (per-character automation)
        /// </summary>
        public static WindowServiceProvider CreateWindowServices(IntPtr windowHandle, Character character)
        {
            if (_provider == null)
            {
                throw new InvalidOperationException("Service container not initialized");
            }

            return new WindowServiceProvider(windowHandle, character, _provider);
        }

        /// <summary>
        /// Reset the container (for testing)
        /// </summary>
        public static void Reset()
        {
            lock (_lock)
            {
                _provider?.Dispose();
                _provider = null;
            }
        }
    }

    /// <summary>
    /// Per-window service provider for character-specific automation
    /// </summary>
    public class WindowServiceProvider : IDisposable
    {
        private readonly IntPtr _windowHandle;
        private readonly Objects.Character _character;
        private readonly SimpleServiceProvider _rootProvider;
        private readonly Dictionary<Type, object> _instances;

        public WindowServiceProvider(IntPtr windowHandle, Objects.Character character, SimpleServiceProvider rootProvider)
        {
            _windowHandle = windowHandle;
            _character = character;
            _rootProvider = rootProvider;
            _instances = new Dictionary<Type, object>();
        }

        public TService GetService<TService>() where TService : class
        {
            var type = typeof(TService);

            // Return cached instance if exists
            if (_instances.ContainsKey(type))
            {
                return (TService)_instances[type];
            }

            // Create per-window services
            object instance = null;

            if (type == typeof(IImageRecognition))
            {
                instance = new EmguCvImageRecognition(_windowHandle);
            }
            else if (type == typeof(IInputSimulator))
            {
                instance = new Win32InputSimulator(_windowHandle);
            }
            else if (type == typeof(ILogger))
            {
                instance = _rootProvider.GetService<ILogger>();
            }
            else if (type == typeof(IWindowManager))
            {
                instance = _rootProvider.GetService<IWindowManager>();
            }
            else if (type == typeof(ICharacterRepository))
            {
                instance = _rootProvider.GetService<ICharacterRepository>();
            }
            else
            {
                // Try to get from root provider
                instance = _rootProvider.GetService<TService>();
            }

            if (instance != null)
            {
                _instances[type] = instance;
                return (TService)instance;
            }

            throw new InvalidOperationException($"Service {typeof(TService).Name} not available");
        }

        public void Dispose()
        {
            foreach (var instance in _instances.Values)
            {
                if (instance is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _instances.Clear();
        }
    }
}
