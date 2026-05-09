using System.ComponentModel.DataAnnotations;

namespace TechRepair_CRM.DTOs.Orders.Services;

public record AddOrderServiceRequest
{
    [Required]
    [Display(Name = "Услуга")]
    public int ServiceId { get; set; }

    [Required]
    [Range(1, short.MaxValue)]
    [Display(Name = "Количество")]
    public short Quantity { get; set; } = 1;

    [Display(Name = "Мастер")]
    public int? TechnicianId { get; set; }

    [Display(Name = "Заметки")]
    public string? Notes { get; set; }
}