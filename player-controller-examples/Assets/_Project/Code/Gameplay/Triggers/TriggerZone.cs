using UnityEngine;

namespace _Project.Code.Gameplay.Triggers
{
    // Base for box/sphere trigger zones. Marks its collider as a trigger and raises OnZoneEntered /
    // OnZoneExited when a collider on one of the accepted layers crosses it. Filtering is by layer
    // rather than a shared player base type, so any of the player archetypes drive it without a common
    // controller class. Draws the zone bounds as a gizmo so the volume is visible in the editor.
    [RequireComponent(typeof(Collider))]
    public abstract class TriggerZone : MonoBehaviour
    {
        [SerializeField, Tooltip("Layers that count as the player. Leave as Everything to accept any body.")]
        private LayerMask _triggerLayers = ~0;

        protected Collider Trigger { get; private set; }

        protected virtual void Awake()
        {
            Trigger = GetComponent<Collider>();
            Trigger.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsValidTrigger(other))
            {
                OnZoneEntered(other.gameObject);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (IsValidTrigger(other))
            {
                OnZoneExited(other.gameObject);
            }
        }

        protected virtual bool IsValidTrigger(Collider other)
        {
            return (_triggerLayers.value & (1 << other.gameObject.layer)) != 0;
        }

        protected abstract void OnZoneEntered(GameObject obj);

        protected abstract void OnZoneExited(GameObject obj);

        protected virtual void OnDrawGizmos()
        {
            if (Trigger == null)
            {
                Trigger = GetComponent<Collider>();
            }

            if (Trigger == null)
            {
                return;
            }

            Gizmos.color = GetGizmoColor();
            Gizmos.matrix = transform.localToWorldMatrix;

            if (Trigger is BoxCollider box)
            {
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (Trigger is SphereCollider sphere)
            {
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
        }

        protected virtual Color GetGizmoColor()
        {
            return new Color(0f, 1f, 1f, 0.3f);
        }
    }
}
