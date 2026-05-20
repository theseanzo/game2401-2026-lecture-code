using System.Collections.Generic;
using UnityEngine;
using _Project.Code.Core.Services;

namespace _Project.Code.Gameplay.Level
{
    /// <summary>
    /// Holds the loaded level grid and exposes it to other systems (movement, AI, gameplay).
    /// Registered in the ServiceLocator once the level has been parsed and spawned.
    /// </summary>
    public interface ILevelService : IService
    {
        int Width { get; }
        int Height { get; }
        Vector2Int PlayerSpawn { get; }
        IReadOnlyList<Vector2Int> GhostSpawns { get; }

        char GetTile(int x, int y);
        bool IsWalkable(int x, int y);
        Vector3 GridToWorld(Vector2Int cell);
    }
}
