using System;
using System.Collections.Generic;

namespace TechRepair_CRM.Models.Db;

public partial class VwRepairDuration
{
    public int? OrderId { get; set; }

    public string? OrderNumber { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? IssuedAt { get; set; }

    public string? OrderStatus { get; set; }

    public TimeSpan? RepairDurationInterval { get; set; }

    public decimal? RepairDurationHours { get; set; }

    public decimal? RepairDurationDays { get; set; }
}
