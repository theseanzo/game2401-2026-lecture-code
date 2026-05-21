using UnityEngine;
using _Project.Code.Gameplay.Input;
using _Project.Code.Gameplay.Level;

namespace _Project.Code.Gameplay.Player
{
    public class PlayerController : MonoBehaviour
    {
        private IMover _mover;
        private Vector2Int _lastCell = new Vector2Int(int.MinValue, int.MinValue);

        private void Start()
        {
            _mover = GetComponent<IMover>();
            InputSingleton.Instance.OnMove += OnMove;
        }

        private void OnDestroy()
        {
            InputSingleton.Instance.OnMove -= OnMove;
        }

        private void OnMove(Vector2Int direction) => _mover?.OnMoveInput(direction);

        // Collect by grid position (no trigger colliders): when we move onto a new cell, the level
        // removes any collectable sitting there.
        private void Update()
        {
            Vector2Int cell = LevelSingleton.Instance.WorldToGrid(transform.position);
            if (cell != _lastCell)
            {
                _lastCell = cell;
                LevelSingleton.Instance.TryCollect(cell);
            }
        }
    }
}
