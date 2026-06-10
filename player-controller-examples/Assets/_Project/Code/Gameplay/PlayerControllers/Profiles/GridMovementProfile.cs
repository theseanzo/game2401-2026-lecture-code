using UnityEngine;
using DG.Tweening;

namespace _Project.Code.Gameplay.PlayerControllers.Profiles
{
    [CreateAssetMenu(fileName = "GridMovementProfile", menuName = "ScriptableObjects/Movement/Grid Movement Profile")]
    public class GridMovementProfile : MovementProfile
    {
        [field: SerializeField, Header("Grid Settings"), Tooltip("Size of each grid cell")]
        public float GridSize { get; private set; } = 1f;

        [field: SerializeField, Tooltip("Duration of movement between grid cells")]
        public float StepDuration { get; private set; } = 0.3f;

        [field: SerializeField, Tooltip("Duration of 90-degree turn")]
        public float TurnDuration { get; private set; } = 0.2f;

        [field: SerializeField, Tooltip("Easing curve for movement")]
        public Ease MoveEase { get; private set; } = Ease.InOutQuad;

        [field: SerializeField, Tooltip("Easing curve for rotation")]
        public Ease RotateEase { get; private set; } = Ease.InOutQuad;

        [field: SerializeField, Header("Movement Rules"), Tooltip("Allow diagonal movement?")]
        public bool AllowDiagonal { get; private set; } = false;

        [field: SerializeField, Tooltip("Allow strafing (move without turning)?")]
        public bool AllowStrafe { get; private set; } = false;

        [field: SerializeField, Tooltip("Allow backwards movement?")]
        public bool AllowBackwards { get; private set; } = true;

        [field: SerializeField, Header("Input Queueing"), Tooltip("Queue inputs during movement?")]
        public bool QueueInputs { get; private set; } = true;

        [field: SerializeField, Tooltip("Maximum number of queued inputs"), Range(1, 5)]
        public int MaxQueuedInputs { get; private set; } = 1;

        [field: SerializeField, Header("Collision"), Tooltip("Check for collisions before moving?")]
        public bool CheckCollisions { get; private set; } = true;

        [field: SerializeField, Tooltip("Layers that block movement")]
        public LayerMask CollisionLayers { get; private set; } = -1;

        [field: SerializeField, Tooltip("Raycast distance for collision check")]
        public float CollisionCheckDistance { get; private set; } = 1.1f;

        [field: SerializeField, Header("Animation"), Tooltip("Slight hop during movement for animation")]
        public bool UseHopAnimation { get; private set; } = false;

        [field: SerializeField, Tooltip("Height of hop during movement")]
        public float HopHeight { get; private set; } = 0.1f;
    }
}
