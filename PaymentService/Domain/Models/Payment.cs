using System;
using System.ComponentModel.DataAnnotations.Schema;
using PaymentService.Domain.Enums;

namespace PaymentService.Domain.Models
{
    public class Payment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long OrderId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public PaymentStatusEnum Status { get; set; }
    }
}