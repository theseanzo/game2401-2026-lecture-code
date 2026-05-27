using System;
using System.Collections;
using UnityEngine;

namespace _Project.Code.Gameplay.Puzzle
{
    public class GatePuzzle : Puzzle
    {
        [Serializable]
        public struct SwordTarget
        {
            public RotatableSword Sword;
            public SwordOrientation Target;
        }

        [field: Header("Inputs")]
        [field: SerializeField] public SwordTarget[] SwordTargets { get; private set; }

        [field: Header("Open animation")]
        [field: SerializeField] public Transform GateRoot { get; private set; }
        [field: SerializeField] public Vector3 OpenLocalPositionOffset { get; private set; } = new Vector3(0f, 3f, 0f);
        [field: SerializeField] public float OpenSeconds { get; private set; } = 1f;
        [field: SerializeField] public GameObject Reveal { get; private set; }

        protected override void Start()
        {
            base.Start();
            foreach (SwordTarget entry in SwordTargets)
            {
                if (entry.Sword != null)
                {
                    entry.Sword.OnRotated += HandleSwordRotated;
                }
            }
        }

        protected override void OnDestroy()
        {
            foreach (SwordTarget entry in SwordTargets)
            {
                if (entry.Sword != null)
                {
                    entry.Sword.OnRotated -= HandleSwordRotated;
                }
            }
            base.OnDestroy();
        }

        private void HandleSwordRotated(RotatableSword _)
        {
            if (IsSolved || !AllSwordsMatchTarget())
            {
                return;
            }
            OpenGate();
            MarkSolved();
        }

        private bool AllSwordsMatchTarget()
        {
            if (SwordTargets == null || SwordTargets.Length == 0)
            {
                return false;
            }
            foreach (SwordTarget entry in SwordTargets)
            {
                if (entry.Sword == null || entry.Sword.Orientation != entry.Target)
                {
                    return false;
                }
            }
            return true;
        }

        private void OpenGate()
        {
            if (GateRoot != null)
            {
                StartCoroutine(OpenRoutine());
            }
            if (Reveal != null)
            {
                Reveal.SetActive(true);
            }
        }

        private IEnumerator OpenRoutine()
        {
            Vector3 start = GateRoot.localPosition;
            Vector3 end = start + OpenLocalPositionOffset;
            float t = 0f;
            while (t < OpenSeconds)
            {
                t += Time.deltaTime;
                GateRoot.localPosition = Vector3.Lerp(start, end, t / OpenSeconds);
                yield return null;
            }
            GateRoot.localPosition = end;
        }
    }
}
