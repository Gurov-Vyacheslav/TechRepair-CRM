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

    public long? AssignedServicesCount { get; set; }

    public long? CompletedServicesCount { get; set; }

    public long? TotalServiceQuantity { get; set; }

    public decimal? TotalServiceAmount { get; set; }

    public DateTime? FirstCompletedAt { get; set; }

    public DateTime? LastCompletedAt { get; set; }
}
