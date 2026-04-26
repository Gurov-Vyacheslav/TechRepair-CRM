using System;
using System.Collections.Generic;

namespace TechRepair_CRM.Models.Db;

public partial class VwOrderCostBreakdown
{
    public int? OrderId { get; set; }

    public string? OrderNumber { get; set; }

    public decimal? ServiceSum { get; set; }

    public decimal? PartSum { get; set; }

    public decimal? CalculatedTotal { get; set; }

    public decimal? StoredTotal { get; set; }

    public decimal? Difference { get; set; }
}
