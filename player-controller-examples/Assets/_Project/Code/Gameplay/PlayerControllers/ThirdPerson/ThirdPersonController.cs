using UnityEngine;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.PlayerControllers.Base;
using _Project.Code.Gameplay.PlayerControllers.Profiles;
using _Project.Code.Gameplay.PlayerControllers.ThirdPerson.Utilities;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.ThirdPerson
{
    // Third-person CharacterController. Moves relative to the camera and turns to face travel; while
    // a lock-on target is held it strafes and keeps facing that target instead.
    [RequireComponent(typeof(CharacterController), typeof(GroundCheck))]
    public class ThirdPersonController : MonoBehaviour
    {
        [field: SerializeField, Header("Third Person Settings")]
        public ThirdPersonMovementProfile MovementProfile { get; private set; }

        [Header("References")]
        // The camera movement is measured against. Leave empty to use the main camera.
        [SerializeField] private Transform _cameraTransform;

        private CharacterController _controller;
        private GroundCheck _groundCheck;
        private PlayerAnimationController _animation;
        private InputSingleton _input;

        private Vector2 _moveInput;
        private bool _sprinting;
        private bool _jumpHeld;
        private bool _jumpCut;
        private float _lastJumpPressedTime = -999f;
        private bool _wasGrounded;

        private Vector3 _planarVelocity;
        private float _verticalVelocity;
        private float _yawVelocity;

        private Transform _lockOnTarget;

        // The current lock-on target, or null. A camera can read this to frame the target.
        public Transform LockOnTarget => _lockOnTarget;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _groundCheck = GetComponent<GroundCheck>();
            _animation = GetComponent<PlayerAnimationController>();

            if (_cameraTransform == null && Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
        }

        private void OnEnable()
        {
            _input = InputSingleton.Instance;
            _input.OnMove += HandleMove;
            _input.OnJump += HandleJump;
            _input.OnJumpReleased += HandleJumpReleased;
            _input.OnSprint += HandleSprint;
            _input.OnLockOn += HandleLockOn;
        }

        private void OnDisable()
        {
            if (_input == null)
            {
                return;
            }

            _input.OnMove -= HandleMove;
            _input.OnJump -= HandleJump;
            _input.OnJumpReleased -= HandleJumpReleased;
            _input.OnSprint -= HandleSprint;
            _input.OnLockOn -= HandleLockOn;
        }

        private void Start()
        {
            // Cinemachine reads the mouse to orbit, so hide and lock the cursor.
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void HandleMove(Vector2 value) => _moveInput = value;

        private void HandleJump()
        {
            _lastJumpPressedTime = Time.time;
            _jumpHeld = true;
        }

        private void HandleJumpReleased() => _jumpHeld = false;

        private void HandleSprint(bool held) => _sprinting = held;

        // Hold to lock onto the nearest target in front; release to drop the lock.
        private void HandleLockOn(bool pressed)
        {
            _lockOnTarget = pressed
                ? TargetFinder.FindNearestTarget(transform, MovementProfile.TargetSearchRadius, MovementProfile.TargetAngle)
                : null;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            bool grounded = _groundCheck.IsGrounded;
            Vector3 wishDirection = CameraRelative(_moveInput);

            if (_lockOnTarget != null)
            {
                MoveLockedOn(wishDirection, grounded, dt);
            }
            else
            {
                MoveFree(wishDirection, grounded, dt);
            }

            ApplyJumpAndGravity(grounded, dt);

            Vector3 velocity = _planarVelocity + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * dt);

            // Trigger the landing animation on the frame we touch back down.
            if (grounded && !_wasGrounded)
            {
                _animation?.TriggerAnimation(AnimationTrigger.Land);
            }
            _wasGrounded = grounded;

            UpdateAnimation(grounded);
        }

        // Flatten the camera's forward/right onto the ground plane so looking up or down never
        // changes which way the stick sends the player.
        private Vector3 CameraRelative(Vector2 input)
        {
            Vector3 forward = _cameraTransform.forward;
            Vector3 right = _cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return forward * input.y + right * input.x;
        }

        private void MoveFree(Vector3 wishDirection, bool grounded, float dt)
        {
            // Stick tilt picks a speed between walk and run; holding sprint overrides to top speed.
            float inputMagnitude = Mathf.Clamp01(_moveInput.magnitude);
            float groundSpeed = Mathf.Lerp(MovementProfile.WalkSpeed, MovementProfile.RunSpeed, inputMagnitude);
            if (_sprinting)
            {
                groundSpeed = MovementProfile.RunSpeed * MovementProfile.SprintMultiplier;
            }

            float targetSpeed = grounded
                ? groundSpeed
                : MovementProfile.WalkSpeed * MovementProfile.AirControlMultiplier;

            AccelerateTowards(wishDirection, targetSpeed, grounded, dt);

            if (MovementProfile.RotateTowardsMovement)
            {
                FaceMovementDirection(_planarVelocity, dt);
            }
        }

        private void MoveLockedOn(Vector3 wishDirection, bool grounded, float dt)
        {
            FaceTarget(dt);

            float speed = grounded
                ? (_sprinting ? MovementProfile.StrafeSprintSpeed : MovementProfile.StrafeWalkSpeed)
                : MovementProfile.StrafeAirSpeed;

            // Backpedalling (pulling the stick away from the target) can be slower.
            if (_moveInput.y < 0f)
            {
                speed *= MovementProfile.BackwardSpeedMultiplier;
            }

            AccelerateTowards(wishDirection, speed, grounded, dt);
        }

        // Ease the horizontal velocity toward the desired direction and speed. Speeding up,
        // slowing down, and being airborne each get their own rate.
        private void AccelerateTowards(Vector3 wishDirection, float targetSpeed, bool grounded, float dt)
        {
            Vector3 targetVelocity = wishDirection.sqrMagnitude > 0.01f
                ? wishDirection.normalized * targetSpeed
                : Vector3.zero;

            float rate;
            if (!grounded)
            {
                rate = MovementProfile.AirAcceleration;
            }
            else
            {
                rate = targetVelocity.sqrMagnitude > _planarVelocity.sqrMagnitude
                    ? MovementProfile.Acceleration
                    : MovementProfile.Deceleration;
            }

            _planarVelocity = Vector3.Lerp(_planarVelocity, targetVelocity, rate * dt);
        }

        // Turn the body to face the way it is moving, smoothed. Skip when nearly still so the player
        // holds its last facing instead of snapping back to a default.
        private void FaceMovementDirection(Vector3 direction, float dt)
        {
            if (direction.sqrMagnitude < 0.01f)
            {
                return;
            }

            float targetYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float yaw = Mathf.SmoothDampAngle(
                transform.eulerAngles.y, targetYaw, ref _yawVelocity,
                MovementProfile.RotationSmoothTime, MovementProfile.RotationSpeed);

            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        // Turn to face the locked-on target on the horizontal plane.
        private void FaceTarget(float dt)
        {
            Vector3 toTarget = _lockOnTarget.position - transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude < 0.001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(toTarget);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRotation, MovementProfile.LockOnRotationSpeed * dt);
        }

        private void ApplyJumpAndGravity(bool grounded, float dt)
        {
            bool jumpBuffered = Time.time - _lastJumpPressedTime <= MovementProfile.JumpBufferTime;
            bool canJump = grounded || _groundCheck.TimeSinceGrounded <= MovementProfile.CoyoteTime;

            // A small downward push keeps the controller pinned to the ground between frames.
            if (grounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }
            if (grounded)
            {
                _jumpCut = false;
            }

            if (jumpBuffered && canJump)
            {
                _verticalVelocity = MovementProfile.CalculateJumpForce();
                _lastJumpPressedTime = -999f;   // consume the buffered press
                _jumpCut = false;
                _animation?.TriggerAnimation(AnimationTrigger.Jump);
                return;
            }

            // Variable height: if the button is released while still rising, cap the climb so a tap
            // gives a short hop and a hold gives the full jump.
            if (MovementProfile.VariableJumpHeight && !_jumpHeld && !_jumpCut && _verticalVelocity > 0f)
            {
                float minRise = MovementProfile.CalculateJumpForce() * MovementProfile.MinJumpMultiplier;
                if (_verticalVelocity > minRise)
                {
                    _verticalVelocity = minRise;
                }
                _jumpCut = true;
            }

            if (!grounded)
            {
                // Heavier gravity on the way down so the arc does not feel floaty.
                float gravity = MovementProfile.Gravity * (_verticalVelocity < 0f ? MovementProfile.FallGravityMultiplier : 1f);
                _verticalVelocity += gravity * dt;
            }
        }

        // Feed movement state to the Animator. PlayerAnimationController ignores any parameter the
        // Animator Controller does not define.
        private void UpdateAnimation(bool grounded)
        {
            if (_animation == null)
            {
                return;
            }

            bool lockedOn = _lockOnTarget != null;

            _animation.SetFloat(AnimationParameter.Speed, _planarVelocity.magnitude / MovementProfile.RunSpeed);
            _animation.SetFloat(AnimationParameter.VerticalSpeed, _verticalVelocity);
            _animation.SetBool(AnimationParameter.IsGrounded, grounded);
            _animation.SetBool(AnimationParameter.IsLockOn, lockedOn);
            _animation.SetFloat(AnimationParameter.StrafeX, _moveInput.x);
            _animation.SetFloat(AnimationParameter.StrafeY, _moveInput.y);
            _animation.SetLayerWeight(1, lockedOn ? 1f : 0f);
        }
    }
}
