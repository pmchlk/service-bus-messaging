using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Messaging.Meta;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrdersService.Domain.Enums;
using OrdersService.Infrastructure.Notifications.Publishers.Dispatchers;
using OrdersService.Persistence;
using Shared.Notifications;
using Shared.Notifications.Models;
using Shared.Persistence.Database.Meta;

namespace OrdersService.Infrastructure.Notifications.Subscribers.Handlers
{
    public class PaymentCompletedEventHandler : IEventHandler<PaymentCompletedEvent>
    {
        private readonly OrderDbContext _context;
        private readonly OrderIsInProgressEventDispatcher _orderIsInProgressEventDispatcher;
        private readonly ILogger<PaymentCompletedEventHandler> _logger;
        private const string LOGGING_PREFIX = "[PaymentCompletedHandler:OrderService]";

        public PaymentCompletedEventHandler(
            OrderIsInProgressEventDispatcher orderIsInProgressEventDispatcher,
            IDatabaseContextProvider<OrderDbContext> contextProvider,
            ILogger<PaymentCompletedEventHandler> logger)
        {
            _context = contextProvider.GetContext() ?? throw new ArgumentNullException(nameof(contextProvider));
            _orderIsInProgressEventDispatcher = orderIsInProgressEventDispatcher ?? throw new ArgumentNullException(nameof(orderIsInProgressEventDispatcher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(PaymentCompletedEvent @event)
        {
            _logger.LogInformation($"{LOGGING_PREFIX} Started handling event with id '{@event.Id}'");
            try
            {
                var order = await _context.Orders.Include(o => o.Lines).SingleAsync(o => o.Id == @event.OrderId);
                if (order == null)
                    throw new KeyNotFoundException($"Order with id {@event.OrderId} does not exist");

                order.Status = OrderStatusEnum.IN_PROGRESS;
                await _context.SaveChangesAsync();
                await _orderIsInProgressEventDispatcher.DispatchAsync(order);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{LOGGING_PREFIX} Error occured during handling event with id '{@event.Id}'");
                throw;
            }

            _logger.LogInformation($"{LOGGING_PREFIX} Successfully handled event with id '{@event.Id}'");
        }
    }
}