using System;
using System.Collections.Generic;

namespace TechRepair_CRM.Models.Db;

public partial class RepairOrder
{
    public int OrderId { get; set; }

    public string OrderNumber { get; set; } = null!;

    public int DeviceId { get; set; }

    public short StatusId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? IssuedAt { get; set; }

    public short? WarrantyMonths { get; set; }

    public string ProblemDescription { get; set; } = null!;

    public string? DiagnosticResult { get; set; }

    public decimal EstimatedCost { get; set; }

    public decimal TotalCost { get; set; }

    public bool IsWarrantyRepair { get; set; }

    public string? Notes { get; set; }

    public virtual Device Device { get; set; } = null!;

    public virtual ICollection<OrderPart> OrderParts { get; set; } = new List<OrderPart>();

    public virtual ICollection<OrderService> OrderServices { get; set; } = new List<OrderService>();

    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual OrderStatus Status { get; set; } = null!;
}
