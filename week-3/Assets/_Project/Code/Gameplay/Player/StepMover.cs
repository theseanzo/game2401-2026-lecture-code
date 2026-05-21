using UnityEngine;
using _Project.Code.Gameplay.Level;

namespace _Project.Code.Gameplay.Player
{
    // Moves exactly one cell per press, then stops. Driven by PlayerController via OnMoveInput.
    public class StepMover : MonoBehaviour, IMover
    {
        [SerializeField] private float _speed = 8f; // cells per second while sliding

        private Vector2Int _cell;
        private Vector3 _from;
        private Vector3 _to;
        private float _t;
        private bool _moving;

        private void Start()
        {
            _cell = LevelSingleton.Instance.WorldToGrid(transform.position);
            transform.position = LevelSingleton.Instance.GridToWorld(_cell);
        }

        public void OnMoveInput(Vector2Int direction)
        {
            if (_moving)
            {
                return; // finish the current step before starting another
            }

            Vector2Int next = _cell + direction;
            if (!LevelSingleton.Instance.IsWalkable(next.x, next.y))
            {
                return;
            }

            _from = LevelSingleton.Instance.GridToWorld(_cell);
            _to = LevelSingleton.Instance.GridToWorld(next);
            _cell = next;
            _t = 0f;
            _moving = true;
            Face(direction);
        }

        private void Face(Vector2Int dir)
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(dir.x, 0f, -dir.y), Vector3.up);
        }

        private void Update()
        {
            if (!_moving)
            {
                return;
            }

            _t += _speed * Time.deltaTime;
            if (_t >= 1f)
            {
                transform.position = _to;
                _moving = false;
                return;
            }

            transform.position = Vector3.Lerp(_from, _to, _t);
        }
    }
}
