using UnityEngine;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.PlayerControllers.Base;
using _Project.Code.Gameplay.PlayerControllers.Profiles;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.SideScroller
{
    // Rigidbody 2.5D platformer: movement is on the world X axis, the body yaws to ±90 to face travel.
    // The feature-rich archetype — double jump, air dash, wall slide and jump, apex hang, fast fall,
    // and variable jump height are each gated by a flag on the JumpingMovementProfile, so one script
    // scales from a basic jumper up.
    [RequireComponent(typeof(Rigidbody), typeof(GroundCheck))]
    public class SideScrollerController : MonoBehaviour
    {
        [field: SerializeField, Header("SideScroller Settings")]
        public JumpingMovementProfile MovementProfile { get; private set; }

        private Rigidbody _rigidbody;
        private GroundCheck _groundCheck;
        private PlayerAnimationController _animation;
        private InputSingleton _input;

        // Latest input snapshot. These are written by the event handlers and read in FixedUpdate.
        private Vector2 _moveInput;
        private bool _sprinting;
        private bool _jumpHeld;
        private float _lastJumpPressedTime = -999f;

        // Per-air-time bookkeeping for the advanced mechanics. All of these reset on landing.
        private bool _jumpCut;          // we already trimmed this jump for variable height
        private int _airJumpsRemaining; // double-jump budget left
        private bool _hasAirDashed;     // dash spent (when AirDashOncePerJump is on)
        private float _lastDashTime = -999f;
        private bool _wasGrounded;

        // The air dash is a short scripted burst. While it runs we ignore normal movement.
        private bool _dashing;
        private float _dashEndTime;
        private float _dashVelocityX;

        // Horizontal speed we ease toward the target each step (separate from the rigidbody so
        // acceleration and deceleration stay smooth even when velocity gets overwritten).
        private float _currentSpeedX;

        // Small threshold so a barely-touched stick does not count as movement.
        private const float InputThreshold = 0.01f;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _groundCheck = GetComponent<GroundCheck>();
            _animation = GetComponent<PlayerAnimationController>();

            // We manage gravity by hand for tunable jump feel, and we never want the body to topple.
            _rigidbody.useGravity = false;
            _rigidbody.freezeRotation = true;
        }

        private void OnEnable()
        {
            _input = InputSingleton.Instance;
            _input.OnMove += HandleMove;
            _input.OnJump += HandleJump;
            _input.OnJumpReleased += HandleJumpReleased;
            _input.OnSprint += HandleSprint;
            _input.OnDodge += HandleDodge;
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
            _input.OnDodge -= HandleDodge;
        }

        private void HandleMove(Vector2 value) => _moveInput = value;

        // Record the press time so a jump can fire from the buffer window if we are not quite
        // grounded yet. _jumpHeld stays true until release for variable jump height.
        private void HandleJump()
        {
            _lastJumpPressedTime = Time.time;
            _jumpHeld = true;

            TryWallJump();
            TryAirJump();
        }

        private void HandleJumpReleased() => _jumpHeld = false;

        private void HandleSprint(bool held) => _sprinting = held;

        // Dodge is the air-dash trigger. It only does anything while airborne and off cooldown.
        private void HandleDodge() => TryAirDash();

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            bool grounded = _groundCheck.IsGrounded;

            // Landing edge: reset the air budget and fire the land animation once.
            if (grounded && !_wasGrounded)
            {
                _airJumpsRemaining = MovementProfile.MaxAirJumps;
                _hasAirDashed = false;
                _jumpCut = false;
                _animation?.TriggerAnimation(AnimationTrigger.Land);
            }
            _wasGrounded = grounded;

            // A scripted dash overrides everything else for its short duration.
            if (_dashing)
            {
                UpdateDash(grounded);
                UpdateAnimation(grounded);
                return;
            }

            // Wall slide takes priority over a normal fall when enabled and we are hugging a wall.
            Vector3 wallNormal = Vector3.zero;
            bool onWall = !grounded && MovementProfile.EnableWallMechanics && CheckWall(out wallNormal);

            if (onWall)
            {
                UpdateWallSlide(wallNormal, dt);
            }
            else
            {
                UpdateHorizontal(grounded, dt);
                UpdateVertical(grounded, dt);
            }

            TryBufferedGroundJump(grounded);

            UpdateAnimation(grounded);
        }

        // --- Horizontal movement -------------------------------------------------------------

        private void UpdateHorizontal(bool grounded, float dt)
        {
            float inputX = _moveInput.x;
            bool hasInput = Mathf.Abs(inputX) > InputThreshold;

            if (grounded)
            {
                // On the ground, ease toward walk or run speed (sprint picks run) with the
                // acceleration rate when speeding up and the deceleration rate when stopping.
                float targetSpeed = hasInput
                    ? (_sprinting ? MovementProfile.RunSpeed : MovementProfile.WalkSpeed)
                    : 0f;

                float rate = targetSpeed > 0f
                    ? MovementProfile.Acceleration
                    : MovementProfile.Deceleration;

                _currentSpeedX = Mathf.MoveTowards(_currentSpeedX, targetSpeed, rate * dt);

                float velocityX = hasInput ? Mathf.Sign(inputX) * _currentSpeedX : 0f;
                SetVelocityX(velocityX);
            }
            else if (hasInput)
            {
                // In the air, preserve momentum: keep the faster of current speed and the air-capped
                // target, so a running leap does not suddenly slow down mid-flight.
                float currentSpeed = Mathf.Abs(_rigidbody.linearVelocity.x);
                float maxAirSpeed = _sprinting ? MovementProfile.RunSpeed : MovementProfile.WalkSpeed;
                float targetAirSpeed = Mathf.Max(currentSpeed, maxAirSpeed * MovementProfile.AirControlMultiplier);

                _currentSpeedX = targetAirSpeed;
                SetVelocityX(Mathf.Sign(inputX) * targetAirSpeed);
            }

            if (hasInput)
            {
                FaceDirection(inputX, dt);
            }
        }

        private void SetVelocityX(float x)
        {
            Vector3 velocity = _rigidbody.linearVelocity;
            velocity.x = x;
            velocity.z = 0f;
            _rigidbody.linearVelocity = velocity;
        }

        // Side-scroller bodies look down +X or -X, so snap toward 90 or -90 degrees of yaw.
        private void FaceDirection(float inputX, float dt)
        {
            Quaternion targetRotation = Quaternion.Euler(0f, inputX > 0f ? 90f : -90f, 0f);
            _rigidbody.MoveRotation(Quaternion.RotateTowards(
                transform.rotation, targetRotation, MovementProfile.RotationSpeed * dt));
        }

        // --- Vertical movement (gravity, variable height, apex, fast fall) -------------------

        private void UpdateVertical(bool grounded, float dt)
        {
            if (grounded)
            {
                // Pin to the ground with a small downward bias so the ground check stays reliable.
                if (_rigidbody.linearVelocity.y < 0f)
                {
                    SetVelocityY(-2f);
                }
                return;
            }

            float velocityY = _rigidbody.linearVelocity.y;

            // Variable jump height: releasing the button while still rising trims the climb so a tap
            // is a short hop and a hold is the full jump. Only cut once, and only above the floor.
            if (MovementProfile.VariableJumpHeight && !_jumpHeld && !_jumpCut && velocityY > 0f)
            {
                float minRise = MovementProfile.CalculateJumpForce() * MovementProfile.MinJumpMultiplier;
                if (velocityY > minRise)
                {
                    velocityY *= 0.4f;
                }
                _jumpCut = true;
            }

            // Pick a gravity multiplier for this frame.
            float gravityMultiplier;
            if (MovementProfile.UseApexHangTime && Mathf.Abs(velocityY) < MovementProfile.ApexThreshold)
            {
                // Near the peak, slow gravity for a floaty hang.
                gravityMultiplier = MovementProfile.ApexGravityMultiplier;
            }
            else if (velocityY < 0f)
            {
                // Falling: heavier gravity so the arc does not feel floaty.
                gravityMultiplier = MovementProfile.FallGravityMultiplier;

                // Fast fall: holding down while falling multiplies the descent further.
                if (MovementProfile.UseFastFall && _moveInput.y < -0.5f)
                {
                    gravityMultiplier *= MovementProfile.FastFallMultiplier;
                }
            }
            else
            {
                gravityMultiplier = 1f;
            }

            // Gravity on the profile is negative, so adding it pulls the body down over time.
            velocityY += MovementProfile.Gravity * gravityMultiplier * dt;

            // Clamp the descent so terminal velocity stays controllable.
            if (velocityY < -MovementProfile.MaxFallSpeed)
            {
                velocityY = -MovementProfile.MaxFallSpeed;
            }

            SetVelocityY(velocityY);
        }

        private void SetVelocityY(float y)
        {
            Vector3 velocity = _rigidbody.linearVelocity;
            velocity.y = y;
            _rigidbody.linearVelocity = velocity;
        }

        // --- Jumping ------------------------------------------------------------------------

        // Fires a ground jump from a buffered press: a press recorded just before landing, or while
        // inside the coyote-time window after leaving a ledge, still launches once we can.
        private void TryBufferedGroundJump(bool grounded)
        {
            bool jumpBuffered = Time.time - _lastJumpPressedTime <= MovementProfile.JumpBufferTime;
            bool canCoyoteJump = grounded || _groundCheck.TimeSinceGrounded <= MovementProfile.CoyoteTime;

            if (jumpBuffered && canCoyoteJump)
            {
                DoJump(MovementProfile.CalculateJumpForce());
                _lastJumpPressedTime = -999f; // consume the buffered press
            }
        }

        // A double jump consumes one air-jump charge. Triggered immediately on the press event so it
        // feels responsive, and only while airborne and outside the coyote window (that case is a
        // ground jump handled by the buffer above).
        private void TryAirJump()
        {
            if (!MovementProfile.EnableDoubleJump)
            {
                return;
            }

            bool airborne = !_groundCheck.IsGrounded && _groundCheck.TimeSinceGrounded > MovementProfile.CoyoteTime;
            if (airborne && _airJumpsRemaining > 0)
            {
                _airJumpsRemaining--;
                DoJump(MovementProfile.CalculateAirJumpForce());
                _lastJumpPressedTime = -999f; // do not also fire a buffered ground jump
            }
        }

        private void DoJump(float force)
        {
            SetVelocityY(force);
            _jumpCut = false;
            _animation?.TriggerAnimation(AnimationTrigger.Jump);
        }

        // --- Air dash -----------------------------------------------------------------------

        private void TryAirDash()
        {
            bool canDash = MovementProfile.EnableAirDash
                && !_groundCheck.IsGrounded
                && (!MovementProfile.AirDashOncePerJump || !_hasAirDashed)
                && Time.time >= _lastDashTime + MovementProfile.AirDashCooldown;

            if (!canDash)
            {
                return;
            }

            // Dash toward the stick, or toward the current facing when the stick is centred.
            float direction;
            if (Mathf.Abs(_moveInput.x) > 0.1f)
            {
                direction = Mathf.Sign(_moveInput.x);
            }
            else
            {
                direction = Mathf.Approximately(transform.rotation.eulerAngles.y, 90f) ? 1f : -1f;
            }

            // Cover the configured distance over the configured duration: speed = distance / time.
            _dashVelocityX = direction * (MovementProfile.AirDashDistance / MovementProfile.AirDashDuration);
            _dashEndTime = Time.time + MovementProfile.AirDashDuration;
            _dashing = true;

            _hasAirDashed = true;
            _lastDashTime = Time.time;

            FaceDirection(direction, 1f);
        }

        // While dashing, drive a flat horizontal burst with zero vertical velocity. When the timer
        // runs out, hand control back to the normal ground or fall path.
        private void UpdateDash(bool grounded)
        {
            if (Time.time >= _dashEndTime || grounded)
            {
                _dashing = false;
                return;
            }

            _rigidbody.linearVelocity = new Vector3(_dashVelocityX, 0f, 0f);
        }

        // --- Wall slide and wall jump -------------------------------------------------------

        // Raycast toward the current facing for a wall. Yaw of 90 means facing +X.
        private bool CheckWall(out Vector3 wallNormal)
        {
            wallNormal = Vector3.zero;

            float direction = Mathf.Approximately(transform.rotation.eulerAngles.y, 90f) ? 1f : -1f;
            Vector3 rayDirection = new Vector3(direction, 0f, 0f);

            if (Physics.Raycast(transform.position, rayDirection, out RaycastHit hit,
                    MovementProfile.WallCheckDistance, MovementProfile.WallLayers))
            {
                wallNormal = hit.normal;
                return true;
            }

            return false;
        }

        private void UpdateWallSlide(Vector3 wallNormal, float dt)
        {
            // Pushing the stick away from the wall releases the slide and lets the player drop off.
            if (Mathf.Abs(_moveInput.x) > InputThreshold
                && Mathf.Sign(_moveInput.x) == Mathf.Sign(wallNormal.x))
            {
                return; // next frame is no longer on the wall, so normal falling resumes
            }

            float velocityY = _rigidbody.linearVelocity.y;

            if (velocityY < -MovementProfile.WallSlideSpeed)
            {
                // Clamp the descent to the slide speed once we are sliding fast enough.
                velocityY = -MovementProfile.WallSlideSpeed;
            }
            else
            {
                // Otherwise apply a gentle gravity so the slide eases in.
                velocityY += MovementProfile.Gravity * 0.3f * dt;
            }

            _rigidbody.linearVelocity = new Vector3(0f, velocityY, 0f);
        }

        // Wall jump pushes off the wall normal and upward at once. Fired from the press event so it
        // responds instantly; only valid while actually sliding on a wall.
        private void TryWallJump()
        {
            if (!MovementProfile.EnableWallMechanics || _groundCheck.IsGrounded)
            {
                return;
            }

            if (!CheckWall(out Vector3 wallNormal))
            {
                return;
            }

            float horizontal = Mathf.Sign(wallNormal.x) * MovementProfile.WallJumpForce;
            _rigidbody.linearVelocity = new Vector3(horizontal, MovementProfile.WallJumpUpForce, 0f);

            _jumpCut = false;
            _lastJumpPressedTime = -999f; // do not also fire a buffered ground jump
            FaceDirection(wallNormal.x, 1f);
            _animation?.TriggerAnimation(AnimationTrigger.Jump);
        }

        // --- Animation ----------------------------------------------------------------------

        // Feed movement state to the Animator. PlayerAnimationController ignores any parameter the
        // Animator Controller does not define, so this is safe even with a minimal rig.
        private void UpdateAnimation(bool grounded)
        {
            if (_animation == null)
            {
                return;
            }

            _animation.SetFloat(AnimationParameter.Speed,
                Mathf.Abs(_rigidbody.linearVelocity.x) / MovementProfile.RunSpeed);
            _animation.SetFloat(AnimationParameter.VerticalSpeed, _rigidbody.linearVelocity.y);
            _animation.SetBool(AnimationParameter.IsGrounded, grounded);
        }
    }
}
