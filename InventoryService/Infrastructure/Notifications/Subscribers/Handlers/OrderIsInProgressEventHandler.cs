using System;
using System.Linq;
using System.Threading.Tasks;
using InventoryService.Persistence.Database;
using Messaging.Meta;
using Messaging.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Notifications;
using Shared.Notifications.Models;
using Shared.Persistence.Database.Meta;

namespace InventoryService.Infrastructure.Notifications.Subscribers.Handlers
{
    public class OrderIsInProgressEventHandler : IEventHandler<OrderIsInProgressEvent>
    {
        private readonly InventoryDbContext _context;
        private readonly ILogger<OrderIsInProgressEventHandler> _logger;
        private const string LOGGING_PREFIX = "[OrderIsInProgressHandler:InventoryService]";

        public OrderIsInProgressEventHandler(
            IDatabaseContextProvider<InventoryDbContext> contextProvider,
            ILogger<OrderIsInProgressEventHandler> logger)
        {
            _context = contextProvider.GetContext() ?? throw new ArgumentNullException(nameof(contextProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(OrderIsInProgressEvent @event)
        {
            _logger.LogInformation($"{LOGGING_PREFIX} Started handling event with id '{@event.Id}'");
            try
            {
                var productsIds = @event.Lines.Select(l => l.Product_Id);
                var products = await _context.Products.Where(p => productsIds.Contains(p.Id)).ToListAsync();
                foreach (var product in products)
                {
                    var line = @event.Lines.Single(l => l.Product_Id == product.Id);
                    product.Quantity -= line.Quantity;
                }

                await _context.SaveChangesAsync();
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