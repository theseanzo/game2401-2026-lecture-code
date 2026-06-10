using UnityEngine;

namespace _Project.Code.Gameplay.PlayerControllers.Profiles
{
    // Tunables for the arcade car. Inherits MovementProfile for the shared create-asset menu only; the
    // car drives entirely from the fields below, so the inherited WalkSpeed/RunSpeed are unused.
    [CreateAssetMenu(fileName = "VehicleMovementProfile", menuName = "ScriptableObjects/Movement/Vehicle Movement Profile")]
    public class VehicleMovementProfile : MovementProfile
    {
        [field: SerializeField, Header("Drive"), Tooltip("Top forward speed in metres per second the car will accelerate toward at full throttle.")]
        public float MaxSpeed { get; private set; } = 25f;

        [field: SerializeField, Tooltip("How hard the engine pushes the car forward. Higher means it reaches top speed faster.")]
        public float Acceleration { get; private set; } = 30f;

        [field: SerializeField, Tooltip("How hard the brakes slow the car when pulling back while moving forward.")]
        public float BrakeForce { get; private set; } = 45f;

        [field: SerializeField, Tooltip("Top speed in metres per second when driving in reverse. Usually slower than forward.")]
        public float ReverseSpeed { get; private set; } = 8f;

        [field: SerializeField, Tooltip("How quickly the car settles to a stop when no throttle or brake is applied (engine braking / rolling resistance).")]
        public float CoastDrag { get; private set; } = 6f;

        [field: SerializeField, Header("Steering"), Tooltip("Degrees per second the car can turn at full steering. Scaled down by how fast you are going so it does not spin on the spot.")]
        public float SteerRate { get; private set; } = 120f;

        [field: SerializeField, Tooltip("Speed (m/s) at which steering reaches its full SteerRate. Below this, turning is reduced proportionally so a near-stopped car barely turns."), Range(0.1f, 20f)]
        public float SteerSpeedReference { get; private set; } = 8f;

        [field: SerializeField, Header("Grip"), Tooltip("How strongly tyres cancel sideways sliding under normal driving. 0 is pure ice, 1 snaps the car instantly to where it points."), Range(0f, 1f)]
        public float Grip { get; private set; } = 0.9f;

        [field: SerializeField, Tooltip("Reduced grip used while the handbrake is held, letting the rear slide out for a drift."), Range(0f, 1f)]
        public float DriftGrip { get; private set; } = 0.25f;

        [field: SerializeField, Header("Assist"), Tooltip("Extra downward force that presses the car into the ground at speed. Helps it stay planted through turns. Set to 0 to disable.")]
        public float Downforce { get; private set; } = 5f;

        [field: SerializeField, Tooltip("Optional raycast distance used to check whether the car is on the ground before applying drive forces. Set to 0 to always drive."), Range(0f, 3f)]
        public float GroundRayDistance { get; private set; } = 1.2f;

        [field: SerializeField, Tooltip("Which layers count as drivable ground for the optional ground check.")]
        public LayerMask DriveableLayers { get; private set; } = -1;
    }
}
