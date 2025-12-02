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
                    // Already initialized, skip silently (idempotent)
                    return;
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

                    // Create list of feature executors
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
                // Determine resource path based on character's IsChinese flag
                string resourcePath = (_character.IsChinese == 1)
                    ? Libs.Constant.ChineseResourcePath
                    : Libs.Constant.ResourcePath;

                // Disable compression - it may affect image matching accuracy
                instance = new EmguCvImageRecognition(_windowHandle, enableCompression: false, resourcePath: resourcePath);
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

        /// <summary>
        /// Create a feature executor for this window
        /// </summary>
        public TExecutor CreateExecutor<TExecutor>(int vipLevel = 0) where TExecutor : class
        {
            var imageRecognition = GetService<IImageRecognition>();
            var inputSimulator = GetService<IInputSimulator>();
            var logger = GetService<ILogger>();

            var executorType = typeof(TExecutor);

            // Create executor based on type
            if (executorType == typeof(DoiNangNoExecutor))
            {
                return new DoiNangNoExecutor(imageRecognition, inputSimulator, logger) as TExecutor;
            }
            else if (executorType == typeof(TrongNLExecutor))
            {
                return new TrongNLExecutor(imageRecognition, inputSimulator, logger) as TExecutor;
            }
            else if (executorType == typeof(TriAnExecutor))
            {
                return new TriAnExecutor(imageRecognition, inputSimulator, logger, vipLevel) as TExecutor;
            }
            else if (executorType == typeof(CheMatBaoExecutor))
            {
                return new CheMatBaoExecutor(imageRecognition, inputSimulator, logger) as TExecutor;
            }
            else if (executorType == typeof(AutoPhuBanExecutor))
            {
                return new AutoPhuBanExecutor(imageRecognition, inputSimulator, logger) as TExecutor;
            }
            else if (executorType == typeof(TruMaExecutor))
            {
                return new TruMaExecutor(imageRecognition, inputSimulator, logger) as TExecutor;
            }
            else if (executorType == typeof(VipPromotionExecutor))
            {
                return new VipPromotionExecutor(imageRecognition, inputSimulator, logger) as TExecutor;
            }
            else if (executorType == typeof(NhanThuongHLVTExecutor))
            {
                return new NhanThuongHLVTExecutor(imageRecognition, inputSimulator, logger) as TExecutor;
            }
            else if (executorType == typeof(RutBoExecutor))
            {
                return new RutBoExecutor(imageRecognition, inputSimulator, logger) as TExecutor;
            }
            else if (executorType == typeof(DoiKGDKExecutor))
            {
                return new DoiKGDKExecutor(imageRecognition, inputSimulator, logger) as TExecutor;
            }
            else if (executorType == typeof(NhanHoiPhucExecutor))
            {
                return new NhanHoiPhucExecutor(imageRecognition, inputSimulator, logger) as TExecutor;
            }
            else if (executorType == typeof(TuHanhExecutor))
            {
                return new TuHanhExecutor(imageRecognition, inputSimulator, logger) as TExecutor;
            }
            else if (executorType == typeof(AutoThanTuExecutor))
            {
                return new AutoThanTuExecutor(imageRecognition, inputSimulator, logger) as TExecutor;
            }

            throw new InvalidOperationException($"Executor type {executorType.Name} not registered");
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
