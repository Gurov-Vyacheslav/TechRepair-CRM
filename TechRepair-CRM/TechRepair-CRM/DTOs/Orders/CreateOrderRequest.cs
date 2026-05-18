using System.ComponentModel.DataAnnotations;

namespace TechRepair_CRM.DTOs.Orders;

public record CreateOrderRequest
{
    [Required(ErrorMessage = "Выберите устройство.")]
    [Display(Name = "Устройство")]
    public int? DeviceId { get; set; }

    [Required(ErrorMessage = "Описание проблемы обязательно.")]
    [Display(Name = "Описание проблемы")]
    public string ProblemDescription { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Предварительная стоимость не может быть отрицательной.")]
    [Display(Name = "Предварительная стоимость")]
    public decimal EstimatedCost { get; set; }

    [Range(0, short.MaxValue, ErrorMessage = "Количество месяцев гарантии не может быть отрицательным.")]
    [Display(Name = "Гарантия, месяцев")]
    public short? WarrantyMonths { get; set; }

    [Display(Name = "Гарантийный ремонт")]
    public bool IsWarrantyRepair { get; set; }

    [Display(Name = "Результат диагностики")]
    public string? DiagnosticResult { get; set; }

    [Display(Name = "Заметки")]
    public string? Notes { get; set; }
}