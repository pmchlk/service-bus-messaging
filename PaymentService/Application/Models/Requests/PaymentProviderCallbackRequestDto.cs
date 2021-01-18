namespace PaymentService.Application.Models.Requests
{
    public class PaymentProviderCallbackRequestDto
    {
        public long PaymentId { get; set; }
        public bool Success { get; set; }
    }
}