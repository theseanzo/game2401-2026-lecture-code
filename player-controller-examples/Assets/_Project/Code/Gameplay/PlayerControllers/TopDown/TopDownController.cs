using UnityEngine;
using UnityEngine.InputSystem;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.PlayerControllers.Base;
using _Project.Code.Gameplay.PlayerControllers.Profiles;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.TopDown
{
    // Top-down CharacterController on the ground plane. The body faces the mouse cursor (shooter), the
    // aim stick (twin-stick), or its travel direction. Movement is world-space or camera-relative by
    // profile flag.
    [RequireComponent(typeof(CharacterController), typeof(GroundCheck))]
    public class TopDownController : MonoBehaviour
    {
        [field: SerializeField, Header("Top Down Settings")]
        public TopDownMovementProfile MovementProfile { get; private set; }

        private CharacterController _controller;
        private GroundCheck _groundCheck;
        private PlayerAnimationController _animation;
        private InputSingleton _input;

        private Vector2 _moveInput;
        private Vector2 _aimInput;

        // The eased ground speed and the running vertical (gravity) speed.
        private float _currentSpeed;
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
            _input.OnLook += HandleLook;
        }

        private void OnDisable()
        {
            if (_input == null)
            {
                return;
            }

            _input.OnMove -= HandleMove;
            _input.OnLook -= HandleLook;
        }

        // Left stick drives movement; right stick aims for twin-stick rotation.
        private void HandleMove(Vector2 value) => _moveInput = value;

        private void HandleLook(Vector2 value) => _aimInput = value;

        private void Update()
        {
            float dt = Time.deltaTime;
            bool grounded = _groundCheck.IsGrounded;

            Vector3 moveDirection = GetMovementDirection();

            if (grounded)
            {
                MoveOnGround(moveDirection, dt);
                HandleRotation(moveDirection, dt);
            }
            else
            {
                // Airborne: no ground control, the body just falls. Speed reads as zero.
                _currentSpeed = 0f;
            }

            ApplyGravity(grounded, dt);
            UpdateAnimation();
        }

        // Map the left-stick input onto the ground plane. In world space the stick maps directly to
        // (x, 0, y); otherwise it is measured against the main camera's facing so up on the stick goes
        // away from the camera no matter how the camera is angled.
        private Vector3 GetMovementDirection()
        {
            if (MovementProfile.UseWorldSpaceMovement || Camera.main == null)
            {
                return new Vector3(_moveInput.x, 0f, _moveInput.y);
            }

            Vector3 forward = Camera.main.transform.forward;
            Vector3 right = Camera.main.transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return forward * _moveInput.y + right * _moveInput.x;
        }

        // Ease the ground speed toward the walk speed while the stick is pushed, then slide the
        // controller along the chosen direction.
        private void MoveOnGround(Vector3 moveDirection, float dt)
        {
            if (moveDirection.magnitude <= 0f)
            {
                _currentSpeed = 0f;
                return;
            }

            // Keep diagonal pushes from travelling faster than a straight push.
            if (MovementProfile.NormalizeDiagonalMovement && moveDirection.magnitude > 1f)
            {
                moveDirection.Normalize();
            }

            _currentSpeed = Mathf.Lerp(_currentSpeed, MovementProfile.WalkSpeed, MovementProfile.Acceleration * dt);
            _controller.Move(moveDirection.normalized * _currentSpeed * dt);
        }

        // Mouse-cursor aiming faces the body toward the point under the cursor; twin-stick aiming faces
        // the right stick; otherwise it auto-rotates toward travel. Aim modes turn at AimRotationSpeed,
        // auto-rotate at RotationSpeed.
        private void HandleRotation(Vector3 moveDirection, float dt)
        {
            if (MovementProfile.UseMouseCursorAiming)
            {
                Vector3 cursorDirection = GetCursorDirection();
                if (cursorDirection.sqrMagnitude > 0f)
                {
                    RotateTowards(cursorDirection, MovementProfile.AimRotationSpeed, dt);
                }
            }
            else if (MovementProfile.UseTwinStickAiming)
            {
                if (_aimInput.magnitude > 0f)
                {
                    Vector3 aimDirection = new Vector3(_aimInput.x, 0f, _aimInput.y);
                    RotateTowards(aimDirection, MovementProfile.AimRotationSpeed, dt);
                }
            }
            else if (MovementProfile.AutoRotateToMovement)
            {
                if (moveDirection.magnitude > 0f)
                {
                    RotateTowards(moveDirection, MovementProfile.RotationSpeed, dt);
                }
            }
        }

        // Cast a ray from the main camera through the mouse cursor onto a flat ground plane at the
        // player's feet, then return the flattened body-to-cursor direction. World space, so no ground
        // collider is needed. Returns zero if there is no main camera, no mouse, or the cursor points
        // at the horizon (ray parallel to the plane).
        private Vector3 GetCursorDirection()
        {
            Camera cam = Camera.main;
            if (cam == null || Mouse.current == null)
            {
                return Vector3.zero;
            }

            Vector2 screenPosition = Mouse.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(screenPosition);
            Plane ground = new Plane(Vector3.up, transform.position);

            if (!ground.Raycast(ray, out float distance))
            {
                return Vector3.zero;
            }

            Vector3 direction = ray.GetPoint(distance) - transform.position;
            direction.y = 0f;
            return direction;
        }

        private void RotateTowards(Vector3 direction, float rotationSpeed, float dt)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRotation, rotationSpeed * dt);
        }

        // Pull the controller toward the ground every frame. A small downward push while grounded keeps
        // the capsule pinned between frames; in the air gravity accumulates so the player falls.
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

            _controller.Move(Vector3.up * _verticalVelocity * dt);
        }

        // Feed the ground speed to the Animator. PlayerAnimationController ignores any parameter the
        // Animator Controller does not define, so this is safe even with no animator wired up.
        private void UpdateAnimation()
        {
            if (_animation == null)
            {
                return;
            }

            _animation.SetFloat(AnimationParameter.Speed, _currentSpeed / MovementProfile.WalkSpeed);
        }
    }
}
