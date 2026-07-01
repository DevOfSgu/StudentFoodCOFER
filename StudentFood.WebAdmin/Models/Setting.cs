using System.ComponentModel.DataAnnotations;

namespace StudentFood.WebAdmin.Models
{
    public class Setting
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Key { get; set; } // VD: "default_delivery_fee"
        
        public string Value { get; set; } // VD: "3000"
    }
}