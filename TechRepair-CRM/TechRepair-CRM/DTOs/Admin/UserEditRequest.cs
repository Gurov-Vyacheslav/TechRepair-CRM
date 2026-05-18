using System.ComponentModel.DataAnnotations;

namespace TechRepair_CRM.DTOs.Admin;

public record UserEditRequest
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email обязателен.")]
    [EmailAddress(ErrorMessage = "Введите корректный email.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите роль.")]
    [Display(Name = "Роль")]
    public string Role { get; set; } = string.Empty;

    [Display(Name = "Мастер")]
    public int? TechnicianId { get; set; }

    [Display(Name = "Заблокирован")]
    public bool IsBlocked { get; set; }
}