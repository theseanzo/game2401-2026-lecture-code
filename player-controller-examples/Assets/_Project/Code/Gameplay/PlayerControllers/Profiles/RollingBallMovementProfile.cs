using UnityEngine;

namespace _Project.Code.Gameplay.PlayerControllers.Profiles
{
    [CreateAssetMenu(fileName = "RollingBallMovementProfile", menuName = "ScriptableObjects/Movement/Rolling Ball Movement Profile")]
    public class RollingBallMovementProfile : ScriptableObject
    {
        [field: SerializeField, Header("Rolling"), Tooltip("How hard the stick pushes the ball along the ground. Bigger numbers accelerate faster.")]
        public float RollForce { get; private set; } = 40f;

        [field: SerializeField, Tooltip("The fastest the ball is allowed to roll across the ground, in metres per second.")]
        public float MaxSpeed { get; private set; } = 12f;

        [field: SerializeField, Tooltip("Spin applied so the ball visibly rolls in the direction it moves. Set to 0 for a pure slide.")]
        public float RollTorque { get; private set; } = 10f;

        [field: SerializeField, Header("Air Control"), Tooltip("How much steering power you keep while airborne. 1 is full control, 0 is none."), Range(0f, 1f)]
        public float AirControlMultiplier { get; private set; } = 0.4f;

        [field: SerializeField, Header("Jump"), Tooltip("Upward impulse applied when the jump button is pressed while grounded.")]
        public float JumpForce { get; private set; } = 7f;

        [field: SerializeField, Tooltip("Extra downward acceleration on top of gravity so the ball does not feel floaty at the top of a jump.")]
        public float ExtraGravity { get; private set; } = 12f;

        [field: SerializeField, Header("Ground Check"), Tooltip("Radius of the sphere cast straight down used to decide whether the ball is touching the ground.")]
        public float GroundCheckRadius { get; private set; } = 0.45f;

        [field: SerializeField, Tooltip("How far below the ball centre the ground check reaches. Roughly the ball radius plus a little slack.")]
        public float GroundCheckDistance { get; private set; } = 0.6f;

        [field: SerializeField, Tooltip("Which physics layers count as the ground.")]
        public LayerMask GroundLayers { get; private set; } = -1;

        [field: SerializeField, Header("Grip"), Tooltip("Linear damping applied while rolling on the ground. Higher values make the ball coast to a stop sooner.")]
        public float GroundDrag { get; private set; } = 1.5f;

        [field: SerializeField, Tooltip("Linear damping applied while in the air. Usually lower than ground drag so jumps carry momentum.")]
        public float AirDrag { get; private set; } = 0.1f;
    }
}
