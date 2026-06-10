using UnityEngine;
using System.Collections.Generic;

namespace _Project.Code.Gameplay.Animation
{
    public class PlayerAnimationController : MonoBehaviour
    {
        private Animator _animator;
        private Dictionary<AnimationTrigger, int> _triggerHashes;
        private Dictionary<AnimationParameter, int> _parameterHashes;
        private HashSet<AnimationTrigger> _availableTriggers;
        private HashSet<AnimationParameter> _availableParameters;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();

            if (_animator == null)
            {
                Debug.LogWarning($"PlayerAnimationController on {gameObject.name}: No Animator component found in children.");
                return;
            }

            _triggerHashes = new Dictionary<AnimationTrigger, int>();
            _availableTriggers = new HashSet<AnimationTrigger>();
            foreach (AnimationTrigger trigger in System.Enum.GetValues(typeof(AnimationTrigger)))
            {
                int hash = Animator.StringToHash(trigger.ToString());
                _triggerHashes[trigger] = hash;

                if (HasParameter(hash))
                {
                    _availableTriggers.Add(trigger);
                }
            }

            _parameterHashes = new Dictionary<AnimationParameter, int>();
            _availableParameters = new HashSet<AnimationParameter>();
            foreach (AnimationParameter parameter in System.Enum.GetValues(typeof(AnimationParameter)))
            {
                int hash = Animator.StringToHash(parameter.ToString());
                _parameterHashes[parameter] = hash;

                if (HasParameter(hash))
                {
                    _availableParameters.Add(parameter);
                }
            }
        }

        private bool HasParameter(int hash)
        {
            if (_animator.runtimeAnimatorController == null) return false;

            foreach (var param in _animator.parameters)
            {
                if (param.nameHash == hash)
                    return true;
            }
            return false;
        }

        public void TriggerAnimation(AnimationTrigger trigger)
        {
            if (!_availableTriggers.Contains(trigger)) return;
            _animator?.SetTrigger(_triggerHashes[trigger]);
        }

        public void SetFloat(AnimationParameter parameter, float value)
        {
            if (!_availableParameters.Contains(parameter)) return;
            _animator?.SetFloat(_parameterHashes[parameter], value);
        }

        public void SetBool(AnimationParameter parameter, bool value)
        {
            if (!_availableParameters.Contains(parameter)) return;
            _animator?.SetBool(_parameterHashes[parameter], value);
        }

        public void SetLayerWeight(int layerIndex, float weight)
        {
            if (_animator == null || layerIndex >= _animator.layerCount) return;
            _animator.SetLayerWeight(layerIndex, weight);
        }

        // Called from animation clip events. Hook points for footstep and landing reactions.
        public void OnFootstep() { }

        public void OnJumpLand() { }
    }
}
