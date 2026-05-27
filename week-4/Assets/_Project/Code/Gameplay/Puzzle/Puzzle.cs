using System;
using UnityEngine;

namespace _Project.Code.Gameplay.Puzzle
{
    public abstract class Puzzle : MonoBehaviour
    {
        public bool IsSolved { get; private set; }

        public event Action<Puzzle> OnSolved;

        protected virtual void Start()
        {
            PuzzleManager.Instance.Register(this);
        }

        protected virtual void OnDestroy()
        {
            if (PuzzleManager.Instance != null)
            {
                PuzzleManager.Instance.Unregister(this);
            }
        }

        protected void MarkSolved()
        {
            if (IsSolved)
            {
                return;
            }
            IsSolved = true;
            OnSolved?.Invoke(this);
            PuzzleManager.Instance.NotifySolved(this);
        }
    }
}
