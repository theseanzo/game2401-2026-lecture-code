using System;
using System.Collections;
using _Project.Code.Core.Patterns;
using Unity.Cinemachine;
using UnityEngine;

namespace _Project.Code.Gameplay.Level
{
    public class LevelManager : Singleton<LevelManager>
    {
        [Serializable]
        public struct NodeCameraBinding
        {
            public NodeId NodeId;
            public CinemachineCamera Vcam;
        }

        [field: Header("Start state")]
        [field: SerializeField] public NodeId StartingNode { get; private set; } = NodeId.LeadUpToChurch;

        [field: Header("Camera rig")]
        [field: SerializeField] public NodeCameraBinding[] Cameras { get; private set; }
        [field: SerializeField] public float TransitionSeconds { get; private set; } = 1.5f;

        public NodeId CurrentNode { get; private set; }
        public bool IsTransitioning { get; private set; }

        public event Action<NodeId> OnNodeEntered;

        protected override void Awake()
        {
            base.Awake();
            CurrentNode = StartingNode;
        }

        private void Start()
        {
            ActivateOnly(CurrentNode);
            OnNodeEntered?.Invoke(CurrentNode);
        }

        private void ActivateOnly(NodeId node)
        {
            if (Cameras == null)
            {
                return;
            }
            foreach (NodeCameraBinding binding in Cameras)
            {
                if (binding.Vcam == null)
                {
                    continue;
                }
                binding.Vcam.Priority = (binding.NodeId == node) ? 100 : 10;
            }
        }

        public void RequestTransition(NodeId target)
        {
            if (IsTransitioning)
            {
                return;
            }
            if (target == CurrentNode || target == NodeId.None)
            {
                return;
            }

            CinemachineCamera targetVcam = LookupVcam(target);
            if (targetVcam == null)
            {
                return;
            }

            StartCoroutine(TransitionRoutine(target, targetVcam));
        }

        private IEnumerator TransitionRoutine(NodeId target, CinemachineCamera targetVcam)
        {
            IsTransitioning = true;

            CinemachineCamera currentVcam = LookupVcam(CurrentNode);
            targetVcam.Priority = 100;
            if (currentVcam != null && currentVcam != targetVcam)
            {
                currentVcam.Priority = 10;
            }

            yield return new WaitForSeconds(TransitionSeconds);

            CurrentNode = target;
            IsTransitioning = false;
            OnNodeEntered?.Invoke(CurrentNode);
        }

        private CinemachineCamera LookupVcam(NodeId id)
        {
            if (Cameras == null)
            {
                return null;
            }
            foreach (NodeCameraBinding binding in Cameras)
            {
                if (binding.NodeId == id)
                {
                    return binding.Vcam;
                }
            }
            return null;
        }
    }
}
