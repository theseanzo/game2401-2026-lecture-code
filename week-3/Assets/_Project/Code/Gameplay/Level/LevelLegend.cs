using System.Collections.Generic;
using UnityEngine;

namespace _Project.Code.Gameplay.Level
{
    /// <summary>
    /// Maps level-layout characters to the prefabs that represent them, and records which
    /// characters are walkable. Symbols may mean terrain (wall, floor) or an actor/item spawned
    /// on the cell (player, ghost, collectable). Authored as a reusable asset so levels share one legend.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelLegend", menuName = "Game/Level Legend")]
    public class LevelLegend : ScriptableObject
    {
        // Canonical layout symbols. Floor is the implicit default for anything not otherwise mapped.
        public const char Wall = '#';
        public const char Collectable = '.';
        public const char PlayerSpawn = 'P';
        public const char GhostSpawn = 'G';
        public const char SafeArea = 'S';
        public const char Floor = ' ';

        [SerializeField] private List<LegendEntry> _entries = new();

        public bool TryGetPrefab(char symbol, out GameObject prefab)
        {
            foreach (LegendEntry entry in _entries)
            {
                if (entry.Symbol == symbol)
                {
                    prefab = entry.Prefab;
                    return prefab != null;
                }
            }

            prefab = null;
            return false;
        }

        public bool IsWalkable(char symbol)
        {
            foreach (LegendEntry entry in _entries)
            {
                if (entry.Symbol == symbol)
                {
                    return entry.Walkable;
                }
            }

            // Unmapped characters are treated as floor, which is walkable.
            return true;
        }
    }
}
