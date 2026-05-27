using UnityEngine;

namespace _Project.Code.Gameplay.Interaction
{
    public class CursorBootstrap : MonoBehaviour
    {
        [field: SerializeField] public Texture2D DefaultCursor { get; private set; }
        [field: SerializeField] public Vector2 Hotspot { get; private set; } = Vector2.zero;
        [field: SerializeField] public CursorMode CursorMode { get; private set; } = CursorMode.Auto;

        private void Start()
        {
            Cursor.SetCursor(DefaultCursor, Hotspot, CursorMode);
        }
    }
}
