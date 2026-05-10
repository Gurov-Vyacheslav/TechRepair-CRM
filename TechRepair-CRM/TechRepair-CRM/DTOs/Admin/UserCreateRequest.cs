using System.ComponentModel.DataAnnotations;

namespace TechRepair_CRM.DTOs.Admin;

public record UserCreateRequest
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Роль")]
    public string Role { get; set; } = string.Empty;

    [Display(Name = "Мастер")]
    public int? TechnicianId { get; set; }
}