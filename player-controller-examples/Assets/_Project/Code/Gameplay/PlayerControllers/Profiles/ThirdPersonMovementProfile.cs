using UnityEngine;

namespace _Project.Code.Gameplay.PlayerControllers.Profiles
{
    [CreateAssetMenu(fileName = "ThirdPersonMovementProfile", menuName = "ScriptableObjects/Movement/Third Person Movement Profile")]
    public class ThirdPersonMovementProfile : MovementProfile
    {
        [field: SerializeField, Header("Jump Settings"), Tooltip("Height the character can jump")]
        public float JumpHeight { get; private set; } = 2f;

        [field: SerializeField, Tooltip("Gravity force applied when falling")]
        public float Gravity { get; private set; } = -15f;

        [field: SerializeField, Tooltip("Additional gravity multiplier when falling (for better jump feel)")]
        public float FallGravityMultiplier { get; private set; } = 2f;

        [field: SerializeField, Tooltip("Time after leaving ground where jump is still allowed"), Range(0f, 0.5f)]
        public float CoyoteTime { get; private set; } = 0.15f;

        [field: SerializeField, Tooltip("Time before landing where jump input is buffered"), Range(0f, 0.5f)]
        public float JumpBufferTime { get; private set; } = 0.2f;

        [field: SerializeField, Tooltip("Can the player hold jump for variable height?")]
        public bool VariableJumpHeight { get; private set; } = true;

        [field: SerializeField, Tooltip("Minimum jump force when tap jumping"), Range(0f, 1f)]
        public float MinJumpMultiplier { get; private set; } = 0.5f;

        [field: SerializeField, Tooltip("Movement control multiplier while airborne"), Range(0f, 1f)]
        public float AirControlMultiplier { get; private set; } = 0.7f;

        [field: SerializeField, Header("Third Person Specific"), Tooltip("Speed multiplier when moving backwards"), Range(0.5f, 1f)]
        public float BackwardSpeedMultiplier { get; private set; } = 1f;

        [field: SerializeField, Header("Lock-On Settings"), Tooltip("Maximum distance to search for targets")]
        public float TargetSearchRadius { get; private set; } = 15f;

        [field: SerializeField, Tooltip("Maximum angle from forward direction to detect targets"), Range(0f, 180f)]
        public float TargetAngle { get; private set; } = 120f;

        [field: SerializeField, Tooltip("Layers that contain targetable enemies")]
        public LayerMask TargetLayers { get; private set; } = -1;

        [field: SerializeField, Tooltip("Walk speed when strafing while locked on")]
        public float StrafeWalkSpeed { get; private set; } = 2.5f;

        [field: SerializeField, Tooltip("Sprint speed when strafing while locked on")]
        public float StrafeSprintSpeed { get; private set; } = 6f;

        [field: SerializeField, Tooltip("Air movement speed when jumping while locked on")]
        public float StrafeAirSpeed { get; private set; } = 2f;

        [field: SerializeField, Tooltip("How quickly to rotate towards target when locked on")]
        public float LockOnRotationSpeed { get; private set; } = 720f;

        public float CalculateJumpForce()
        {
            return Mathf.Sqrt(JumpHeight * -2f * Gravity);
        }
    }
}