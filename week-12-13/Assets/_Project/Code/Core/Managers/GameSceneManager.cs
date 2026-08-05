using _Project.Code.Core.Patterns;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Code.Core.Managers
{
    public class GameSceneManager : Singleton<GameSceneManager>
    {
        //each name must match an enabled entry in the Build Settings scene list
        [SerializeField] private string _titleSceneName = "TitleScene";
        [SerializeField] private string _dialogueSceneName = "DialogueScene";

        public void LoadTitleScene()
        {
            SceneManager.LoadScene(_titleSceneName);
        }

        public void LoadDialogueScene()
        {
            SceneManager.LoadScene(_dialogueSceneName);
        }
    }
}
