using UnityEngine;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.PlayerControllers.Profiles;

namespace _Project.Code.Gameplay.PlayerControllers.Vehicle
{
    // Arcade car on a Rigidbody: throttle drives toward a top speed, steering only bites while moving,
    // grip cancels sideways slide so the car follows its nose, and the handbrake loosens grip to drift.
    // Stick Y is throttle/brake, X is steer, jump or dodge is the handbrake.
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleController : MonoBehaviour
    {
        [field: SerializeField, Header("Vehicle Settings")]
        public VehicleMovementProfile MovementProfile { get; private set; }

        private Rigidbody _rigidbody;
        private InputSingleton _input;

        // Raw input cached from the events and read in FixedUpdate.
        private Vector2 _moveInput;
        private bool _handbrakeHeld;
        private bool _handbrakeTapped;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            _input = InputSingleton.Instance;
            _input.OnMove += HandleMove;
            _input.OnJump += HandleHandbrakePressed;
            _input.OnJumpReleased += HandleHandbrakeReleased;
            _input.OnDodge += HandleHandbrakeTapped;
        }

        private void OnDisable()
        {
            if (_input == null)
            {
                return;
            }

            _input.OnMove -= HandleMove;
            _input.OnJump -= HandleHandbrakePressed;
            _input.OnJumpReleased -= HandleHandbrakeReleased;
            _input.OnDodge -= HandleHandbrakeTapped;
        }

        private void HandleMove(Vector2 value) => _moveInput = value;

        // Jump is a press-and-hold handbrake: pressing engages it, releasing lets go.
        private void HandleHandbrakePressed() => _handbrakeHeld = true;

        private void HandleHandbrakeReleased() => _handbrakeHeld = false;

        // Dodge has no release event, so it is a quick tap: it engages the handbrake for the next
        // physics step only, which is cleared at the end of FixedUpdate.
        private void HandleHandbrakeTapped() => _handbrakeTapped = true;

        // All physics work happens here so forces stay in step with the Rigidbody.
        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;

            // If a ground check is configured and we are airborne, skip drive forces. Gravity still
            // pulls the car down through the Rigidbody, so it simply falls until it lands.
            bool grounded = IsGrounded();
            if (!grounded)
            {
                _handbrakeTapped = false;
                return;
            }

            ApplyThrottle(dt);
            ApplySteering(dt);
            ApplyGrip();
            ApplyDownforce();

            // A dodge tap only loosens grip for this single step, so clear it now. A held jump keeps
            // _handbrakeHeld set on its own until its release event fires.
            _handbrakeTapped = false;
        }

        // Optional downward raycast. When GroundRayDistance is zero the car is always considered
        // grounded, which keeps the simplest setup working with no colliders to configure.
        private bool IsGrounded()
        {
            if (MovementProfile.GroundRayDistance <= 0f)
            {
                return true;
            }

            return Physics.Raycast(
                transform.position, Vector3.down,
                MovementProfile.GroundRayDistance, MovementProfile.DriveableLayers,
                QueryTriggerInteraction.Ignore);
        }

        // Throttle and brake from the stick's vertical axis. Pushing up accelerates toward MaxSpeed,
        // pulling down brakes while moving forward and then drives in reverse, and a centred stick
        // lets the car coast to a stop.
        private void ApplyThrottle(float dt)
        {
            float throttle = _moveInput.y;

            // How fast the car is travelling along its own forward axis. Positive is forward,
            // negative is reverse.
            float forwardSpeed = Vector3.Dot(_rigidbody.linearVelocity, transform.forward);

            if (throttle > 0.01f)
            {
                // Accelerate toward the forward top speed. Stop pushing once we are already there so
                // the car holds its top speed instead of climbing past it.
                if (forwardSpeed < MovementProfile.MaxSpeed)
                {
                    _rigidbody.AddForce(transform.forward * (throttle * MovementProfile.Acceleration), ForceMode.Acceleration);
                }
            }
            else if (throttle < -0.01f)
            {
                if (forwardSpeed > 0.1f)
                {
                    // Still rolling forward, so the stick acts as the brake.
                    _rigidbody.AddForce(transform.forward * (throttle * MovementProfile.BrakeForce), ForceMode.Acceleration);
                }
                else if (forwardSpeed > -MovementProfile.ReverseSpeed)
                {
                    // Stopped or already reversing, so drive backward up to the reverse top speed.
                    _rigidbody.AddForce(transform.forward * (throttle * MovementProfile.Acceleration), ForceMode.Acceleration);
                }
            }
            else
            {
                // No throttle: bleed off forward/back speed gently so the car coasts to rest.
                Vector3 forwardVelocity = transform.forward * forwardSpeed;
                _rigidbody.AddForce(-forwardVelocity * MovementProfile.CoastDrag * dt, ForceMode.Acceleration);
            }
        }

        // Steering from the stick's horizontal axis. The turn is scaled by how fast the car is going
        // so a parked car barely rotates and turning gets crisper as it picks up speed. We also fade
        // the steer with the sign of travel so reversing turns the wheel the natural way.
        private void ApplySteering(float dt)
        {
            float steer = _moveInput.x;
            if (Mathf.Abs(steer) < 0.01f)
            {
                return;
            }

            float forwardSpeed = Vector3.Dot(_rigidbody.linearVelocity, transform.forward);

            // 0 when stopped, climbing to 1 once we reach the reference speed. This is what keeps the
            // car from spinning in place.
            float speedFactor = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / MovementProfile.SteerSpeedReference);

            // Reverse the steering direction when backing up, matching how a real car feels.
            float direction = Mathf.Sign(forwardSpeed == 0f ? 1f : forwardSpeed);

            float turn = steer * direction * MovementProfile.SteerRate * speedFactor * dt;
            Quaternion delta = Quaternion.Euler(0f, turn, 0f);
            _rigidbody.MoveRotation(_rigidbody.rotation * delta);
        }

        // Grip cancels sideways (lateral) velocity so the car follows its nose instead of sliding
        // around like it is on ice. Holding the handbrake swaps to the much lower DriftGrip, letting
        // the rear break loose for a controlled slide.
        private void ApplyGrip()
        {
            bool drifting = _handbrakeHeld || _handbrakeTapped;
            float grip = drifting ? MovementProfile.DriftGrip : MovementProfile.Grip;

            // Split the velocity into the part along the car's right axis (the sideways slide) and
            // remove a fraction of it. Removing all of it (grip = 1) snaps instantly to the heading.
            Vector3 lateralVelocity = transform.right * Vector3.Dot(_rigidbody.linearVelocity, transform.right);
            _rigidbody.AddForce(-lateralVelocity * grip, ForceMode.VelocityChange);
        }

        // Extra downward force at speed. This is purely a feel/assist knob that helps the car stay
        // planted through fast turns instead of feeling like it might lift off.
        private void ApplyDownforce()
        {
            if (MovementProfile.Downforce <= 0f)
            {
                return;
            }

            float speed = _rigidbody.linearVelocity.magnitude;
            _rigidbody.AddForce(Vector3.down * (MovementProfile.Downforce * speed), ForceMode.Acceleration);
        }
    }
}
