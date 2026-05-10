using System.ComponentModel.DataAnnotations;

namespace TechRepair_CRM.DTOs.Devices;

public record DeviceFormRequest
{
    [Required]
    [Display(Name = "Тип устройства")]
    public int DeviceTypeId { get; set; }

    [Display(Name = "Бренд")]
    public string? Brand { get; set; }

    [Display(Name = "Модель")]
    public string? Model { get; set; }

    [Display(Name = "Серийный номер")]
    public string? SerialNumber { get; set; }

    [Display(Name = "Дата покупки")]
    public DateOnly? PurchaseDate { get; set; }

    [Display(Name = "Комплектация")]
    public string? EquipmentDescription { get; set; }

    [Display(Name = "Внешнее состояние")]
    public string? ExternalCondition { get; set; }

    [Display(Name = "Заметки")]
    public string? Notes { get; set; }
}
