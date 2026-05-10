using System.ComponentModel.DataAnnotations;

namespace TechRepair_CRM.DTOs.Admin;

public record UserEditRequest
{
    public string Id { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Роль")]
    public string Role { get; set; } = string.Empty;

    [Display(Name = "Мастер")]
    public int? TechnicianId { get; set; }

    [Display(Name = "Активен")]
    public bool IsActive { get; set; } = true;
}