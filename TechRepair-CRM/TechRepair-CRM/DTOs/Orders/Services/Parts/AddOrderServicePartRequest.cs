using System.ComponentModel.DataAnnotations;

namespace TechRepair_CRM.DTOs.Orders.Services.Parts;

public record AddOrderServicePartRequest
{
    [Required(ErrorMessage = "Выберите деталь.")]
    [Display(Name = "Деталь")]
    public int? PartId { get; set; }

    [Required(ErrorMessage = "Количество обязательно.")]
    [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть больше 0.")]
    [Display(Name = "Количество")]
    public int Quantity { get; set; } = 1;
}