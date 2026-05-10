using System.ComponentModel.DataAnnotations;

namespace TechRepair_CRM.DTOs.Orders;

public record OrderEditRequest
{
    [Required]
    [Display(Name = "Описание проблемы")]
    public string ProblemDescription { get; set; } = string.Empty;

    [Display(Name = "Результат диагностики")]
    public string? DiagnosticResult { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Предварительная стоимость")]
    public decimal EstimatedCost { get; set; }

    [Range(0, short.MaxValue)]
    [Display(Name = "Гарантия, месяцев")]
    public short? WarrantyMonths { get; set; }

    [Display(Name = "Гарантийный ремонт")]
    public bool IsWarrantyRepair { get; set; }

    [Display(Name = "Заметки")]
    public string? Notes { get; set; }
}