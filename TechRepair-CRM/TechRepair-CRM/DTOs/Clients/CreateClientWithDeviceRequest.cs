using System.ComponentModel.DataAnnotations;

namespace TechRepair_CRM.DTOs.Clients;

public record CreateClientWithDeviceRequest
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

    [Required]
    [Display(Name = "Тип устройства")]
    public int DeviceTypeId { get; set; }

    [Display(Name = "Бренд")]
    public string? Brand { get; set; }

    [Display(Name = "Модель")]
    public string? Model { get; set; }

    [Display(Name = "Серийный номер")]
    public string? SerialNumber { get; set; }

    [Display(Name = "Комплектация")]
    public string? EquipmentDescription { get; set; }

    [Display(Name = "Внешнее состояние")]
    public string? ExternalCondition { get; set; }

    [Display(Name = "Заметки")]
    public string? Notes { get; set; }
}