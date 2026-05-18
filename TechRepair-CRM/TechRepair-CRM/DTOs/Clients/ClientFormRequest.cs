using System.ComponentModel.DataAnnotations;

namespace TechRepair_CRM.DTOs.Clients;

public record ClientFormRequest
{
    [Required(ErrorMessage = "Имя обязательно.")]
    [Display(Name = "Имя")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Фамилия обязательна.")]
    [Display(Name = "Фамилия")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Телефон обязателен.")]
    [Display(Name = "Телефон")]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Введите корректный email.")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Адрес")]
    public string? Address { get; set; }

    [Display(Name = "Заметки")]
    public string? Notes { get; set; }
}