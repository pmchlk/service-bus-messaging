using System.Threading.Tasks;
using Messaging.AzureServiceBus;
using Messaging.Settings;
using Microsoft.Extensions.Options;
using PaymentService.Domain.Models;
using Shared.Notifications;
using Shared.Notifications.Models;

namespace PaymentService.Infrastructure.Notifications.Publishers.Dispatchers
{
    public class PaymentCompletedEventDispatcher : ServiceBusEventDispatcher<PaymentCompletedEvent>
    {
        public PaymentCompletedEventDispatcher(IOptions<ServiceBusSettings> serviceBusOptions) : base(serviceBusOptions)
        {
        }

        public Task DispatchAsync(Payment payment) => PublishAsync(new PaymentCompletedEvent
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId
        });
    }
}