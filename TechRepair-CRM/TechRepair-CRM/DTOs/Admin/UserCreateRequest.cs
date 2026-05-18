using System.ComponentModel.DataAnnotations;

namespace TechRepair_CRM.DTOs.Admin;

public record UserCreateRequest
{
    [Required(ErrorMessage = "Email обязателен.")]
    [EmailAddress(ErrorMessage = "Введите корректный email.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Пароль обязателен.")]
    [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов.")]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите роль.")]
    [Display(Name = "Роль")]
    public string Role { get; set; } = string.Empty;

    [Display(Name = "Мастер")]
    public int? TechnicianId { get; set; }
}