using System.Collections.Generic;
using System.Threading.Tasks;
using Messaging.Models;

namespace Messaging.Meta
{
    public interface IEventDispatcher<TEvent> where TEvent : Event
    {
        void Publish(TEvent @event);
        Task PublishAsync(TEvent @event);
        Task PublishManyAsync(ICollection<TEvent> @events);
    }
}