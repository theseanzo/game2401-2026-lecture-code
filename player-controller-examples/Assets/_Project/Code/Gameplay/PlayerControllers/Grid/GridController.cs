using System.Collections;
using UnityEngine;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.PlayerControllers.Profiles;

namespace _Project.Code.Gameplay.PlayerControllers.Grid
{
    // Grid movement: snaps the stick to one camera-relative cardinal direction and steps exactly one
    // cell over a fixed duration. Moves the transform directly via coroutine — no Rigidbody or
    // CharacterController.
    public class GridController : MonoBehaviour
    {
        [field: SerializeField, Header("Grid Settings")]
        public GridMovementProfile MovementProfile { get; private set; }

        private InputSingleton _input;

        // The most recent stick value from the input event. Read once per step, never continuously.
        private Vector2 _moveInput;

        // True while a step coroutine is running. While stepping, new input is ignored; the next
        // input is only read once the current step finishes.
        private bool _isStepping;

        private void OnEnable()
        {
            _input = InputSingleton.Instance;
            _input.OnMove += HandleMove;
        }

        private void OnDisable()
        {
            if (_input == null)
            {
                return;
            }

            _input.OnMove -= HandleMove;
        }

        private void Start()
        {
            // Snap to the nearest cell on the grid so steps line up cleanly from the first move.
            transform.position = SnapToGrid(transform.position);
        }

        private void HandleMove(Vector2 value) => _moveInput = value;

        private void Update()
        {
            // One step at a time. Ignore all input until the current step coroutine completes.
            if (_isStepping)
            {
                return;
            }

            Vector3 direction = GetGridDirection();
            if (direction.sqrMagnitude < 0.01f)
            {
                return;
            }

            Vector3 target = transform.position + direction * MovementProfile.GridSize;

            // A wall (or anything on the collision layers) in the way cancels the step entirely.
            if (!CanMoveTo(direction))
            {
                return;
            }

            StartCoroutine(Step(target, direction));
        }

        // Round a world position to the nearest grid cell on the X and Z axes. Height is left alone.
        private Vector3 SnapToGrid(Vector3 position)
        {
            float gridSize = MovementProfile.GridSize;
            return new Vector3(
                Mathf.Round(position.x / gridSize) * gridSize,
                position.y,
                Mathf.Round(position.z / gridSize) * gridSize);
        }

        // Convert the analog stick into one of the four cardinal directions. The stick is first
        // interpreted relative to where the camera faces, then the stronger of the two axes wins so
        // the result is always a clean north/south/east/west step (never a diagonal).
        private Vector3 GetGridDirection()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                return Vector3.zero;
            }

            Vector3 forward = camera.transform.forward;
            Vector3 right = camera.transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 direction = right * _moveInput.x + forward * _moveInput.y;

            // Pick the dominant axis and collapse it to a single cardinal unit vector.
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
            {
                return direction.x > 0f ? Vector3.right : Vector3.left;
            }

            if (Mathf.Abs(direction.z) > 0.01f)
            {
                return direction.z > 0f ? Vector3.forward : Vector3.back;
            }

            return Vector3.zero;
        }

        // Cast a short ray in the step direction to see if something blocks the next cell. When
        // collision checks are disabled the step is always allowed.
        private bool CanMoveTo(Vector3 direction)
        {
            if (!MovementProfile.CheckCollisions)
            {
                return true;
            }

            Vector3 origin = transform.position + Vector3.up * 0.5f;
            return !Physics.Raycast(origin, direction, MovementProfile.GridSize, MovementProfile.CollisionLayers);
        }

        // Move from the current cell to the target cell over StepDuration. Optionally turn to face
        // the direction of travel first, and optionally arc upward through a hop. The coroutine holds
        // the stepping flag for its whole lifetime so no second step can start until it returns.
        private IEnumerator Step(Vector3 target, Vector3 direction)
        {
            _isStepping = true;

            // Turn to face the way we are about to move, unless strafing keeps the current facing.
            if (!MovementProfile.AllowStrafe)
            {
                yield return StartCoroutine(Rotate(direction));
            }

            Vector3 start = transform.position;
            float duration = Mathf.Max(MovementProfile.StepDuration, 0.0001f);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                Vector3 position = Vector3.Lerp(start, target, t);

                if (MovementProfile.UseHopAnimation)
                {
                    // A sine curve peaks at the midpoint of the step, giving a smooth up-and-down hop.
                    position.y += Mathf.Sin(t * Mathf.PI) * MovementProfile.HopHeight;
                }

                transform.position = position;
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Land exactly on the target cell to avoid drift from accumulated lerp error.
            transform.position = target;
            _isStepping = false;
        }

        // Smoothly rotate to look along the step direction over TurnDuration.
        private IEnumerator Rotate(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.01f)
            {
                yield break;
            }

            Quaternion start = transform.rotation;
            Quaternion target = Quaternion.LookRotation(direction);
            float duration = Mathf.Max(MovementProfile.TurnDuration, 0.0001f);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                transform.rotation = Quaternion.Slerp(start, target, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.rotation = target;
        }
    }
}
