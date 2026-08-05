using _Project.Code.Core.Managers;
using _Project.Code.Gameplay.Dialogue;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Code.UI
{
    public class UIDialogueScene : MonoBehaviour
    {
        [SerializeField] private GameObject _dialogueDisplayPrefab;
        private VisualElement _root, _background;
        private DialogueScene _currentScene;
        private DialogueText[] _dialogueTexts;

        private int _currentSceneIndex = 0;
        private int _currentLineIndex = 0;

        private void Start()
        {
            Load();
        }

        private void Load()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _background = _root.Q<VisualElement>(UIConstants.backgroundName);
            LoadScene(0);
        }

        private void LoadScene(int sceneIndex)
        {
            _currentSceneIndex = sceneIndex;
            _currentScene = DialogueManager.Instance.GetSceneByIndex(sceneIndex);
            if (_currentScene == null)
            {
                GameSceneManager.Instance.LoadTitleScene();
                return;
            }

            _dialogueTexts = _currentScene.dialogue;
            LoadBackground(_currentScene);

            if (_dialogueTexts == null || _dialogueTexts.Length == 0)
            {
                Debug.LogWarning($"Dialogue scene '{_currentScene.name}' has no dialogue lines, so it is being skipped.");
                LoadFollowingScene();
                return;
            }

            _currentLineIndex = 0;
            LoadNextLine();
        }

        private void LoadBackground(DialogueScene scene)
        {
            Texture2D background = Resources.Load<Texture2D>("Backgrounds/" + scene.background);
            if (background == null)
            {
                Debug.LogWarning($"Dialogue scene '{scene.name}' names background '{scene.background}', but no such texture was found under Resources/Backgrounds. Keeping the current image.");
                return;
            }

            _background.style.backgroundImage = new StyleBackground(background);
        }

        private void LoadFollowingScene()
        {
            int nextSceneIndex = _currentSceneIndex + 1;
            if (nextSceneIndex < DialogueManager.Instance.SceneCount)
            {
                LoadScene(nextSceneIndex);
                return;
            }

            //the story is over, so hand the player back to the title
            GameSceneManager.Instance.LoadTitleScene();
        }

        private void LoadNextLine()
        {
            GameObject dialogueDisplay = Instantiate(_dialogueDisplayPrefab);
            UIDialogueDisplay display = dialogueDisplay.GetComponent<UIDialogueDisplay>();
            display.Load(_dialogueTexts[_currentLineIndex], DialogueManager.Instance.LetterWaitTime);
            _root.Add(dialogueDisplay.GetComponent<UIDocument>().rootVisualElement);
            display.displayComplete.AddListener(() =>
            {
                _currentLineIndex++;
                if (_currentLineIndex < _dialogueTexts.Length)
                    LoadNextLine();
                else
                    LoadFollowingScene();
            });
        }
    }
}
