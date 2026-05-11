namespace TechRepair_CRM.DTOs.Orders;

public record OrderFilterRequest
{
    public string? OrderNumber { get; set; }

    public string? Status { get; set; }

    public string? ClientPhone { get; set; }

    public DateTime? CreatedFrom { get; set; }

    public DateTime? CreatedTo { get; set; }

    public int? TechnicianId { get; set; }

    public int? DeviceTypeId { get; set; }

    public bool? HasDebt { get; set; }
}