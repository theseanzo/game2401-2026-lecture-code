using UnityEngine;
using _Project.Code.Core.Events;

namespace _Project.Code.Gameplay.Input
{
    public struct MoveEvent : IEvent
    {
        public Vector2Int Direction;
    }
}
