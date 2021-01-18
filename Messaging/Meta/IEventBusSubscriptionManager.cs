using System;
using System.Collections.Generic;
using Messaging.Models;

namespace Messaging.Meta
{
    public interface IEventBusSubscriptionManager
    {
        bool IsEmpty { get; }
        event EventHandler<string> OnEventRemoved;
        
        void AddSubscription<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>;

        void RemoveSubscription<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>;

        bool HasSubscriptionsForEvent<TEvent>() where TEvent : Event;
        bool HasSubscriptionsForEvent(string eventName);
        Type GetEventType(string eventName);
        void Clear();
        IEnumerable<Subscription> GetHandlersForEvent<TEvent>() where TEvent : Event;
        IEnumerable<Subscription> GetHandlersForEvent(string eventName);
        string GetEventKey<TEvent>() where TEvent : Event;
    }
}