using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Code.Core.Interaction
{
    public class MouseInteractor : MonoBehaviour
    {
        [field: SerializeField] public Camera RaycastCamera { get; private set; }
        [field: SerializeField] public float MaxDistance { get; private set; } = Mathf.Infinity;

        private void Reset()
        {
            RaycastCamera = Camera.main;
        }

        private void Update()
        {
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            {
                return;
            }

            Camera cam = RaycastCamera != null ? RaycastCamera : Camera.main;
            if (cam == null)
            {
                return;
            }

            Vector2 screenPosition = Mouse.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(screenPosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, MaxDistance))
            {
                return;
            }

            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                interactable.Interact();
            }
        }
    }
}
