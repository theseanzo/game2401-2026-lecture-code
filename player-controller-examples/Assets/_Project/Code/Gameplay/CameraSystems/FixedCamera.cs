using UnityEngine;
using Unity.Cinemachine;

namespace _Project.Code.Gameplay.CameraSystems
{
    // A camera that holds the fixed position and rotation it has in the scene. Drop it on a
    // CinemachineCamera GameObject that has no Position/Rotation Control and it acts as a static
    // vantage point. Turn on Track Target and assign a target (usually the player) to keep the fixed
    // position but ease the rotation to face that target. No services, events, or profiles: the
    // CinemachineCamera owns its own lens and priority; CameraZoneTrigger owns the switching.
    [RequireComponent(typeof(CinemachineCamera))]
    public class FixedCamera : MonoBehaviour
    {
        [Header("Tracking")]
        [SerializeField, Tooltip("Stay at the fixed position but rotate to face the target")]
        private bool _trackTarget;

        [SerializeField, Tooltip("Target to look at when Track Target is on (usually the player)")]
        private Transform _target;

        [SerializeField, Tooltip("Height above the target the camera aims at")]
        private float _trackingHeightOffset = 1f;

        [SerializeField, Tooltip("How quickly the camera eases toward the look direction")]
        private float _trackingSpeed = 5f;

        private Vector3 _fixedPosition;
        private Quaternion _fixedRotation;

        private void Awake()
        {
            // Capture the scene-authored pose as the pose to hold.
            _fixedPosition = transform.position;
            _fixedRotation = transform.rotation;
        }

        private void LateUpdate()
        {
            // Always hold the captured position; only rotation changes when tracking.
            transform.position = _fixedPosition;

            if (_trackTarget && _target != null)
            {
                Vector3 lookPoint = _target.position + Vector3.up * _trackingHeightOffset;
                Vector3 toTarget = lookPoint - transform.position;

                if (toTarget.sqrMagnitude > 0.0001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(toTarget.normalized);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation, targetRotation, _trackingSpeed * Time.deltaTime);
                }
            }
            else
            {
                transform.rotation = _fixedRotation;
            }
        }
    }
}
