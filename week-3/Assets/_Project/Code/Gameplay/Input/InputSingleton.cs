using System;
using UnityEngine;
using UnityEngine.InputSystem;
using _Project.Code.Core.Patterns;

namespace _Project.Code.Gameplay.Input
{
    // Reads input and raises OnMove with a cardinal direction. Subscribe with += .
    public class InputSingleton : Singleton<InputSingleton>
    {
        public event Action<Vector2Int> OnMove;

        private PlayerInputActions _input;

        protected override void Awake()
        {
            base.Awake();
            _input = new PlayerInputActions();
        }

        private void OnEnable()
        {
            _input.Player.Enable();
            _input.Player.Move.performed += HandleMove;
        }

        private void OnDisable()
        {
            _input.Player.Move.performed -= HandleMove;
            _input.Player.Disable();
        }

        private void HandleMove(InputAction.CallbackContext ctx)
        {
            Vector2Int direction = ToCardinal(ctx.ReadValue<Vector2>());
            if (direction != Vector2Int.zero)
            {
                OnMove?.Invoke(direction);
            }
        }

        // Snap an analog Vector2 to one grid step. y is flipped: "up" is -y (toward row 0).
        private Vector2Int ToCardinal(Vector2 raw)
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
