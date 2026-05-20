using System;
using System.Collections.Generic;
using UnityEngine;
using Services = _Project.Code.Core.Services;

namespace _Project.Code.Core.Events
{
    /// <summary>
    /// Base class for MonoBehaviours that need to subscribe to events.
    /// Automatically unsubscribes on destroy.
    /// </summary>
    public class EventBusSubscriber : MonoBehaviour
    {
        private readonly List<Action> _unsubscribeActions = new();

        protected void Subscribe<T>(Action<T> callback) where T : IEvent
        {
            if (Services.ServiceLocator.TryGet<IEventBus>(out IEventBus eventBus))
            {
                eventBus.Subscribe(this, callback);
                _unsubscribeActions.Add(() =>
                {
                    if (Services.ServiceLocator.TryGet<IEventBus>(out IEventBus bus))
                    {
                        bus.Unsubscribe<T>(this);
                    }
                });
            }
            else
            {
                Debug.LogWarning($"[EventBusSubscriber] Cannot subscribe to {typeof(T).Name}: EventBus not available.");
            }
        }

        protected virtual void OnDestroy()
        {
            foreach (Action unsubscribe in _unsubscribeActions)
            {
                unsubscribe();
            }
            _unsubscribeActions.Clear();
        }
    }
}
