using System.ComponentModel.DataAnnotations;

namespace StudentFood.WebAdmin.DTOs;

public class UserProfileDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class UpdateUserProfileRequest
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;
}