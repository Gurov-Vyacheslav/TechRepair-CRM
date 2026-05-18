using System.ComponentModel.DataAnnotations;

namespace TechRepair_CRM.DTOs.References.Technicians;

public record TechnicianFormRequest
{
    [Required(ErrorMessage = "Имя обязательно.")]
    [Display(Name = "Имя")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Фамилия обязательна.")]
    [Display(Name = "Фамилия")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email обязателен.")]
    [EmailAddress(ErrorMessage = "Введите корректный email.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Телефон обязателен.")]
    [Display(Name = "Телефон")]
    public string Phone { get; set; } = string.Empty;

    [Display(Name = "Специализация")]
    public string? Specialization { get; set; }

    [Display(Name = "Активен")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Заметки")]
    public string? Notes { get; set; }
}