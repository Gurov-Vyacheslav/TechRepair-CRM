using System.ComponentModel.DataAnnotations;

namespace TechRepair_CRM.DTOs.Orders.Services.Parts;

public record AddOrderServicePartRequest
{
    [Required]
    [Display(Name = "Деталь")]
    public int PartId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    [Display(Name = "Количество")]
    public int Quantity { get; set; } = 1;
}