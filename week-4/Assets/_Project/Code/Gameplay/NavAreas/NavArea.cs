using _Project.Code.Core.Interaction;
using _Project.Code.Gameplay.Level;
using UnityEngine;

namespace _Project.Code.Gameplay.NavAreas
{
    [RequireComponent(typeof(Collider))]
    public class NavArea : MonoBehaviour, IInteractable
    {
        [field: SerializeField] public NodeId TargetNode { get; private set; }

        public void Interact()
        {
            LevelManager.Instance.RequestTransition(TargetNode);
        }
    }
}
