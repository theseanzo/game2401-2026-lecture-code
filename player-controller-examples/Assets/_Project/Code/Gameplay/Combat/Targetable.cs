using UnityEngine;

namespace _Project.Code.Gameplay.Combat
{
    public class Targetable : MonoBehaviour, ITargetable
    {
        [SerializeField] private bool _isTargetable = true;
        [SerializeField] private float _priority = 1f;

        public bool IsTargetable => _isTargetable && gameObject.activeInHierarchy;
        public float Priority => _priority;

        public void SetTargetable(bool targetable)
        {
            _isTargetable = targetable;
        }

        public void SetPriority(float priority)
        {
            _priority = priority;
        }
    }
}