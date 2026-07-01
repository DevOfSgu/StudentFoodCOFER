using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentFood.WebAdmin.Models
{
    public class Canteen
    {
        [Key]
        public int Id { get; set; }

        public int OwnerId { get; set; }
        [ForeignKey("OwnerId")]
        public User Owner { get; set; }

        [Required]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public string Status { get; set; } = "open"; // "open", "closed"
        
        public double CommissionRate { get; set; } = 0.1; // VD: 10%

        // Navigation Properties
        public ICollection<Food> Foods { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}