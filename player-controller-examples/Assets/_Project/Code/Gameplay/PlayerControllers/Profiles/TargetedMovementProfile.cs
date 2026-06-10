using UnityEngine;

namespace _Project.Code.Gameplay.PlayerControllers.Profiles
{
    [CreateAssetMenu(fileName = "TargetedMovementProfile", menuName = "ScriptableObjects/Movement/Targeted Movement Profile")]
    public class TargetedMovementProfile : MovementProfile
    {
        [field: SerializeField, Header("Strafe Settings"), Tooltip("Movement speed when strafing around target")]
        public float StrafeSpeed { get; private set; } = 4f;

        [field: SerializeField, Tooltip("Backwards movement speed when locked on")]
        public float BackpedalSpeed { get; private set; } = 3f;

        [field: SerializeField, Header("Lock-On Settings"), Tooltip("Maximum distance to acquire lock-on target")]
        public float LockOnDistance { get; private set; } = 15f;

        [field: SerializeField, Tooltip("Distance at which lock-on breaks")]
        public float LockOnBreakDistance { get; private set; } = 20f;

        [field: SerializeField, Tooltip("Angle threshold for lock-on acquisition (degrees)"), Range(0f, 180f)]
        public float LockOnAngle { get; private set; } = 45f;

        [field: SerializeField, Tooltip("Speed of rotation when orbiting target")]
        public float OrbitSpeed { get; private set; } = 180f;

        [field: SerializeField, Tooltip("Layers that can be locked onto")]
        public LayerMask TargetLayers { get; private set; } = -1;

        [field: SerializeField, Header("Target Switching"), Tooltip("Can switch targets while locked on?")]
        public bool AllowTargetSwitching { get; private set; } = true;

        [field: SerializeField, Tooltip("Cooldown between target switches (seconds)")]
        public float TargetSwitchCooldown { get; private set; } = 0.2f;

        [field: SerializeField, Header("Dodge Settings"), Tooltip("Dodge force/speed")]
        public float DodgeForce { get; private set; } = 10f;

        [field: SerializeField, Tooltip("Duration of dodge")]
        public float DodgeDuration { get; private set; } = 0.4f;

        [field: SerializeField, Tooltip("Cooldown between dodges (seconds)")]
        public float DodgeCooldown { get; private set; } = 1f;

        [field: SerializeField, Tooltip("Is character invulnerable during dodge?")]
        public bool DodgeInvulnerable { get; private set; } = true;
    }
}
