using _Project.Code.Core.Managers;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Code.UI
{
    public class TitleScreen : MonoBehaviour
    {
        private VisualElement _root, _titleBox;
        private Button _newButton, _loadButton, _exitButton;
        [SerializeField] private AudioSource _backgroundMusic;
        [SerializeField] private int _flyInTime = 200;
        [SerializeField] private int _buttonPulseTime = 5000;

        private void Start()
        {
            if (_backgroundMusic != null)
                _backgroundMusic.Play();

            _root = GetComponent<UIDocument>().rootVisualElement;
            _titleBox = _root.Q<VisualElement>(UIConstants.titleBox);
            _newButton = _root.Q<Button>(UIConstants.newButton);
            _loadButton = _root.Q<Button>(UIConstants.loadButton);
            _exitButton = _root.Q<Button>(UIConstants.exitButton);
            _root.schedule.Execute(TextFlyIn).StartingIn(_flyInTime);
            _newButton.clicked += () => GameSceneManager.Instance.LoadDialogueScene();
            _loadButton.clicked += () =>
            {
                //TODO implement loading once save data exists
            };
            _exitButton.clicked += () => EditorApplication.isPlaying = false;
            _newButton.RegisterCallback<TransitionEndEvent>(evt =>
            {
                _newButton.ToggleInClassList(UIConstants.fadeBackground);
                _newButton.ToggleInClassList(UIConstants.brightenBackground);
            });
            _root.schedule.Execute(() =>
            {
                _newButton.ToggleInClassList(UIConstants.fadeBackground);
                _newButton.ToggleInClassList(UIConstants.brightenBackground);
            }).StartingIn(_buttonPulseTime);
        }

        private void TextFlyIn()
        {
            _titleBox.RemoveFromClassList(UIConstants.flyIn);
            _newButton.RemoveFromClassList(UIConstants.flyIn);
            _loadButton.RemoveFromClassList(UIConstants.flyIn);
            _exitButton.RemoveFromClassList(UIConstants.flyIn);
        }
    }
}
