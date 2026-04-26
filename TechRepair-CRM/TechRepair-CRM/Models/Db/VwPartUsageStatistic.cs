using System;
using System.Collections.Generic;

namespace TechRepair_CRM.Models.Db;

public partial class VwPartUsageStatistic
{
    public int? PartId { get; set; }

    public string? PartNumber { get; set; }

    public string? PartName { get; set; }

    public string? Manufacturer { get; set; }

    public bool? IsActive { get; set; }

    public long? UsageCount { get; set; }

    public long? TotalQuantity { get; set; }

    public decimal? TotalAmount { get; set; }
}
