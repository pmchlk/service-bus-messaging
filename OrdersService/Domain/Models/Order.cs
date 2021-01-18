using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using OrdersService.Domain.Enums;

namespace OrdersService.Domain.Models
{
    public class Order
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public OrderStatusEnum Status { get; set; }
        public virtual ICollection<OrderLine> Lines { get; set; }
    }
}