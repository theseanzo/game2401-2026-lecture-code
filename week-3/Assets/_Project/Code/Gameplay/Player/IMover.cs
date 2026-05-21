using UnityEngine;

namespace _Project.Code.Gameplay.Player
{
    // Implemented by PacmanMover and StepMover so PlayerController can drive either.
    public interface IMover
    {
        void OnMoveInput(Vector2Int direction);
    }
}
