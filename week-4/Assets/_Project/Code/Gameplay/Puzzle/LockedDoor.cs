using _Project.Code.Core.Interaction;
using _Project.Code.Gameplay.Items;
using UnityEngine;

namespace _Project.Code.Gameplay.Puzzle
{
    [RequireComponent(typeof(Collider))]
    public class LockedDoor : Puzzle, IInteractable
    {
        [field: SerializeField] public Item RequiredItem { get; private set; }

        public void Interact()
        {
            if (IsSolved || !Inventory.Instance.Has(RequiredItem))
            {
                return;
            }

            transform.GetChild(0).gameObject.SetActive(false);
            MarkSolved();
        }
    }
}
