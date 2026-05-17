using System;
using System.Collections.Generic;

namespace TechRepair_CRM.Models.Db;

public partial class VwOrderServicePartDetail
{
    public int? OrderId { get; set; }

    public string? OrderNumber { get; set; }

    public int? ServiceId { get; set; }

    public string? ServiceName { get; set; }

    public int? PartId { get; set; }

    public string? PartNumber { get; set; }

    public string? PartName { get; set; }

    public string? Manufacturer { get; set; }

    public int? Quantity { get; set; }

    public decimal? PriceAtMoment { get; set; }

    public decimal? TotalAmount { get; set; }
}
