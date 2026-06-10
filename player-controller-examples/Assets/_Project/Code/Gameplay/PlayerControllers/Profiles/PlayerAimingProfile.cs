using UnityEngine;

namespace _Project.Code.Gameplay.PlayerControllers.Profiles
{
    [CreateAssetMenu(fileName = "PlayerAimingProfile", menuName = "ScriptableObjects/Movement/Player Aiming Profile")]
    public class PlayerAimingProfile : MovementProfile
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

        public float CalculateJumpForce()
        {
            return Mathf.Sqrt(JumpHeight * -2f * Gravity);
        }
    }
}
