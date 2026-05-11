using System.ComponentModel.DataAnnotations;

namespace TechRepair_CRM.DTOs.Orders.Parts;

public record EditOrderPartRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть больше 0")]
    [Display(Name = "Количество")]
    public int Quantity { get; set; } = 1;
}