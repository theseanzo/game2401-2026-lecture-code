using System.Collections;
using _Project.Code.Gameplay.Dialogue;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace _Project.Code.UI
{
    public class UIDialogueDisplay : MonoBehaviour
    {
        [HideInInspector]
        public UnityEvent displayComplete;
        private VisualElement _root, _characterDisplay, _dialogueContainer;
        private TextElement _characterName;
        private TextElement _dialogueText;

        [SerializeField] private int _flyInTime = 100;
        //must outlast the fly out transition in USS, or the display tears down mid animation
        [SerializeField] private int _flyOutTime = 400;
        private float _letterWaitTime;
        private string _finishedText;
        private Coroutine _typingRoutine;
        private bool _isTyping;
        private bool _advanced;
        private bool _dismissed;

        public void Load(DialogueText dialogueText, float letterWaitTime)
        {
            _letterWaitTime = letterWaitTime;
            _root = GetComponent<UIDocument>().rootVisualElement;
            _dialogueContainer = _root.Q<VisualElement>(UIConstants.dialogueContainer);
            _characterDisplay = _root.Q<VisualElement>(UIConstants.characterDisplayName);
            _characterName = _root.Q<TextElement>(UIConstants.characterNameName);
            _dialogueText = _root.Q<TextElement>(UIConstants.dialogueTextName);
            string charRef = dialogueText.character_ref;
            DialogueCharacter currentCharacter =
                DialogueManager.Instance.GetCharacter(charRef);
            _characterName.text = currentCharacter.name;
            _finishedText = dialogueText.text;
            _dialogueText.text = "";
            //clicks on the arrow button bubble up to here, so the container is the only handler
            _dialogueContainer.RegisterCallback<ClickEvent>(OnContainerClicked);
            Sprite sprite = currentCharacter.emotionDictionary[dialogueText.emotion];
            _characterDisplay.style.backgroundImage = new StyleBackground(sprite);

            _root.schedule.Execute(RemoveFlyOutClass).StartingIn(_flyInTime);

            _typingRoutine = StartCoroutine(DisplayCharacters());
        }

        void OnContainerClicked(ClickEvent clickEvent)
        {
            if (_isTyping)
            {
                StopCoroutine(_typingRoutine);
                _isTyping = false;
                _dialogueText.text = _finishedText;
                return;
            }

            if (_advanced)
                return;

            _advanced = true;
            _dialogueContainer.AddToClassList(UIConstants.flyOutRight);
            displayComplete.Invoke();
            //let the fly out finish before this display tears itself down
            _root.schedule.Execute(Dismiss).StartingIn(_flyOutTime);
        }

        public void Dismiss()
        {
            if (_dismissed)
                return;
            _dismissed = true;
            _root.RemoveFromHierarchy();
            Destroy(gameObject);
        }

        void RemoveFlyOutClass()
        {
            _dialogueContainer.RemoveFromClassList(UIConstants.flyOutLeft);
        }

        IEnumerator DisplayCharacters()
        {
            _isTyping = true;
            string currentString = "";
            for (int i = 0; i < _finishedText.Length; i++)
            {
                currentString += _finishedText[i];
                _dialogueText.text = currentString;
                yield return new WaitForSeconds(_letterWaitTime);
            }
            _isTyping = false;
        }
    }
}
