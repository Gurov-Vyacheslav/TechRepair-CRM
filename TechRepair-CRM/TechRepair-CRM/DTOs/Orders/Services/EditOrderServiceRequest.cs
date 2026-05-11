using System.ComponentModel.DataAnnotations;

namespace TechRepair_CRM.DTOs.Orders.Services;

public record EditOrderServiceRequest
{
    [Required]
    [Range(1, short.MaxValue, ErrorMessage = "Количество должно быть больше 0")]
    [Display(Name = "Количество")]
    public short Quantity { get; set; } = 1;

    [Display(Name = "Мастер")]
    public int? TechnicianId { get; set; }

    [Display(Name = "Заметки")]
    public string? Notes { get; set; }
}