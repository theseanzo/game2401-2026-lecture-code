using System;
using System.Collections.Generic;
using UnityEngine;
using _Project.Code.Core.Services;

namespace _Project.Code.Core.Events
{
    public class EventBusService : MonoBehaviourService<IEventBus>, IEventBus
    {
#if UNITY_EDITOR
        public static event Action<string, object> OnEventPublished;
#endif

        private readonly Dictionary<Type, List<EventSubscription>> _subscriptions = new();
        private readonly List<EventSubscription> _pendingRemovals = new();
        private bool _isPublishing;

        private class EventSubscription
        {
            public WeakReference TargetReference { get; set; }
            public Delegate Callback { get; set; }
            public bool MarkedForRemoval { get; set; }
        }

        public void Subscribe<T>(object target, Action<T> callback) where T : IEvent
        {
            Type eventType = typeof(T);

            if (!_subscriptions.TryGetValue(eventType, out List<EventSubscription> subscriptionList))
            {
                subscriptionList = new List<EventSubscription>();
                _subscriptions[eventType] = subscriptionList;
            }

            subscriptionList.Add(new EventSubscription
            {
                TargetReference = new WeakReference(target),
                Callback = callback
            });
        }

        public void Unsubscribe<T>(object target) where T : IEvent
        {
            Type eventType = typeof(T);

            if (!_subscriptions.TryGetValue(eventType, out List<EventSubscription> subscriptionList))
                return;

            foreach (EventSubscription subscription in subscriptionList)
            {
                if (subscription.TargetReference.Target == target)
                {
                    if (_isPublishing)
                    {
                        subscription.MarkedForRemoval = true;
                        _pendingRemovals.Add(subscription);
                    }
                    else
                    {
                        subscriptionList.Remove(subscription);
                        break;
                    }
                }
            }
        }

        public void Publish<T>(T eventData) where T : IEvent
        {
            Type eventType = typeof(T);

            if (!_subscriptions.TryGetValue(eventType, out List<EventSubscription> subscriptionList))
                return;

            _isPublishing = true;

            for (int i = subscriptionList.Count - 1; i >= 0; i--)
            {
                EventSubscription subscription = subscriptionList[i];

                if (subscription.MarkedForRemoval)
                    continue;

                object target = subscription.TargetReference.Target;

                if (target == null)
                {
                    subscription.MarkedForRemoval = true;
                    _pendingRemovals.Add(subscription);
                    continue;
                }

                try
                {
                    ((Action<T>)subscription.Callback).Invoke(eventData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error publishing event {typeof(T).Name}: {e.Message}");
                }
            }

            _isPublishing = false;
            CleanupPendingRemovals();

#if UNITY_EDITOR
            OnEventPublished?.Invoke(typeof(T).Name, eventData);
#endif
        }

        private void CleanupPendingRemovals()
        {
            if (_pendingRemovals.Count == 0)
                return;

            foreach (EventSubscription subscription in _pendingRemovals)
            {
                foreach (KeyValuePair<Type, List<EventSubscription>> kvp in _subscriptions)
                {
                    kvp.Value.Remove(subscription);
                }
            }

            _pendingRemovals.Clear();
        }

        public void Clear()
        {
            _subscriptions.Clear();
            _pendingRemovals.Clear();
            _isPublishing = false;
        }

        public override void Dispose()
        {
            Clear();
            base.Dispose();
        }
    }
}
