using System;
using UnityEngine;

namespace _Project.Code.Gameplay.Level
{
    /// <summary>
    /// One symbol-to-prefab mapping in a <see cref="LevelLegend"/>. The symbol is authored as a
    /// single-character string because Unity does not serialize <c>char</c> cleanly in the inspector.
    /// </summary>
    [Serializable]
    public class LegendEntry
    {
        [SerializeField] private string _symbol = "#";
        [SerializeField] private GameObject _prefab;
        [SerializeField] private bool _walkable;

        public GameObject Prefab => _prefab;
        public bool Walkable => _walkable;

        /// <summary>The authored symbol as a character. Defaults to a space if the string is empty.</summary>
        public char Symbol => string.IsNullOrEmpty(_symbol) ? ' ' : _symbol[0];
    }
}
