using System.Collections.Generic;
using System.Linq;
using _Project.Code.Core.Patterns;
using UnityEngine;

namespace _Project.Code.Gameplay.Dialogue
{
    public class DialogueManager : Singleton<DialogueManager>
    {
        private Dictionary<string, DialogueCharacter> _characters = new Dictionary<string, DialogueCharacter>();
        private Dictionary<string, DialogueScene> _scenes = new Dictionary<string, DialogueScene>();
        private List<DialogueScene> _orderedScenes = new List<DialogueScene>();

        [SerializeField] private float _letterWaitTime = 0.04f;

        public int SceneCount => _orderedScenes.Count;

        public float LetterWaitTime => _letterWaitTime;

        public void Initialize(DialogueData data)
        {
            //returning to the title and starting again runs this a second time, so start from empty
            _characters.Clear();
            _scenes.Clear();
            _orderedScenes.Clear();

            foreach (DialogueScene dialogueScene in data.scenes)
            {
                _scenes.Add(dialogueScene.name, dialogueScene);
            }
            //OrderBy is stable, so scenes sharing an order value stay in the order the file lists them
            _orderedScenes.AddRange(data.scenes.OrderBy(dialogueScene => dialogueScene.order));

            foreach (DialogueCharacter dialogueCharacter in data.characters)
            {
                _characters.Add(dialogueCharacter.reference, dialogueCharacter);
                string path = "Characters/" + dialogueCharacter.reference;
                Sprite[] sprites = Resources.LoadAll<Sprite>(path);

                Dictionary<string, Sprite> spritesByName = new Dictionary<string, Sprite>();
                foreach (Sprite sprite in sprites)
                {
                    spritesByName[sprite.name] = sprite;
                }

                foreach (KeyValuePair<string, string> emotion in dialogueCharacter.emotions.AsDictionary())
                {
                    if (string.IsNullOrEmpty(emotion.Value))
                        continue;

                    if (spritesByName.TryGetValue(emotion.Value, out Sprite emotionSprite))
                    {
                        dialogueCharacter.emotionDictionary.Add(emotion.Key, emotionSprite);
                    }
                    else
                    {
                        Debug.LogWarning($"Character '{dialogueCharacter.reference}' names sprite '{emotion.Value}' for emotion '{emotion.Key}', but no such sprite was found under Resources/{path}.");
                    }
                }
            }
        }

        public DialogueCharacter GetCharacter(string characterRef)
        {
            return _characters[characterRef];
        }

        public DialogueScene GetScene(string sceneRef)
        {
            return _scenes[sceneRef];
        }

        public DialogueScene GetSceneByIndex(int index)
        {
            if (index < 0 || index >= _orderedScenes.Count)
                return null;

            return _orderedScenes[index];
        }

        public Sprite GetCharacterSprite(string characterRef, string emotion)
        {
            Dictionary<string, Sprite> emotionDictionary = GetCharacter(characterRef).emotionDictionary;
            if (emotionDictionary.TryGetValue(emotion, out Sprite sprite))
                return sprite;

            Debug.LogWarning($"Character '{characterRef}' has no sprite for emotion '{emotion}'. Falling back to neutral.");
            return emotionDictionary.TryGetValue("neutral", out Sprite neutralSprite) ? neutralSprite : null;
        }
    }
}
