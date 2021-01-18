using System;
using System.Threading.Tasks;
using Messaging.Models;

namespace Messaging.Meta
{
    public interface IEventBus : IDisposable
    { 
        void Subscribe<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>;
        
        void Unsubscribe<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>;
    }
}