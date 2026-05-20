using UnityEngine;

namespace _Project.Code.Core.Services
{
    public abstract class MonoBehaviourService<T> : MonoBehaviour, IMonoBehaviourService where T : class, IService
    {
        // Sealed to prevent subclasses from using Awake - all init must go through InitializeAsync
        protected void Awake() { }

#pragma warning disable CS1998 // Async method lacks 'await' - base implementation is intentionally empty
        public virtual async Awaitable InitializeAsync() { }
#pragma warning restore CS1998

        public void RegisterSelf() => ServiceLocator.Register<T>((T)(object)this);

        public void ReplaceService() => ServiceLocator.Replace<T>((T)(object)this);

        public void UnregisterSelf() => ServiceLocator.Unregister<T>();

        public virtual void Dispose()
        {
            if (this != null && gameObject != null)
            {
                Destroy(gameObject);
            }
        }
    }
}
