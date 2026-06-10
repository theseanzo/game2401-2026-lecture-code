using UnityEngine;

namespace _Project.Code.Gameplay.PlayerControllers.Profiles
{
    [CreateAssetMenu(fileName = "TopDownMovementProfile", menuName = "ScriptableObjects/Movement/Top Down Movement Profile")]
    public class TopDownMovementProfile : MovementProfile
    {
        [field: SerializeField, Header("Top Down Settings"), Tooltip("Use world-space movement (true) or camera-relative movement (false)")]
        public bool UseWorldSpaceMovement { get; private set; } = true;

        [field: SerializeField, Tooltip("Allow diagonal movement normalization")]
        public bool NormalizeDiagonalMovement { get; private set; } = true;

        [field: SerializeField, Tooltip("Use twin-stick aiming (right stick controls rotation)")]
        public bool UseTwinStickAiming { get; private set; } = false;

        [field: SerializeField, Tooltip("Rotate to face the mouse cursor on the ground (top-down shooter aim)")]
        public bool UseMouseCursorAiming { get; private set; } = false;

        [field: SerializeField, Tooltip("Rotation speed when aiming (twin-stick or mouse cursor)"), Range(360f, 1080f)]
        public float AimRotationSpeed { get; private set; } = 720f;

        [field: SerializeField, Tooltip("Auto-rotate to face movement direction when not aiming")]
        public bool AutoRotateToMovement { get; private set; } = true;

        [field: SerializeField, Header("Physics"), Tooltip("Gravity force applied to keep character grounded")]
        public float Gravity { get; private set; } = -15f;
    }
}