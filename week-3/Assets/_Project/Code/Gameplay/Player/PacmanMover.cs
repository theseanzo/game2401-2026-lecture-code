using UnityEngine;
using _Project.Code.Gameplay.Level;

namespace _Project.Code.Gameplay.Player
{
    // Glides cell-to-cell, keeping its direction until blocked; a new direction takes effect at the
    // next cell (buffered). Driven by PlayerController via OnMoveInput.
    public class PacmanMover : MonoBehaviour, IMover
    {
        [SerializeField] private float _speed = 5f; // cells per second

        private Vector2Int _cell;      // cell we're leaving
        private Vector2Int _dir;       // current heading (zero = stopped)
        private Vector2Int _desired;   // last requested direction (buffered)
        private Vector3 _from;
        private Vector3 _to;
        private float _t;              // 0..1 glide progress
        private bool _moving;

        private void Start()
        {
            _cell = LevelSingleton.Instance.WorldToGrid(transform.position);
            transform.position = LevelSingleton.Instance.GridToWorld(_cell);
        }

        public void OnMoveInput(Vector2Int direction) => _desired = direction;

        private void Update()
        {
            if (_moving)
            {
                Glide();
            }
            else
            {
                TryLeave(_cell);
            }
        }

        private void Glide()
        {
            _t += _speed * Time.deltaTime;

            if (_t >= 1f)
            {
                _cell += _dir;
                transform.position = _to;
                _moving = false;
                _t = 0f;
                TryLeave(_cell);
                return;
            }

            transform.position = Vector3.Lerp(_from, _to, _t);
        }

        // Begin gliding to the next cell. Prefer the buffered direction, else keep going straight.
        private bool TryLeave(Vector2Int from)
        {
            Vector2Int next = Vector2Int.zero;
            if (_desired != Vector2Int.zero && IsWalkable(from + _desired))
            {
                next = _desired;
            }
            else if (_dir != Vector2Int.zero && IsWalkable(from + _dir))
            {
                next = _dir;
            }

            if (next == Vector2Int.zero)
            {
                _dir = Vector2Int.zero;
                return false;
            }

            _dir = next;
            _from = LevelSingleton.Instance.GridToWorld(from);
            _to = LevelSingleton.Instance.GridToWorld(from + next);
            _moving = true;
            Face(next);
            return true;
        }

        private bool IsWalkable(Vector2Int cell) => LevelSingleton.Instance.IsWalkable(cell.x, cell.y);

        private void Face(Vector2Int dir)
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(dir.x, 0f, -dir.y), Vector3.up);
        }
    }
}
