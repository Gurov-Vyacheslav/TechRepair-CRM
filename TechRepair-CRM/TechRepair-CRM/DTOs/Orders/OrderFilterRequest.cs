using System.ComponentModel.DataAnnotations;

namespace TechRepair_CRM.DTOs.Orders;

public record OrderFilterRequest
{
    [Display(Name = "Номер заказа")]
    public string? OrderNumber { get; set; }

    [Display(Name = "Статус")]
    public string? Status { get; set; }

    [Display(Name = "Телефон клиента")]
    public string? ClientPhone { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Дата от")]
    public DateTime? CreatedFrom { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Дата до")]
    public DateTime? CreatedTo { get; set; }
}