using System;
using System.Collections.Generic;

namespace AutoVPT.DependencyInjection
{
    /// <summary>
    /// Simple lightweight dependency injection container.
    /// No external dependencies - pure .NET Framework implementation.
    /// </summary>
    public class SimpleServiceProvider : IDisposable
    {
        private readonly Dictionary<Type, Func<SimpleServiceProvider, object>> _factories;
        private readonly Dictionary<Type, object> _singletons;
        private bool _disposed;

        public SimpleServiceProvider()
        {
            _factories = new Dictionary<Type, Func<SimpleServiceProvider, object>>();
            _singletons = new Dictionary<Type, object>();
        }

        /// <summary>
        /// Register a singleton service
        /// </summary>
        public void RegisterSingleton<TService>(Func<SimpleServiceProvider, TService> factory) where TService : class
        {
            _factories[typeof(TService)] = sp => factory(sp);
        }

        /// <summary>
        /// Register a singleton instance
        /// </summary>
        public void RegisterSingleton<TService>(TService instance) where TService : class
        {
            _singletons[typeof(TService)] = instance;
        }

        /// <summary>
        /// Register a transient service (new instance each time)
        /// </summary>
        public void RegisterTransient<TService>(Func<SimpleServiceProvider, TService> factory) where TService : class
        {
            _factories[typeof(TService)] = sp => factory(sp);
        }

        /// <summary>
        /// Get a service from the container
        /// </summary>
        public TService GetService<TService>() where TService : class
        {
            var type = typeof(TService);

            // Check if singleton instance already exists
            if (_singletons.ContainsKey(type))
            {
                return (TService)_singletons[type];
            }

            // Check if factory exists
            if (_factories.ContainsKey(type))
            {
                var factory = _factories[type];
                var instance = factory(this);

                // Cache singletons
                if (!_factories.ContainsKey(type) || _singletons.ContainsKey(type))
                {
                    _singletons[type] = instance;
                }

                return (TService)instance;
            }

            throw new InvalidOperationException($"Service {typeof(TService).Name} not registered");
        }

        public void Dispose()
        {
            if (_disposed) return;

            // Dispose all disposable singletons
            foreach (var singleton in _singletons.Values)
            {
                if (singleton is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _singletons.Clear();
            _factories.Clear();
            _disposed = true;
        }
    }
}
