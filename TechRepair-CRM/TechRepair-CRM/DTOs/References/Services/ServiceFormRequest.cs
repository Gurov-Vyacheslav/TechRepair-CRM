using System.ComponentModel.DataAnnotations;

namespace TechRepair_CRM.DTOs.References.Services;

public record ServiceFormRequest
{
    [Required]
    [Display(Name = "Название услуги")]
    public string ServiceName { get; set; } = string.Empty;

    [Display(Name = "Описание")]
    public string? Description { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    [Display(Name = "Базовая цена")]
    public decimal BasePrice { get; set; }

    [Display(Name = "Оценочная длительность, минут")]
    public int? EstimatedDuration { get; set; }

    [Display(Name = "Активна")]
    public bool IsActive { get; set; } = true;
}