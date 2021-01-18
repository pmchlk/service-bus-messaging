using System.ComponentModel.DataAnnotations.Schema;

namespace OrdersService.Domain.Models
{
    public class OrderLine
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long Product_Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
        
        [ForeignKey(nameof(Order))]
        public long Order_Id { get; set; }
        public virtual Order Order { get; set; }
    }
}