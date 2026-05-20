using UnityEngine;
using _Project.Code.Core.Events;

namespace _Project.Code.Gameplay.Level
{
    /// <summary>Published once a level has been parsed and spawned. Carries the grid size and spawn points.</summary>
    public struct LevelLoadedEvent : IEvent
    {
        public int Width;
        public int Height;
        public Vector2Int PlayerSpawn;
        public Vector2Int[] GhostSpawns;
    }
}
