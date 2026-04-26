using System;
using System.Collections.Generic;

namespace TechRepair_CRM.Models.Db;

public partial class VwClientOrderHistory
{
    public int? ClientId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public int? DeviceId { get; set; }

    public string? DeviceType { get; set; }

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? SerialNumber { get; set; }

    public int? OrderId { get; set; }

    public string? OrderNumber { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? IssuedAt { get; set; }

    public string? OrderStatus { get; set; }

    public string? ProblemDescription { get; set; }

    public decimal? TotalCost { get; set; }

    public decimal? PaidAmount { get; set; }

    public decimal? RemainingAmount { get; set; }
}
