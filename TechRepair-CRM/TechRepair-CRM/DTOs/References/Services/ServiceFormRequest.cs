using System.ComponentModel.DataAnnotations;

namespace TechRepair_CRM.DTOs.References.Services;

public record ServiceFormRequest
{
    [Required(ErrorMessage = "Название услуги обязательно.")]
    [Display(Name = "Название услуги")]
    public string ServiceName { get; set; } = string.Empty;

    [Display(Name = "Описание")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Базовая цена обязательна.")]
    [Range(0, double.MaxValue, ErrorMessage = "Базовая цена не может быть отрицательной.")]
    [Display(Name = "Базовая цена")]
    public decimal BasePrice { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Оценочная длительность не может быть отрицательной.")]
    [Display(Name = "Оценочная длительность, минут")]
    public int? EstimatedDuration { get; set; }

    [Display(Name = "Активна")]
    public bool IsActive { get; set; } = true;
}