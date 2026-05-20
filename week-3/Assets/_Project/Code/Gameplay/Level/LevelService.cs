using System.Collections.Generic;
using UnityEngine;
using _Project.Code.Core.Events;
using _Project.Code.Core.Services;

namespace _Project.Code.Gameplay.Level
{
    /// <summary>
    /// Scene service that loads its assigned <see cref="LevelData"/>, builds the grid, and exposes it.
    /// Comes online after the core services are initialized, then registers itself as a scene service.
    /// </summary>
    public class LevelService : MonoBehaviourService<ILevelService>, ILevelService
    {
        [SerializeField] private LevelData _level;

        private LevelGrid _grid;
        private Transform _levelContainer;

        public int Width => _grid?.Width ?? 0;
        public int Height => _grid?.Height ?? 0;
        public Vector2Int PlayerSpawn => _grid?.PlayerSpawn ?? new Vector2Int(-1, -1);
        public IReadOnlyList<Vector2Int> GhostSpawns => _grid?.GhostSpawns ?? new List<Vector2Int>();

        public char GetTile(int x, int y) => _grid != null ? _grid.GetTile(x, y) : LevelLegend.Floor;
        public bool IsWalkable(int x, int y) => _grid != null && _grid.IsWalkable(x, y);
        public Vector3 GridToWorld(Vector2Int cell) => LevelGrid.GridToWorld(cell);

#pragma warning disable CS1998 // Parsing and spawning are synchronous; the async signature is the service contract.
        public override async Awaitable InitializeAsync()
        {
            if (_level == null)
            {
                Debug.LogError("[LevelService] No LevelData assigned.", this);
                return;
            }

            if (_level.Legend == null)
            {
                Debug.LogError($"[LevelService] LevelData '{_level.LevelName}' has no LevelLegend assigned.", this);
                return;
            }

            _grid = LevelGrid.FromLayout(_level.Layout, _level.Legend);
            SpawnTiles(_level.Legend);
            PublishLoaded();
        }
#pragma warning restore CS1998

        private void SpawnTiles(LevelLegend legend)
        {
            GameObject container = new GameObject("[Level]");
            _levelContainer = container.transform;

            for (int y = 0; y < _grid.Height; y++)
            {
                for (int x = 0; x < _grid.Width; x++)
                {
                    char symbol = _grid.GetTile(x, y);
                    if (!legend.TryGetPrefab(symbol, out GameObject prefab))
                    {
                        continue;
                    }

                    Vector3 position = LevelGrid.GridToWorld(new Vector2Int(x, y));
                    Instantiate(prefab, position, Quaternion.identity, _levelContainer);
                }
            }
        }

        private void PublishLoaded()
        {
            if (!ServiceLocator.TryGet<IEventBus>(out IEventBus eventBus))
            {
                Debug.LogWarning("[LevelService] EventBus not available; LevelLoadedEvent not published.");
                return;
            }

            List<Vector2Int> ghostSpawns = new(_grid.GhostSpawns);
            eventBus.Publish(new LevelLoadedEvent
            {
                Width = _grid.Width,
                Height = _grid.Height,
                PlayerSpawn = _grid.PlayerSpawn,
                GhostSpawns = ghostSpawns.ToArray()
            });
        }
    }
}
