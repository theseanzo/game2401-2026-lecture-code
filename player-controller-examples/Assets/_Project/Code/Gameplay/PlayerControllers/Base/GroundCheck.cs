using UnityEngine;

namespace _Project.Code.Gameplay.PlayerControllers.Base
{
    public class GroundCheck : MonoBehaviour
    {
        [field: SerializeField, Header("Ground Detection")]
        public Transform GroundCheckOrigin { get; private set; }

        [SerializeField]
        private float _checkDistance = 0.2f;

        [SerializeField]
        private LayerMask _groundLayers = -1;

        public bool IsGrounded { get; private set; }
        public float TimeSinceGrounded { get; private set; }

        private void FixedUpdate()
        {
            if (GroundCheckOrigin == null) return;

            Vector3 origin = GroundCheckOrigin.position;
            IsGrounded = Physics.Raycast(origin, Vector3.down, _checkDistance, _groundLayers);

            if (IsGrounded)
            {
                TimeSinceGrounded = 0f;
            }
            else
            {
                TimeSinceGrounded += Time.fixedDeltaTime;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (GroundCheckOrigin == null) return;

            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Vector3 origin = GroundCheckOrigin.position;
            Gizmos.DrawLine(origin, origin + Vector3.down * _checkDistance);
        }
    }
}
