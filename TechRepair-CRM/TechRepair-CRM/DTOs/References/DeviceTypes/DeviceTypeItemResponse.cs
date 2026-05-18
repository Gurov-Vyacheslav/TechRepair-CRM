using System.ComponentModel.DataAnnotations;

namespace TechRepair_CRM.DTOs.References.DeviceTypes;

public record DeviceTypeFormRequest
{
    [Required(ErrorMessage = "Название типа устройства обязательно")]
    [StringLength(100, ErrorMessage = "Название типа устройства не должно быть длиннее 100 символов")]
    [Display(Name = "Название типа устройства")]
    public string TypeName { get; set; } = string.Empty;
}