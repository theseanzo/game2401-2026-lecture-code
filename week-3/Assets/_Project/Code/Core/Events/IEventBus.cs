using System;
using _Project.Code.Core.Services;

namespace _Project.Code.Core.Events
{
    public interface IEventBus : IService
    {
        void Subscribe<T>(object target, Action<T> callback) where T : IEvent;
        void Unsubscribe<T>(object target) where T : IEvent;
        void Publish<T>(T eventData) where T : IEvent;
        void Clear();
    }
}
