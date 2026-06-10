using UnityEngine;

namespace _Project.Code.Gameplay.PlayerControllers.Profiles
{
    [CreateAssetMenu(fileName = "MovementProfile", menuName = "ScriptableObjects/Movement/Base Movement Profile")]
    public class MovementProfile : ScriptableObject
    {
        [field: SerializeField, Header("Speed")] public float WalkSpeed { get; private set; } = 3f;
        [field: SerializeField] public float RunSpeed { get; private set; } = 6f;
        [field: SerializeField] public float SprintMultiplier { get; private set; } = 1.5f;

        [field: SerializeField, Header("Acceleration")] public float Acceleration { get; private set; } = 10f;
        [field: SerializeField] public float Deceleration { get; private set; } = 15f;
        [field: SerializeField] public float AirAcceleration { get; private set; } = 5f;

        [field: SerializeField, Header("Rotation")] public float RotationSpeed { get; private set; } = 720f;
        [field: SerializeField] public bool RotateTowardsMovement { get; private set; } = true;
        [field: SerializeField] public float RotationSmoothTime { get; private set; } = 0.1f;

        [field: SerializeField, Header("Grounding")] public float GroundCheckDistance { get; private set; } = 0.2f;
        [field: SerializeField] public LayerMask GroundLayers { get; private set; } = -1;
    }
}
