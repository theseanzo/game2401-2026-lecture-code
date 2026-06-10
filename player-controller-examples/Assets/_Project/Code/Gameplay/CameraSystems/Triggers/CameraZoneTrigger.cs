using UnityEngine;
using Unity.Cinemachine;
using _Project.Code.Gameplay.Triggers;

namespace _Project.Code.Gameplay.CameraSystems.Triggers
{
    // Walk into the zone and its camera becomes live; leave and the previous framing returns. Switching
    // is by Cinemachine priority alone, so there is no camera service to wire up: on enter the zone
    // raises its target camera's Priority, on exit it drops it back to the cached base so whatever
    // camera was live before resumes. With Restore Previous Camera off, the zone camera stays live.
    public class CameraZoneTrigger : TriggerZone
    {
        [Header("Camera Settings")]
        [SerializeField] private CinemachineCamera _cameraToActivate;

        [SerializeField, Tooltip("Priority given to the zone camera while the player is inside")]
        private int _activePriority = 20;

        [Header("Exit Behavior")]
        [SerializeField] private bool _restorePreviousCamera = true;

        private int _basePriority;
        private bool _isActive;

        protected override void OnZoneEntered(GameObject obj)
        {
            if (_cameraToActivate == null || _isActive)
            {
                return;
            }

            // Remember the camera's standing priority so exit can hand control back.
            _basePriority = _cameraToActivate.Priority.Value;
            _cameraToActivate.Priority = _activePriority;
            _isActive = true;
        }

        protected override void OnZoneExited(GameObject obj)
        {
            if (_cameraToActivate == null || !_isActive)
            {
                return;
            }

            if (_restorePreviousCamera)
            {
                _cameraToActivate.Priority = _basePriority;
            }

            _isActive = false;
        }

        protected override Color GetGizmoColor()
        {
            return new Color(0f, 1f, 1f, 0.3f);
        }
    }
}
