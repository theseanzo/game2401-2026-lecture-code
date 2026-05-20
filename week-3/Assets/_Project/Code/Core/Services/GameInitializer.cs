using UnityEngine;

namespace _Project.Code.Core.Services
{
    public class GameInitializer : MonoBehaviour
    {
        protected virtual async void Start()
        {
            await ServiceLocator.WaitUntilAsync(() => ServiceLocator.IsInitialized);
            await RegisterSceneServices();
        }

#pragma warning disable CS1998 // Async method lacks 'await' - base implementation is intentionally empty
        protected virtual async Awaitable RegisterSceneServices() { }
#pragma warning restore CS1998

        protected virtual void OnDestroy()
        {
            UnregisterSceneServices();
        }

        protected virtual void UnregisterSceneServices() { }
    }
}
