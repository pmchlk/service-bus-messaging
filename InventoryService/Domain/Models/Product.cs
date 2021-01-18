using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryService.Domain.Models
{
    public class Product
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}