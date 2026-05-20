using UnityEngine;
using UnityEngine.InputSystem;
using _Project.Code.Core.Events;
using _Project.Code.Core.Services;

namespace _Project.Code.Gameplay.Input
{
    public class InputPublisher : MonoBehaviour
    {
        private PlayerInputActions _input;
        private IEventBus _eventBus;
        private Vector2Int _lastDirection = Vector2Int.zero;

        private void Awake()
        {
            _input = new PlayerInputActions();
        }

        private async void Start()
        {
            _eventBus = await ServiceLocator.WaitFor<IEventBus>();
            _input.Player.Move.performed += OnMove;
            _input.Player.Enable();
        }

        private void OnDestroy()
        {
            _input.Player.Move.performed -= OnMove;
            _input.Dispose();
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            Vector2Int direction = ToCardinal(ctx.ReadValue<Vector2>());
            if (direction == _lastDirection || direction == Vector2Int.zero)
            {
                return;
            }

            _lastDirection = direction;
            _eventBus.Publish(new MoveEvent { Direction = direction });
        }

        private static Vector2Int ToCardinal(Vector2 raw)
        {
            if (Mathf.Abs(raw.x) < 0.5f && Mathf.Abs(raw.y) < 0.5f)
            {
                return Vector2Int.zero;
            }

            return Mathf.Abs(raw.x) >= Mathf.Abs(raw.y)
                ? new Vector2Int(raw.x > 0f ? 1 : -1, 0)
                : new Vector2Int(0, raw.y > 0f ? -1 : 1);
        }
    }
}
