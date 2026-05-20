using UnityEngine;

namespace _Project.Code.Gameplay.Level
{
    /// <summary>One cell of a parsed level: the authored symbol, its grid position, and walkability.</summary>
    public readonly struct Tile
    {
        public char Symbol { get; }
        public Vector2Int GridPosition { get; }
        public bool Walkable { get; }

        public Tile(char symbol, Vector2Int gridPosition, bool walkable)
        {
            Symbol = symbol;
            GridPosition = gridPosition;
            Walkable = walkable;
        }
    }
}
