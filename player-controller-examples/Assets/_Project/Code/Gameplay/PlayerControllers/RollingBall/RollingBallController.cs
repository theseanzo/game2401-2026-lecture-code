using UnityEngine;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.PlayerControllers.Profiles;

namespace _Project.Code.Gameplay.PlayerControllers.RollingBall
{
    // Camera-relative force on a physics sphere: adds force and rolling torque and never sets velocity
    // directly, so momentum is the Rigidbody's. The arcade rolling ball; the authentic stage-tilt
    // version is in Code/Gameplay/Stage.
    [RequireComponent(typeof(Rigidbody))]
    public class RollingBallController : MonoBehaviour
    {
        [field: SerializeField, Header("Rolling Ball Settings")]
        public RollingBallMovementProfile MovementProfile { get; private set; }

        [Header("References")]
        // The camera that movement is measured against. Leave empty to use the main camera.
        [SerializeField] private Transform _cameraTransform;

        private Rigidbody _rigidbody;
        private InputSingleton _input;

        // The latest stick reading. X is left/right, Y is forward/back, each in the range -1 to 1.
        private Vector2 _moveInput;

        // Set true for a single FixedUpdate when the jump button is pressed, then consumed.
        private bool _jumpRequested;

        // Recomputed every FixedUpdate so the rest of the frame can share the same answer.
        private bool _grounded;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();

            // Fall back to the main camera if nobody wired one up in the Inspector.
            if (_cameraTransform == null && Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
        }

        // Subscribe to input only while this component is active. Mirroring the subscribe and
        // unsubscribe in OnEnable/OnDisable prevents leaking handlers when the object is disabled.
        private void OnEnable()
        {
            _input = InputSingleton.Instance;
            _input.OnMove += HandleMove;
            _input.OnJump += HandleJump;
        }

        private void OnDisable()
        {
            // The singleton may already be gone during shutdown, so guard before unsubscribing.
            if (_input == null)
            {
                return;
            }

            _input.OnMove -= HandleMove;
            _input.OnJump -= HandleJump;
        }

        // Move fires on both press and release, so letting go of the stick hands us a zero vector.
        private void HandleMove(Vector2 value) => _moveInput = value;

        // Remember that jump was pressed. FixedUpdate decides whether it is actually allowed.
        private void HandleJump() => _jumpRequested = true;

        private void FixedUpdate()
        {
            _grounded = CheckGrounded();

            // Match the Rigidbody's damping to the situation: grippy on the ground, slippery in air.
            _rigidbody.linearDamping = _grounded ? MovementProfile.GroundDrag : MovementProfile.AirDrag;

            Vector3 wishDirection = CameraRelative(_moveInput);

            ApplyRollForce(wishDirection);
            ApplyRollTorque(wishDirection);
            ClampHorizontalSpeed();
            ApplyJump();
            ApplyExtraGravity();
        }

        // Flatten the camera's forward and right onto the ground plane, then blend them by the stick.
        // Zeroing the Y component means tilting the camera up or down never changes which way the ball
        // goes. The result is the direction the player wants to roll, in world space.
        private Vector3 CameraRelative(Vector2 input)
        {
            if (_cameraTransform == null)
            {
                // With no camera we fall back to world axes so the ball still responds to input.
                return new Vector3(input.x, 0f, input.y);
            }

            Vector3 forward = _cameraTransform.forward;
            Vector3 right = _cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return forward * input.y + right * input.x;
        }

        // Push the ball in the wished direction. While airborne we scale the push down so steering in
        // mid-air feels weaker than rolling on solid ground. Acceleration mode ignores the ball's mass
        // so the feel stays the same if a designer changes how heavy the ball is.
        private void ApplyRollForce(Vector3 wishDirection)
        {
            if (wishDirection.sqrMagnitude < 0.0001f)
            {
                return;
            }

            float control = _grounded ? 1f : MovementProfile.AirControlMultiplier;
            _rigidbody.AddForce(wishDirection * (MovementProfile.RollForce * control), ForceMode.Acceleration);
        }

        // Spin the ball so it visibly rolls instead of sliding. The spin axis is perpendicular to the
        // travel direction: crossing the up vector with the wished direction gives the axis the ball
        // would naturally turn about as it rolls forward. Only matters on the ground.
        private void ApplyRollTorque(Vector3 wishDirection)
        {
            if (MovementProfile.RollTorque <= 0f || !_grounded || wishDirection.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Vector3 spinAxis = Vector3.Cross(Vector3.up, wishDirection.normalized);
            _rigidbody.AddTorque(spinAxis * MovementProfile.RollTorque, ForceMode.Acceleration);
        }

        // Cap how fast the ball can travel across the ground while leaving its up/down speed alone, so
        // gravity and jumps still work. We only touch the horizontal part of the velocity.
        private void ClampHorizontalSpeed()
        {
            Vector3 velocity = _rigidbody.linearVelocity;
            Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);

            if (horizontal.magnitude > MovementProfile.MaxSpeed)
            {
                horizontal = horizontal.normalized * MovementProfile.MaxSpeed;
                _rigidbody.linearVelocity = new Vector3(horizontal.x, velocity.y, horizontal.z);
            }
        }

        // Jump only when the press happened and the ball is actually touching the ground. We zero the
        // current upward speed first so a jump always reaches the same height regardless of any small
        // bounce already in progress, then add a clean upward impulse.
        private void ApplyJump()
        {
            if (!_jumpRequested)
            {
                return;
            }

            _jumpRequested = false;

            if (!_grounded)
            {
                return;
            }

            Vector3 velocity = _rigidbody.linearVelocity;
            velocity.y = 0f;
            _rigidbody.linearVelocity = velocity;
            _rigidbody.AddForce(Vector3.up * MovementProfile.JumpForce, ForceMode.Impulse);
        }

        // Add a little extra downward pull while airborne. Unity's built-in gravity alone can feel
        // floaty, so this sharpens the arc and makes the ball land sooner.
        private void ApplyExtraGravity()
        {
            if (_grounded)
            {
                return;
            }

            _rigidbody.AddForce(Vector3.down * MovementProfile.ExtraGravity, ForceMode.Acceleration);
        }

        // Cast a small sphere straight down from the ball's centre. If it hits something on the ground
        // layers within reach, the ball counts as grounded. A sphere cast hugs slopes and edges better
        // than a single thin raycast.
        private bool CheckGrounded()
        {
            return Physics.SphereCast(
                transform.position,
                MovementProfile.GroundCheckRadius,
                Vector3.down,
                out _,
                MovementProfile.GroundCheckDistance,
                MovementProfile.GroundLayers,
                QueryTriggerInteraction.Ignore);
        }

        // Draw the ground check in the Scene view while the object is selected, so the radius and reach
        // can be tuned by eye. Green when grounded, red when airborne.
        private void OnDrawGizmosSelected()
        {
            if (MovementProfile == null)
            {
                return;
            }

            Gizmos.color = _grounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(
                transform.position + Vector3.down * MovementProfile.GroundCheckDistance,
                MovementProfile.GroundCheckRadius);
        }
    }
}
