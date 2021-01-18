using Messaging.Models;

namespace Shared.Notifications.Models
{
    public class PaymentCompletedEvent : Event
    {
        public long OrderId { get; set; }
        public long PaymentId { get; set; }
    }
}