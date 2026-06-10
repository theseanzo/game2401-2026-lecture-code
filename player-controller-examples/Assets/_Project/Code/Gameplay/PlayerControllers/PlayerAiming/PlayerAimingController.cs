using UnityEngine;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.PlayerControllers.Base;
using _Project.Code.Gameplay.PlayerControllers.Profiles;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.PlayerAiming
{
    // First-person CharacterController. Look yaws the whole body and pitches a camera pivot, so
    // movement is body-relative: forward follows the aim, left/right strafe.
    [RequireComponent(typeof(CharacterController), typeof(GroundCheck))]
    public class PlayerAimingController : MonoBehaviour
    {
        [field: SerializeField, Header("Movement Settings")]
        public PlayerAimingProfile MovementProfile { get; private set; }

        [Header("Look")]
        // The transform that pitches up and down for the camera. Leave empty to skip pitch and only
        // yaw the body. Usually an eye-height child the camera follows.
        [SerializeField] private Transform _cameraPivot;
        // Degrees of body yaw and pivot pitch per unit of look input.
        [SerializeField] private float _lookSensitivity = 0.15f;
        // How far the pivot can tilt up (positive) and down (negative) before it stops.
        [SerializeField] private float _minPitch = -80f;
        [SerializeField] private float _maxPitch = 80f;

        private CharacterController _controller;
        private GroundCheck _groundCheck;
        private PlayerAnimationController _animation;
        private InputSingleton _input;

        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private bool _sprinting;
        private bool _jumpHeld;
        private bool _jumpCut;
        private float _lastJumpPressedTime = -999f;
        private bool _wasGrounded;

        private Vector3 _planarVelocity;
        private float _verticalVelocity;
        private float _pitch;

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
            _input.OnLook += HandleLook;
            _input.OnJump += HandleJump;
            _input.OnJumpReleased += HandleJumpReleased;
            _input.OnSprint += HandleSprint;
        }

        private void OnDisable()
        {
            if (_input == null)
            {
                return;
            }

            _input.OnMove -= HandleMove;
            _input.OnLook -= HandleLook;
            _input.OnJump -= HandleJump;
            _input.OnJumpReleased -= HandleJumpReleased;
            _input.OnSprint -= HandleSprint;
        }

        private void Start()
        {
            // First-person aiming reads the mouse every frame, so hide and lock the cursor.
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void HandleMove(Vector2 value) => _moveInput = value;

        private void HandleLook(Vector2 value) => _lookInput = value;

        private void HandleJump()
        {
            _lastJumpPressedTime = Time.time;
            _jumpHeld = true;
        }

        private void HandleJumpReleased() => _jumpHeld = false;

        private void HandleSprint(bool held) => _sprinting = held;

        private void Update()
        {
            float dt = Time.deltaTime;
            bool grounded = _groundCheck.IsGrounded;

            ApplyLook();

            Vector3 wishDirection = BodyRelative(_moveInput);
            Move(wishDirection, grounded, dt);
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

        // Look.x yaws the whole body so forward always means "where the player is facing".
        // Look.y pitches the camera pivot, clamped so the view never rolls past straight up or down.
        private void ApplyLook()
        {
            transform.Rotate(Vector3.up, _lookInput.x * _lookSensitivity);

            if (_cameraPivot == null)
            {
                return;
            }

            _pitch = Mathf.Clamp(_pitch - _lookInput.y * _lookSensitivity, _minPitch, _maxPitch);
            _cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        // Build the desired move direction from the body's own axes. Because the body yaws with look,
        // forward/back follows the aim and left/right strafes around it.
        private Vector3 BodyRelative(Vector2 input)
        {
            return transform.right * input.x + transform.forward * input.y;
        }

        private void Move(Vector3 wishDirection, bool grounded, float dt)
        {
            // Walk by default; holding sprint runs at the profile's run speed times its sprint boost.
            float groundSpeed = _sprinting
                ? MovementProfile.RunSpeed * MovementProfile.SprintMultiplier
                : MovementProfile.WalkSpeed;

            float targetSpeed = grounded
                ? groundSpeed
                : MovementProfile.WalkSpeed * MovementProfile.AirControlMultiplier;

            AccelerateTowards(wishDirection, targetSpeed, grounded, dt);
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

            _animation.SetFloat(AnimationParameter.Speed, _planarVelocity.magnitude / MovementProfile.RunSpeed);
            _animation.SetFloat(AnimationParameter.VerticalSpeed, _verticalVelocity);
            _animation.SetBool(AnimationParameter.IsGrounded, grounded);
            _animation.SetFloat(AnimationParameter.StrafeX, _moveInput.x);
            _animation.SetFloat(AnimationParameter.StrafeY, _moveInput.y);
        }
    }
}
