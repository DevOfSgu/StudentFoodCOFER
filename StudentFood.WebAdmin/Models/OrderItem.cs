using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentFood.WebAdmin.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        public int FoodId { get; set; }
        [ForeignKey("FoodId")]
        public Food Food { get; set; }

        public int Quantity { get; set; }
        
        public decimal PriceAtOrder { get; set; } // Lưu lại giá tại thời điểm đặt

        public string Note { get; set; } = string.Empty;
    }
}