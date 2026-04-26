using System;
using System.Collections.Generic;

namespace TechRepair_CRM.Models.Db;

public partial class Service
{
    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal BasePrice { get; set; }

    public int? EstimatedDuration { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<OrderService> OrderServices { get; set; } = new List<OrderService>();
}
