using UnityEngine;

namespace _Project.Code.Gameplay.PlayerControllers.Profiles
{
    [CreateAssetMenu(fileName = "JumpingMovementProfile", menuName = "ScriptableObjects/Movement/Jumping Movement Profile")]
    public class JumpingMovementProfile : MovementProfile
    {
        [field: SerializeField, Header("Jump Settings"), Tooltip("Height the character can jump")]
        public float JumpHeight { get; private set; } = 2f;

        [field: SerializeField, Tooltip("Gravity force applied when falling")]
        public float Gravity { get; private set; } = -15f;

        [field: SerializeField, Tooltip("Additional gravity multiplier when falling (for better jump feel)")]
        public float FallGravityMultiplier { get; private set; } = 2f;

        [field: SerializeField, Tooltip("Maximum falling speed"), Range(10f, 100f)]
        public float MaxFallSpeed { get; private set; } = 30f;

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

        [field: SerializeField, Header("Advanced Jump Mechanics"), Tooltip("Slows gravity at apex for floatier feel")]
        public bool UseApexHangTime { get; private set; } = true;

        [field: SerializeField, Tooltip("Vertical speed threshold for apex hang"), Range(0f, 5f)]
        public float ApexThreshold { get; private set; } = 2f;

        [field: SerializeField, Tooltip("Gravity multiplier at apex"), Range(0f, 1f)]
        public float ApexGravityMultiplier { get; private set; } = 0.3f;

        [field: SerializeField, Tooltip("Enable fast fall by holding down")]
        public bool UseFastFall { get; private set; } = true;

        [field: SerializeField, Tooltip("Gravity multiplier when fast falling"), Range(1f, 5f)]
        public float FastFallMultiplier { get; private set; } = 3f;

        [field: SerializeField, Header("Double Jump"), Tooltip("Enable double jump")]
        public bool EnableDoubleJump { get; private set; } = false;

        [field: SerializeField, Tooltip("Maximum number of air jumps"), Range(1, 5)]
        public int MaxAirJumps { get; private set; } = 1;

        [field: SerializeField, Tooltip("Height multiplier for air jumps"), Range(0.5f, 1.5f)]
        public float AirJumpHeightMultiplier { get; private set; } = 0.9f;

        [field: SerializeField, Header("Air Dash"), Tooltip("Enable air dash")]
        public bool EnableAirDash { get; private set; } = false;

        [field: SerializeField, Tooltip("Distance of air dash"), Range(0.5f, 5f)]
        public float AirDashDistance { get; private set; } = 2f;

        [field: SerializeField, Tooltip("Duration of air dash in seconds"), Range(0.05f, 0.5f)]
        public float AirDashDuration { get; private set; } = 0.15f;

        [field: SerializeField, Tooltip("Cooldown between dashes in seconds"), Range(0f, 5f)]
        public float AirDashCooldown { get; private set; } = 1f;

        [field: SerializeField, Tooltip("Can only dash once per air time")]
        public bool AirDashOncePerJump { get; private set; } = true;

        [field: SerializeField, Header("Wall Mechanics"), Tooltip("Enable wall slide and wall jump")]
        public bool EnableWallMechanics { get; private set; } = false;

        [field: SerializeField, Tooltip("Layers that count as walls")]
        public LayerMask WallLayers { get; private set; } = -1;

        [field: SerializeField, Tooltip("Distance to check for walls"), Range(0.1f, 2f)]
        public float WallCheckDistance { get; private set; } = 0.5f;

        [field: SerializeField, Tooltip("Falling speed while sliding on wall"), Range(1f, 10f)]
        public float WallSlideSpeed { get; private set; } = 2f;

        [field: SerializeField, Tooltip("Horizontal force of wall jump"), Range(1f, 20f)]
        public float WallJumpForce { get; private set; } = 10f;

        [field: SerializeField, Tooltip("Vertical force of wall jump"), Range(1f, 20f)]
        public float WallJumpUpForce { get; private set; } = 12f;

        [field: SerializeField, Tooltip("Time after wall jump where input is reduced"), Range(0f, 0.5f)]
        public float WallJumpInputDelay { get; private set; } = 0.2f;

        public float CalculateJumpForce()
        {
            return Mathf.Sqrt(JumpHeight * -2f * Gravity);
        }

        public float CalculateAirJumpForce()
        {
            return CalculateJumpForce() * AirJumpHeightMultiplier;
        }
    }
}
