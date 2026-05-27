using UnityEngine;

namespace _Project.Code.Gameplay.Level
{
    public class LevelNode : MonoBehaviour
    {
        [field: SerializeField] public NodeId NodeId { get; private set; }
    }
}
