using System;
using System.Collections.Generic;

namespace TechRepair_CRM.Models.Db;

public partial class OrderService
{
    public int OrderId { get; set; }

    public int ServiceId { get; set; }

    public int? TechnicianId { get; set; }

    public short Quantity { get; set; }

    public decimal PriceAtMoment { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? Notes { get; set; }

    public virtual RepairOrder Order { get; set; } = null!;

    public virtual ICollection<OrderServicePart> OrderServiceParts { get; set; } = new List<OrderServicePart>();

    public virtual Service Service { get; set; } = null!;

    public virtual Technician? Technician { get; set; }
}
