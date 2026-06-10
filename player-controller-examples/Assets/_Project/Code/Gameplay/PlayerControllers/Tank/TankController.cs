using UnityEngine;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.PlayerControllers.Base;
using _Project.Code.Gameplay.PlayerControllers.Profiles;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.Tank
{
    // Tank-control CharacterController. The stick's Y drives forward/back along the body's own facing;
    // X turns the body in place. Never camera-relative — it only travels the way it points.
    [RequireComponent(typeof(CharacterController), typeof(GroundCheck))]
    public class TankController : MonoBehaviour
    {
        [field: SerializeField, Header("Tank Settings")]
        public TankMovementProfile MovementProfile { get; private set; }

        private CharacterController _controller;
        private GroundCheck _groundCheck;
        private PlayerAnimationController _animation;
        private InputSingleton _input;

        // The latest stick reading. y is forward/back, x is turn left/right.
        private Vector2 _moveInput;

        // The eased forward/back speed, always a positive magnitude.
        private float _currentSpeed;

        // Falling speed, carried between frames so gravity can build up.
        private float _verticalVelocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _groundCheck = GetComponent<GroundCheck>();
            _animation = GetComponent<PlayerAnimationController>();
        }

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

        // Move fires on both press and release, so letting go of the stick sends a zero vector.
        private void HandleMove(Vector2 value) => _moveInput = value;

        private void Update()
        {
            float dt = Time.deltaTime;
            bool grounded = _groundCheck.IsGrounded;

            bool movingForward = _moveInput.y > 0.1f;
            bool movingBackward = _moveInput.y < -0.1f;
            bool turning = Mathf.Abs(_moveInput.x) > 0.1f;
            bool moving = movingForward || movingBackward;

            TurnInPlace(turning, moving, dt);
            float forwardSpeed = DriveForwardBack(movingForward, movingBackward, turning, dt);
            ApplyGravity(grounded, dt);

            Vector3 planar = transform.forward * forwardSpeed;
            Vector3 velocity = planar + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * dt);

            UpdateAnimation();
        }

        // Rotate the body around its up axis. When the profile forbids turning while moving, a tank
        // that is driving forward or back ignores turn input entirely and only steers when stopped.
        private void TurnInPlace(bool turning, bool moving, float dt)
        {
            if (!turning)
            {
                return;
            }

            if (moving && !MovementProfile.CanTurnWhileMoving)
            {
                return;
            }

            float yaw = _moveInput.x * MovementProfile.TurnSpeed * dt;
            transform.Rotate(Vector3.up, yaw);
        }

        // Ease the signed forward/back speed toward its target and return the distance to travel this
        // frame. Turning while moving (when allowed) trims the target so steering costs a little speed.
        private float DriveForwardBack(bool movingForward, bool movingBackward, bool turning, float dt)
        {
            float targetSpeed;
            float direction;

            if (movingForward)
            {
                targetSpeed = MovementProfile.WalkSpeed;
                direction = 1f;
            }
            else if (movingBackward)
            {
                targetSpeed = MovementProfile.WalkSpeed * MovementProfile.BackwardSpeedMultiplier;
                direction = -1f;
            }
            else
            {
                targetSpeed = 0f;
                direction = 0f;
            }

            // A tank that is both moving and steering loses a little speed to the turn.
            if (turning && (movingForward || movingBackward) && MovementProfile.CanTurnWhileMoving)
            {
                targetSpeed *= MovementProfile.TurnMovementSpeedMultiplier;
            }

            _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, MovementProfile.Acceleration * dt);

            return _currentSpeed * direction;
        }

        // Pull the tank down with the profile's gravity. While grounded a small constant push keeps
        // the controller settled on the surface between frames.
        private void ApplyGravity(bool grounded, float dt)
        {
            if (grounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }
            else
            {
                _verticalVelocity += MovementProfile.Gravity * dt;
            }
        }

        // Feed the eased speed to the Animator as a normalized Speed value. PlayerAnimationController
        // is null-safe and ignores any parameter the Animator Controller does not define.
        private void UpdateAnimation()
        {
            _animation?.SetFloat(AnimationParameter.Speed, _currentSpeed / MovementProfile.WalkSpeed);
        }
    }
}
