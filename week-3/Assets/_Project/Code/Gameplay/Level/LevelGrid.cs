using System.Collections.Generic;
using UnityEngine;

namespace _Project.Code.Gameplay.Level
{
    /// <summary>
    /// A parsed level: a 2D grid of <see cref="Tile"/>s plus the spawn points found in the layout.
    /// Pure data — no scene objects. Built from a layout string via <see cref="FromLayout"/>.
    /// Grid coordinates are (column, row) with row 0 at the top, matching the inspector text.
    /// </summary>
    public class LevelGrid
    {
        private readonly Tile[,] _tiles;

        public int Width { get; }
        public int Height { get; }
        public Vector2Int PlayerSpawn { get; }
        public IReadOnlyList<Vector2Int> GhostSpawns { get; }

        private LevelGrid(Tile[,] tiles, int width, int height, Vector2Int playerSpawn, List<Vector2Int> ghostSpawns)
        {
            _tiles = tiles;
            Width = width;
            Height = height;
            PlayerSpawn = playerSpawn;
            GhostSpawns = ghostSpawns;
        }

        /// <summary>Maps a grid cell to world space on the XZ plane. Row 0 is at z = 0; rows descend in -z.</summary>
        public static Vector3 GridToWorld(Vector2Int cell) => new Vector3(cell.x, 0f, -cell.y);

        public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        public char GetTile(int x, int y) => InBounds(x, y) ? _tiles[x, y].Symbol : LevelLegend.Floor;

        public bool IsWalkable(int x, int y) => InBounds(x, y) && _tiles[x, y].Walkable;

        /// <summary>
        /// Parses a layout string into a grid using <paramref name="legend"/> for walkability.
        /// Outer blank lines are trimmed; short rows are padded with floor so the grid is rectangular.
        /// </summary>
        public static LevelGrid FromLayout(string layout, LevelLegend legend)
        {
            List<string> rows = SplitRows(layout);

            int height = rows.Count;
            int width = 0;
            foreach (string row in rows)
            {
                if (row.Length > width)
                {
                    width = row.Length;
                }
            }

            Tile[,] tiles = new Tile[Mathf.Max(width, 0), Mathf.Max(height, 0)];
            Vector2Int playerSpawn = new Vector2Int(-1, -1);
            bool playerFound = false;
            List<Vector2Int> ghostSpawns = new();

            for (int y = 0; y < height; y++)
            {
                string row = rows[y];
                for (int x = 0; x < width; x++)
                {
                    char symbol = x < row.Length ? row[x] : LevelLegend.Floor;
                    bool walkable = legend == null || legend.IsWalkable(symbol);
                    Vector2Int cell = new Vector2Int(x, y);
                    tiles[x, y] = new Tile(symbol, cell, walkable);

                    switch (symbol)
                    {
                        case LevelLegend.PlayerSpawn:
                            if (playerFound)
                            {
                                Debug.LogWarning($"[LevelGrid] Multiple player spawns; using the one at {cell}.");
                            }
                            playerSpawn = cell;
                            playerFound = true;
                            break;
                        case LevelLegend.GhostSpawn:
                            ghostSpawns.Add(cell);
                            break;
                    }
                }
            }

            if (!playerFound)
            {
                Debug.LogWarning("[LevelGrid] No player spawn ('P') found in layout.");
            }

            return new LevelGrid(tiles, width, height, playerSpawn, ghostSpawns);
        }

        private static List<string> SplitRows(string layout)
        {
            List<string> rows = new();
            if (string.IsNullOrEmpty(layout))
            {
                return rows;
            }

            foreach (string raw in layout.Split('\n'))
            {
                rows.Add(raw.TrimEnd('\r'));
            }

            // Trim fully-blank lines from the top and bottom only; interior blank lines and
            // leading spaces are meaningful (floor tiles).
            while (rows.Count > 0 && rows[0].Trim().Length == 0)
            {
                rows.RemoveAt(0);
            }
            while (rows.Count > 0 && rows[rows.Count - 1].Trim().Length == 0)
            {
                rows.RemoveAt(rows.Count - 1);
            }

            return rows;
        }
    }
}
