using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentFood.WebAdmin.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } // Email của người dùng
        
        [Required]
        public string Password { get; set; }
        
        [Required]
        public string FullName { get; set; }
        
        public string Role { get; set; } // "student", "canteen", "admin"
        
        public string PhoneNumber { get; set; }
        
        public string AvatarUrl { get; set; }
        
        public string Status { get; set; } = "active"; // "active", "banned"
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<Order> Orders { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Canteen> Canteens { get; set; }
    }
}