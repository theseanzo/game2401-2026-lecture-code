using UnityEngine;

namespace _Project.Code.Gameplay.PlayerControllers.Profiles
{
    [CreateAssetMenu(fileName = "TankMovementProfile", menuName = "ScriptableObjects/Movement/Tank Movement Profile")]
    public class TankMovementProfile : MovementProfile
    {
        [field: SerializeField, Header("Tank Settings"), Tooltip("Speed multiplier when moving backward"), Range(0.3f, 1f)]
        public float BackwardSpeedMultiplier { get; private set; } = 0.5f;

        [field: SerializeField, Tooltip("Degrees per second for turning in place"), Range(45f, 180f)]
        public float TurnSpeed { get; private set; } = 90f;

        [field: SerializeField, Tooltip("Can the tank turn while moving?")]
        public bool CanTurnWhileMoving { get; private set; } = false;

        [field: SerializeField, Tooltip("Speed reduction when turning while moving"), Range(0.5f, 1f)]
        public float TurnMovementSpeedMultiplier { get; private set; } = 0.7f;

        [field: SerializeField, Header("Physics"), Tooltip("Gravity force applied to keep tank grounded")]
        public float Gravity { get; private set; } = -15f;
    }
}