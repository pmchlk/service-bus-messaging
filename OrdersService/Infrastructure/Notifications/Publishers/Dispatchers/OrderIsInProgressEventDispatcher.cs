using System.Linq;
using System.Threading.Tasks;
using Messaging.AzureServiceBus;
using Messaging.Settings;
using Microsoft.Extensions.Options;
using OrdersService.Domain.Models;
using Shared.Notifications;
using Shared.Notifications.Models;

namespace OrdersService.Infrastructure.Notifications.Publishers.Dispatchers
{
    public class OrderIsInProgressEventDispatcher : ServiceBusEventDispatcher<OrderIsInProgressEvent>
    {
        public OrderIsInProgressEventDispatcher(IOptions<ServiceBusSettings> serviceBusOptions) : base(serviceBusOptions)
        {
        }

        public Task DispatchAsync(Order order) => PublishAsync(new OrderIsInProgressEvent
        {
            OrderId = order.Id,
            Lines = order.Lines.Select(l => new OrderIsInProgressEventLine
            {
                Product_Id = l.Product_Id,
                Quantity = l.Quantity
            }).ToList()
        });
    }
}