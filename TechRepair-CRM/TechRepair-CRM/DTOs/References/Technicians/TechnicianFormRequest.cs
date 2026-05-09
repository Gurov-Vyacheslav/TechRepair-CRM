using System.ComponentModel.DataAnnotations;

namespace TechRepair_CRM.DTOs.References.Technicians;

public record TechnicianFormRequest
{
    [Required]
    [Display(Name = "Имя")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Фамилия")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Телефон")]
    public string Phone { get; set; } = string.Empty;

    [Display(Name = "Специализация")]
    public string? Specialization { get; set; }

    [Display(Name = "Активен")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Заметки")]
    public string? Notes { get; set; }
}