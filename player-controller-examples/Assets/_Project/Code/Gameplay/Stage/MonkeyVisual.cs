using UnityEngine;

namespace _Project.Code.Gameplay.Stage
{
    public class MonkeyVisual : MonoBehaviour
    {
        [Header("References")]
        // The physics ball this visual rides on. Position is copied from it; rotation is not.
        [SerializeField] private Transform _ball;

        // The stage drives steering; this visual turns to face it.
        [SerializeField] private StageTiltController _stage;

        [Header("Settings")]
        [SerializeField] private Vector3 _offset = Vector3.zero;

        [SerializeField, Tooltip("How fast the visual turns to face the steer direction, in degrees per second")]
        private float _turnSpeed = 540f;

        private Vector3 _steer;

        private void OnEnable()
        {
            if (_stage != null)
            {
                _stage.OnSteer += HandleSteer;
            }
        }

        private void OnDisable()
        {
            if (_stage != null)
            {
                _stage.OnSteer -= HandleSteer;
            }
        }

        private void HandleSteer(Vector3 direction) => _steer = direction;

        // Run after physics has moved the ball so the visual lands on its final position for the frame.
        private void LateUpdate()
        {
            if (_ball != null)
            {
                transform.position = _ball.position + _offset;
            }

            // Face the steer direction on the ground plane only. Keeping the up vector world-up is what
            // holds the visual upright while the ball tumbles freely beneath it.
            Vector3 flat = new Vector3(_steer.x, 0f, _steer.z);
            if (flat.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Quaternion target = Quaternion.LookRotation(flat.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, _turnSpeed * Time.deltaTime);
        }
    }
}
