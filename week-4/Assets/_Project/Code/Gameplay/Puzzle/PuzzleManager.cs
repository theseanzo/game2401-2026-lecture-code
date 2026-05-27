using System;
using System.Collections.Generic;
using _Project.Code.Core.Patterns;

namespace _Project.Code.Gameplay.Puzzle
{
    public class PuzzleManager : Singleton<PuzzleManager>
    {
        private readonly List<Puzzle> _puzzles = new();

        public IReadOnlyList<Puzzle> Puzzles => _puzzles;

        public event Action<Puzzle> OnPuzzleSolved;
        public event Action OnAllSolved;

        public void Register(Puzzle puzzle)
        {
            if (puzzle == null || _puzzles.Contains(puzzle))
            {
                return;
            }
            _puzzles.Add(puzzle);
        }

        public void Unregister(Puzzle puzzle)
        {
            _puzzles.Remove(puzzle);
        }

        public void NotifySolved(Puzzle puzzle)
        {
            OnPuzzleSolved?.Invoke(puzzle);
            if (AllSolved())
            {
                OnAllSolved?.Invoke();
            }
        }

        private bool AllSolved()
        {
            if (_puzzles.Count == 0)
            {
                return false;
            }
            foreach (Puzzle puzzle in _puzzles)
            {
                if (!puzzle.IsSolved)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
