using _Project.Code.Core.Patterns;
using _Project.Code.Gameplay.Dialogue;
using UnityEngine;

namespace _Project.Code.Core.Managers
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private TextAsset _jsonFile;

        public DialogueData GameData { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            GameData = JsonUtility.FromJson<DialogueData>(_jsonFile.text);
            DialogueManager.Instance.Initialize(GameData);
        }
    }
}
