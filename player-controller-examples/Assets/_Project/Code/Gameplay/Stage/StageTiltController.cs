using System;
using UnityEngine;
using _Project.Code.Gameplay.Input;

namespace _Project.Code.Gameplay.Stage
{
    [RequireComponent(typeof(Rigidbody))]
    public class StageTiltController : MonoBehaviour
    {
        // The horizontal direction the player is steering this physics step, in world space (zero when
        // the stick is centred). A ball rider subscribes to face where it is rolling.
        public event Action<Vector3> OnSteer;

        [field: SerializeField, Header("Stage Tilt Settings")]
        public StageTiltProfile Profile { get; private set; }

        [Header("References")]
        // Tilt is measured against this camera, so pushing the stick away from the view dips the far
        // edge down. Leave empty to use the main camera.
        [SerializeField] private Transform _cameraTransform;

        private Rigidbody _rigidbody;
        private InputSingleton _input;
        private Vector2 _moveInput;

        // The stage's authored orientation. Tilt is layered on top of this, so a pre-angled stage keeps
        // its base pose.
        private Quaternion _restRotation;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.isKinematic = true;
            _restRotation = _rigidbody.rotation;

            if (_cameraTransform == null && Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
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

        private void HandleMove(Vector2 value) => _moveInput = value;

        private void FixedUpdate()
        {
            Vector3 pushDirection = CameraRelative(_moveInput);
            OnSteer?.Invoke(pushDirection);

            float angle = Mathf.Min(pushDirection.magnitude, 1f) * Profile.MaxTiltAngle;

            Quaternion target = _restRotation;
            if (angle > 0.0001f)
            {
                // The tilt axis is the horizontal direction perpendicular to the push, so the edge the
                // player pushes toward is the edge that drops.
                Vector3 axis = Vector3.Cross(Vector3.up, pushDirection.normalized);
                target = Quaternion.AngleAxis(angle, axis) * _restRotation;
            }

            // Ease toward the target and drive it with MoveRotation: a kinematic body rotates the floor
            // collider so the ball resting on it is carried, rather than left behind by a transform poke.
            Quaternion next = Quaternion.RotateTowards(_rigidbody.rotation, target, Profile.TiltSpeed * Time.fixedDeltaTime);
            _rigidbody.MoveRotation(next);
        }

        // Flatten the camera's forward and right onto the ground plane and blend them by the stick, so
        // tilt is relative to the view and camera pitch never changes which way the stage leans.
        private Vector3 CameraRelative(Vector2 input)
        {
            if (_cameraTransform == null)
            {
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
    }
}
