using _Project.Code.Core.Interaction;
using _Project.Code.Gameplay.Items;
using UnityEngine;

namespace _Project.Code.Gameplay.Puzzle
{
    [RequireComponent(typeof(Collider))]
    public class KeyPickup : Puzzle, IInteractable
    {
        [field: SerializeField] public Item Item { get; private set; }

        public void Interact()
        {
            if (IsSolved)
            {
                return;
            }

            Inventory.Instance.Add(Item);
            transform.GetChild(0).gameObject.SetActive(false);
            MarkSolved();
        }
    }
}
