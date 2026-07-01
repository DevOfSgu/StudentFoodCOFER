using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentFood.WebAdmin.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public User Student { get; set; }

        public int CanteenId { get; set; }
        [ForeignKey("CanteenId")]
        public Canteen Canteen { get; set; }

        public string Status { get; set; } = "pending"; // pending, preparing, ready, delivered, cancelled
        
        public string DeliveryAddress { get; set; }
        
        public string PaymentMethod { get; set; } = "cash"; // cash, transfer
        
        public decimal Subtotal { get; set; }
        
        public decimal DeliveryFee { get; set; }
        
        public decimal CommissionAmount { get; set; }
        
        public decimal TotalPrice { get; set; }
        
        public string CancelReason { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<OrderItem> OrderItems { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}