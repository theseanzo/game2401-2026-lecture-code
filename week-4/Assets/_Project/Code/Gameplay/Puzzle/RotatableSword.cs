using System;
using _Project.Code.Core.Interaction;
using UnityEngine;

namespace _Project.Code.Gameplay.Puzzle
{
    [RequireComponent(typeof(Collider))]
    public class RotatableSword : MonoBehaviour, IInteractable
    {
        [field: SerializeField] public SwordOrientation StartingOrientation { get; private set; } = SwordOrientation.Up;
        [field: SerializeField] public Vector3 RotationAxis { get; private set; } = Vector3.forward;

        public SwordOrientation Orientation { get; private set; }

        public event Action<RotatableSword> OnRotated;

        private void Awake()
        {
            Orientation = StartingOrientation;
            ApplyRotation();
        }

        public void Interact()
        {
            Orientation = NextClockwise(Orientation);
            ApplyRotation();
            OnRotated?.Invoke(this);
        }

        private void ApplyRotation()
        {
            transform.GetChild(0).localRotation = Quaternion.AngleAxis((float)Orientation, RotationAxis);
        }

        private static SwordOrientation NextClockwise(SwordOrientation current)
        {
            return current switch
            {
                SwordOrientation.Up => SwordOrientation.Right,
                SwordOrientation.Right => SwordOrientation.Down,
                SwordOrientation.Down => SwordOrientation.Left,
                SwordOrientation.Left => SwordOrientation.Up,
                _ => SwordOrientation.Up,
            };
        }
    }
}
