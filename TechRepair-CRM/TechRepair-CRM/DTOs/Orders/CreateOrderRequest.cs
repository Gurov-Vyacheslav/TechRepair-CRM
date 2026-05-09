using System.ComponentModel.DataAnnotations;

namespace TechRepair_CRM.DTOs.Orders;

public record CreateOrderRequest
{
    [Required]
    [Display(Name = "Устройство")]
    public int DeviceId { get; set; }

    [Required]
    [Display(Name = "Описание проблемы")]
    public string ProblemDescription { get; set; } = string.Empty;

    [Display(Name = "Предварительная стоимость")]
    public decimal EstimatedCost { get; set; }

    [Display(Name = "Гарантия, месяцев")]
    public short? WarrantyMonths { get; set; }

    [Display(Name = "Гарантийный ремонт")]
    public bool IsWarrantyRepair { get; set; }

    [Display(Name = "Результат диагностики")]
    public string? DiagnosticResult { get; set; }

    [Display(Name = "Заметки")]
    public string? Notes { get; set; }
}