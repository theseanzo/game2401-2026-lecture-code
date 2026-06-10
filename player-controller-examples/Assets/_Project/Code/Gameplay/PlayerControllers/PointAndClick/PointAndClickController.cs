using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using _Project.Code.Gameplay.PlayerControllers.Profiles;
using _Project.Code.Gameplay.Animation;

namespace _Project.Code.Gameplay.PlayerControllers.PointAndClick
{
    // NavMeshAgent click-to-move: left-click a point and the agent paths there, slowing as it nears.
    // The mouse is read directly through the Input System, not InputSingleton. Requires a baked NavMesh
    // in the scene and a NavMeshAgent on this GameObject.
    [RequireComponent(typeof(NavMeshAgent))]
    public class PointAndClickController : MonoBehaviour
    {
        [field: SerializeField, Header("Point & Click Settings")]
        public PointClickMovementProfile MovementProfile { get; private set; }

        private NavMeshAgent _agent;
        private PlayerAnimationController _animation;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animation = GetComponent<PlayerAnimationController>();
        }

        private void Start()
        {
            // How close to the destination the agent settles before it considers itself arrived.
            _agent.stoppingDistance = MovementProfile.StoppingDistance;
        }

        private void Update()
        {
            HandleClickToMove();
            ApplySlowdown();
            UpdateAnimation();
        }

        // Read the mouse directly. On a left-click this frame, fire a ray from the main camera
        // through the cursor and, if it lands on a clickable surface, send the agent to that point.
        private void HandleClickToMove()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || Camera.main == null)
            {
                return;
            }

            if (!mouse.leftButton.wasPressedThisFrame)
            {
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, MovementProfile.ClickableLayers))
            {
                _agent.SetDestination(hit.point);
            }
        }

        // Ease the agent's speed down as it nears the destination so it does not stop abruptly.
        // Outside the slowdown band it travels at full WalkSpeed; inside, speed scales toward zero.
        private void ApplySlowdown()
        {
            // A path that is still being calculated has no meaningful remaining distance yet.
            if (_agent.pathPending)
            {
                _agent.speed = MovementProfile.WalkSpeed;
                return;
            }

            float distanceRemaining = _agent.remainingDistance;

            if (distanceRemaining < MovementProfile.SlowdownDistance)
            {
                float speedMultiplier = Mathf.Clamp01(distanceRemaining / MovementProfile.SlowdownDistance);
                _agent.speed = MovementProfile.WalkSpeed * speedMultiplier;
            }
            else
            {
                _agent.speed = MovementProfile.WalkSpeed;
            }
        }

        // Feed the Animator a normalized Speed value. PlayerAnimationController is null-safe here and
        // ignores the parameter if the Animator Controller does not define it.
        private void UpdateAnimation()
        {
            _animation?.SetFloat(AnimationParameter.Speed, _agent.velocity.magnitude / MovementProfile.WalkSpeed);
        }
    }
}
