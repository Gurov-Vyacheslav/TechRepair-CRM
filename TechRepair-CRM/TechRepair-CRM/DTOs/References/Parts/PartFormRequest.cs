using System.ComponentModel.DataAnnotations;

namespace TechRepair_CRM.DTOs.References.Parts;

public record PartFormRequest
{
    [Display(Name = "Артикул")]
    public string? PartNumber { get; set; }

    [Required(ErrorMessage = "Название детали обязательно.")]
    [Display(Name = "Название детали")]
    public string PartName { get; set; } = string.Empty;

    [Display(Name = "Производитель")]
    public string? Manufacturer { get; set; }

    [Required(ErrorMessage = "Текущая цена обязательна.")]
    [Range(0, double.MaxValue, ErrorMessage = "Текущая цена не может быть отрицательной.")]
    [Display(Name = "Текущая цена")]
    public decimal DefaultPrice { get; set; }

    [Display(Name = "Активна")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Описание")]
    public string? Description { get; set; }
}