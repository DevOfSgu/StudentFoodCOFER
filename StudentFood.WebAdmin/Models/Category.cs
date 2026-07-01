using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentFood.WebAdmin.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } // Cơm, Mì, Nước uống...
        
        public string IconUrl { get; set; }

        // Navigation Properties
        public ICollection<Food> Foods { get; set; }
    }
}