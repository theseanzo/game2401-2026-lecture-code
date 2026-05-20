using UnityEngine;

namespace _Project.Code.Gameplay.Level
{
    /// <summary>
    /// A level authored as a block of text. Each line is a row; each character is a tile
    /// resolved through <see cref="LevelLegend"/>. The maze is edited directly in the inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
    public class LevelData : ScriptableObject
    {
        [SerializeField] private string _levelName = "Untitled";

        [SerializeField, TextArea(10, 20)]
        private string _layout = string.Empty;

        [SerializeField] private LevelLegend _legend;

        public string LevelName => _levelName;
        public string Layout => _layout;
        public LevelLegend Legend => _legend;
    }
}
