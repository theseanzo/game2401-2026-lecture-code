using System;
using System.Collections.Generic;
using UnityEngine;
using _Project.Code.Core.Events;

namespace _Project.Code.Core.Services
{
    public static class PersistentServiceInitializer
    {
        private static GameObject _serviceRoot;
        private static readonly List<Type> _registeredServiceTypes = new();

        public static void RegisterPersistentService<T>() where T : MonoBehaviour, IMonoBehaviourService
        {
            _registeredServiceTypes.Add(typeof(T));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static async void Initialize()
        {
            Application.quitting += OnApplicationQuitting;

            _serviceRoot = new GameObject("[Services]");
            UnityEngine.Object.DontDestroyOnLoad(_serviceRoot);

            RegisterCoreServices();
            RegisterExternalServices();
            await ServiceLocator.InitializeAllAsync();

            Debug.Log("[PersistentServiceInitializer] Core services initialized.");
        }

        private static void RegisterCoreServices()
        {
            EventBusService eventBus = _serviceRoot.AddComponent<EventBusService>();
            eventBus.RegisterSelf();
        }

        private static void RegisterExternalServices()
        {
            foreach (Type serviceType in _registeredServiceTypes)
            {
                Component component = _serviceRoot.AddComponent(serviceType);
                if (component is IMonoBehaviourService service)
                {
                    service.RegisterSelf();
                }
            }
            _registeredServiceTypes.Clear();
        }

        private static void OnApplicationQuitting()
        {
            ServiceLocator.Clear();
        }
    }
}
