using System.ComponentModel.DataAnnotations;

namespace TechRepair_CRM.DTOs.Clients;

public record ClientFormRequest
{
    [Required]
    [Display(Name = "Имя")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Фамилия")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Телефон")]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Адрес")]
    public string? Address { get; set; }

    [Display(Name = "Заметки")]
    public string? Notes { get; set; }
}