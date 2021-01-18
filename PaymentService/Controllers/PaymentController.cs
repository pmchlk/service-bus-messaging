using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Models.Requests;
using PaymentService.Domain.Enums;
using PaymentService.Infrastructure.Notifications.Publishers.Dispatchers;
using PaymentService.Persistence.Database;
using Shared.Persistence.Database.Meta;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("/api/payments")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentDbContext _context;
        private readonly PaymentCompletedEventDispatcher _paymentCompletedEventDispatcher;

        public PaymentController(IDatabaseContextProvider<PaymentDbContext> contextProvider, PaymentCompletedEventDispatcher paymentCompletedEventDispatcher)
        {
            _context = contextProvider.GetContext() ?? throw new ArgumentNullException(nameof(contextProvider));
            _paymentCompletedEventDispatcher = paymentCompletedEventDispatcher ?? throw new ArgumentNullException(nameof(paymentCompletedEventDispatcher));
        }

        [HttpPost("callback")]
        public async Task<IActionResult> PaymentProviderCallback([FromBody] PaymentProviderCallbackRequestDto request)
        {
            var payment = await _context.Payments.SingleOrDefaultAsync(p => p.Id == request.PaymentId);
            if (payment == null)
                return BadRequest("Invalid PaymentId");

            payment.Status = request.Success ? PaymentStatusEnum.COMPLETED : PaymentStatusEnum.REJECTED;
            await _context.SaveChangesAsync();
            if (payment.Status == PaymentStatusEnum.COMPLETED)
                await _paymentCompletedEventDispatcher.DispatchAsync(payment);

            return Ok();
        }
    }
}