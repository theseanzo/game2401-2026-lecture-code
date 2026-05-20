using UnityEngine;
using _Project.Code.Core.Services;

namespace _Project.Code.Gameplay.Level
{
    /// <summary>
    /// Scene bootstrap: once the core services are ready, registers the scene's services so
    /// everything loads when the scene opens. Add this to a GameObject in the initial scene and
    /// assign the scene's <see cref="LevelService"/>.
    /// </summary>
    public class GameSceneInitializer : GameInitializer
    {
        [SerializeField] private LevelService _levelService;

        protected override async Awaitable RegisterSceneServices()
        {
            if (_levelService == null)
            {
                _levelService = FindFirstObjectByType<LevelService>();
            }

            if (_levelService == null)
            {
                Debug.LogError("[GameSceneInitializer] No LevelService found in scene.", this);
                return;
            }

            await ServiceLocator.RegisterSceneServiceAsync<ILevelService>(_levelService);
        }
    }
}
