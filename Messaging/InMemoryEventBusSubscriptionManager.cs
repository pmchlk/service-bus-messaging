using System;
using System.Collections.Generic;
using System.Linq;
using Messaging.Meta;
using Messaging.Models;

namespace Messaging
{
    public class InMemoryEventBusSubscriptionManager : IEventBusSubscriptionManager
    {
        private readonly Dictionary<string, ICollection<Subscription>> _handlers;
        private readonly ICollection<Type> _eventTypes;

        public event EventHandler<string> OnEventRemoved;

        public InMemoryEventBusSubscriptionManager()
        {
            _handlers = new Dictionary<string, ICollection<Subscription>>();
            _eventTypes = new List<Type>();
        }

        public bool IsEmpty => !_handlers.Keys.Any();
        public void Clear() => _handlers.Clear();

        public void AddSubscription<TEvent, TEventHandler>() where TEvent : Event where TEventHandler : IEventHandler<TEvent>
        {
            var key = GetEventKey<TEvent>();
            DoAddSubscription(typeof(TEventHandler), key);

            if (!_eventTypes.Contains(typeof(TEvent)))
                _eventTypes.Add(typeof(TEvent));
        }
        
        private void DoAddSubscription(Type handlerType, string eventName)
        {
            if(!HasSubscriptionsForEvent(eventName))
                _handlers.Add(eventName, new List<Subscription>());

            if (_handlers[eventName].Any(s => s.HandlerType == handlerType))
                throw new ArgumentException(
                    $"Handler of type {handlerType.Name} is already registered for '{eventName}'", nameof(handlerType));

            _handlers[eventName].Add(new Subscription(handlerType));
        }

        public void RemoveSubscription<TEvent, TEventHandler>() where TEvent : Event where TEventHandler : IEventHandler<TEvent>
        {
            var handlerToRemove = FindSubscriptionToRemove<TEvent, TEventHandler>();
            var eventName = GetEventKey<TEvent>();
            DoRemoveHandler(eventName, handlerToRemove);
        }
        
        private void DoRemoveHandler(string eventName, Subscription subscriptionToRemove)
        {
            if (subscriptionToRemove != null)
            {
                _handlers[eventName].Remove(subscriptionToRemove);
                if (!_handlers[eventName].Any())
                {
                    _handlers.Remove(eventName);
                    var eventType = _eventTypes.SingleOrDefault(e =>
                        string.Equals(e.Name, eventName, StringComparison.InvariantCultureIgnoreCase));
                    if (eventType != null)
                        _eventTypes.Remove(eventType);
                }
            }
        }

        public IEnumerable<Subscription> GetHandlersForEvent<TEvent>() where TEvent : Event
        {
            var key = GetEventKey<TEvent>();
            return GetHandlersForEvent(key);
        }

        public IEnumerable<Subscription> GetHandlersForEvent(string eventName) => _handlers[eventName];

        public bool HasSubscriptionsForEvent<TEvent>() where TEvent : Event
        {
            var key = GetEventKey<TEvent>();
            return HasSubscriptionsForEvent(key);
        }

        public bool HasSubscriptionsForEvent(string eventName) => _handlers.ContainsKey(eventName);

        public string GetEventKey<TEvent>() where TEvent : Event
        {
            return typeof(TEvent).Name;
        }

        public Type GetEventType(string eventName) => _eventTypes.SingleOrDefault(t =>
            string.Equals(t.Name, eventName, StringComparison.InvariantCultureIgnoreCase));
        
        private void RaiseOnEventRemoved(string eventName)
        {
            var handler = OnEventRemoved;
            handler?.Invoke(this, eventName);
        }
        private Subscription FindSubscriptionToRemove<TEvent, TEventHandler>() 
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>
        {
            var key = GetEventKey<TEvent>();
            return DoFindSubscriptionToRemove(key, typeof(TEventHandler));
        }

        private Subscription FindSubscriptionToRemove(string eventName, Type handlerType) =>
            DoFindSubscriptionToRemove(eventName, handlerType);

        private Subscription DoFindSubscriptionToRemove(string eventName, Type handlerType) =>
            HasSubscriptionsForEvent(eventName)
                ? _handlers[eventName].Single(s => s.HandlerType == handlerType)
                : null;
    }
}