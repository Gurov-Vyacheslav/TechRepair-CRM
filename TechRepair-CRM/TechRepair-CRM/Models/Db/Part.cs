using System;
using System.Collections.Generic;

namespace TechRepair_CRM.Models.Db;

public partial class Part
{
    public int PartId { get; set; }

    public string? PartNumber { get; set; }

    public string PartName { get; set; } = null!;

    public string? Manufacturer { get; set; }

    public decimal? DefaultPrice { get; set; }

    public bool IsActive { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<OrderPart> OrderParts { get; set; } = new List<OrderPart>();
}
