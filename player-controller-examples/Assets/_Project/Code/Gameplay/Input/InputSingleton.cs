using System;
using UnityEngine;
using UnityEngine.InputSystem;
using _Project.Code.Core.Patterns;

namespace _Project.Code.Gameplay.Input
{
    // The one input object for the player. Reads the generated PlayerInputActions and raises
    // events for each intent. Other components subscribe with += and react.
    public class InputSingleton : Singleton<InputSingleton>
    {
        public event Action<Vector2> OnMove;
        public event Action<Vector2> OnLook;
        public event Action OnJump;
        public event Action OnJumpReleased;
        public event Action<bool> OnSprint;
        public event Action OnDodge;
        public event Action OnInteract;
        public event Action OnFire;
        public event Action<bool> OnLockOn;

        private PlayerInputActions _input;

        protected override void Awake()
        {
            base.Awake();
            _input = new PlayerInputActions();
        }

        private void OnEnable()
        {
            _input.Gameplay.Enable();

            _input.Gameplay.Move.performed += HandleMove;
            _input.Gameplay.Move.canceled += HandleMove;
            _input.Gameplay.Look.performed += HandleLook;
            _input.Gameplay.Jump.performed += HandleJump;
            _input.Gameplay.Jump.canceled += HandleJumpReleased;
            _input.Gameplay.Sprint.performed += HandleSprintStart;
            _input.Gameplay.Sprint.canceled += HandleSprintStop;
            _input.Gameplay.Dodge.performed += HandleDodge;
            _input.Gameplay.Interact.performed += HandleInteract;
            _input.Gameplay.Attack.performed += HandleFire;
            _input.Gameplay.LockOn.performed += HandleLockOnStart;
            _input.Gameplay.LockOn.canceled += HandleLockOnStop;
        }

        private void OnDisable()
        {
            _input.Gameplay.Move.performed -= HandleMove;
            _input.Gameplay.Move.canceled -= HandleMove;
            _input.Gameplay.Look.performed -= HandleLook;
            _input.Gameplay.Jump.performed -= HandleJump;
            _input.Gameplay.Jump.canceled -= HandleJumpReleased;
            _input.Gameplay.Sprint.performed -= HandleSprintStart;
            _input.Gameplay.Sprint.canceled -= HandleSprintStop;
            _input.Gameplay.Dodge.performed -= HandleDodge;
            _input.Gameplay.Interact.performed -= HandleInteract;
            _input.Gameplay.Attack.performed -= HandleFire;
            _input.Gameplay.LockOn.performed -= HandleLockOnStart;
            _input.Gameplay.LockOn.canceled -= HandleLockOnStop;

            _input.Gameplay.Disable();
        }

        // Move fires on both performed and canceled, so releasing the stick broadcasts a zero vector.
        private void HandleMove(InputAction.CallbackContext ctx) => OnMove?.Invoke(ctx.ReadValue<Vector2>());

        private void HandleLook(InputAction.CallbackContext ctx) => OnLook?.Invoke(ctx.ReadValue<Vector2>());

        private void HandleJump(InputAction.CallbackContext ctx) => OnJump?.Invoke();

        // Releasing jump is its own event so a controller can cut a jump short for variable height.
        private void HandleJumpReleased(InputAction.CallbackContext ctx) => OnJumpReleased?.Invoke();

        private void HandleSprintStart(InputAction.CallbackContext ctx) => OnSprint?.Invoke(true);

        private void HandleSprintStop(InputAction.CallbackContext ctx) => OnSprint?.Invoke(false);

        private void HandleDodge(InputAction.CallbackContext ctx) => OnDodge?.Invoke();

        private void HandleInteract(InputAction.CallbackContext ctx) => OnInteract?.Invoke();

        private void HandleFire(InputAction.CallbackContext ctx) => OnFire?.Invoke();

        private void HandleLockOnStart(InputAction.CallbackContext ctx) => OnLockOn?.Invoke(true);

        private void HandleLockOnStop(InputAction.CallbackContext ctx) => OnLockOn?.Invoke(false);
    }
}
