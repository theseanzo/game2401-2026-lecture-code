using UnityEngine;

namespace _Project.Code.Gameplay.PlayerControllers.Profiles
{
    [CreateAssetMenu(fileName = "PointClickMovementProfile", menuName = "ScriptableObjects/Movement/Point Click Movement Profile")]
    public class PointClickMovementProfile : MovementProfile
    {
        [field: SerializeField, Header("Point & Click Settings"), Tooltip("Stopping distance from the target position"), Range(0.1f, 2f)]
        public float StoppingDistance { get; private set; } = 0.5f;

        [field: SerializeField, Tooltip("Distance to start slowing down"), Range(1f, 5f)]
        public float SlowdownDistance { get; private set; } = 2f;

        [field: SerializeField, Tooltip("Layers that can be clicked for movement")]
        public LayerMask ClickableLayers { get; private set; } = -1;

        [field: SerializeField, Tooltip("Show click feedback at destination")]
        public bool ShowClickFeedback { get; private set; } = true;

        [field: SerializeField, Tooltip("NavMesh area mask for pathfinding")]
        public int NavMeshAreaMask { get; private set; } = -1;

        [field: SerializeField, Tooltip("Recalculate path if target moves this distance"), Range(0.5f, 5f)]
        public float PathRecalculationDistance { get; private set; } = 1f;
    }
}