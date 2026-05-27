using UnityEngine;

namespace _Project.Code.Gameplay.Items
{
    [CreateAssetMenu(fileName = "Item", menuName = "Game/Item")]
    public class Item : ScriptableObject
    {
        [field: SerializeField] public string DisplayName { get; private set; }
    }
}
