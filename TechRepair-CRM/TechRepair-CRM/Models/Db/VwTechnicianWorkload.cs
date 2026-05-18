using System;
using System.Collections.Generic;

namespace TechRepair_CRM.Models.Db;

public partial class VwTechnicianWorkload
{
    public int? TechnicianId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Specialization { get; set; }

    public bool? IsActive { get; set; }

    public long? AssignedServiceRowsCount { get; set; }

    public long? AssignedServiceQuantity { get; set; }

    public long? CompletedServiceQuantity { get; set; }

    public decimal? AssignedServiceAmount { get; set; }

    public decimal? CompletedServiceAmount { get; set; }

    public DateTime? FirstCompletedAt { get; set; }

    public DateTime? LastCompletedAt { get; set; }
}
