using System;
using System.Collections.Generic;

namespace TechRepair_CRM.Models.Db;

public partial class VwServiceStatistic
{
    public int? ServiceId { get; set; }

    public string? ServiceName { get; set; }

    public bool? IsActive { get; set; }

    public long? UsageCount { get; set; }

    public long? TotalQuantity { get; set; }

    public decimal? TotalRevenue { get; set; }

    public decimal? AvgPriceAtMoment { get; set; }
}
