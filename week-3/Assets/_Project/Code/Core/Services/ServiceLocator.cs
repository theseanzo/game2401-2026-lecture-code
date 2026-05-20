using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Code.Core.Services
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, IService> _services = new();
        private static readonly List<IService> _initializationOrder = new();
        private static bool _isInitialized;

        public static bool IsInitialized => _isInitialized;

        public static void Register<T>(T service) where T : class, IService
        {
            Type type = typeof(T);

            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"Service {type.Name} is already registered. Skipping.");
                return;
            }

            _services[type] = service;
            _initializationOrder.Add(service);
        }

        public static async Awaitable InitializeAllAsync()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("Services already initialized.");
                return;
            }

            foreach (IService service in _initializationOrder)
            {
                try
                {
                    await service.InitializeAsync();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to initialize service {service.GetType().Name}: {e.Message}");
                    throw;
                }
            }

            _isInitialized = true;
        }

        public static T Get<T>() where T : class, IService
        {
            if (!_isInitialized)
            {
                Debug.LogWarning($"[ServiceLocator] Accessing {typeof(T).Name} before initialization complete. Use WaitFor<T>() in async context.");
            }

            Type type = typeof(T);

            if (_services.TryGetValue(type, out IService service))
            {
                return (T)service;
            }

            Debug.LogError($"Service {type.Name} not found. Make sure it's registered.");
            return null;
        }

        public static async Awaitable<T> WaitFor<T>() where T : class, IService
        {
            while (!TryGet<T>(out _))
            {
                await Awaitable.NextFrameAsync();
            }
            return Get<T>();
        }

        public static async Awaitable WaitUntilAsync(Func<bool> condition)
        {
            while (!condition())
            {
                await Awaitable.NextFrameAsync();
            }
        }

        public static bool TryGet<T>(out T service) where T : class, IService
        {
            Type type = typeof(T);

            if (_services.TryGetValue(type, out IService foundService))
            {
                service = (T)foundService;
                return true;
            }

            service = null;
            return false;
        }

        public static async Awaitable RegisterSceneServiceAsync<T>(T service) where T : class, IService
        {
            if (!_isInitialized)
            {
                Debug.LogError("Cannot register scene service before core services are initialized.");
                return;
            }

            Type type = typeof(T);

            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"Service {type.Name} is already registered. Skipping.");
                return;
            }

            _services[type] = service;
            await service.InitializeAsync();
        }

        public static void Unregister<T>() where T : class, IService
        {
            Type type = typeof(T);

            if (_services.TryGetValue(type, out IService service))
            {
                service.Dispose();
                _services.Remove(type);
                _initializationOrder.Remove(service);
            }
        }

        public static void Replace<T>(T service) where T : class, IService
        {
            Type type = typeof(T);

            if (_services.TryGetValue(type, out IService existing))
            {
                existing.Dispose();
                _initializationOrder.Remove(existing);
            }

            _services[type] = service;
            _initializationOrder.Add(service);
        }

        public static void Clear()
        {
            foreach (IService service in _initializationOrder)
            {
                try
                {
                    service.Dispose();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to dispose service {service.GetType().Name}: {e.Message}");
                }
            }

            _services.Clear();
            _initializationOrder.Clear();
            _isInitialized = false;
        }
    }
}
