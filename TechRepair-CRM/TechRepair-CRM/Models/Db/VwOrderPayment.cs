using System;
using System.Collections.Generic;

namespace TechRepair_CRM.Models.Db;

public partial class VwOrderPayment
{
    public int? OrderId { get; set; }

    public string? OrderNumber { get; set; }

    public decimal? TotalCost { get; set; }

    public decimal? PaidAmount { get; set; }

    public decimal? RemainingAmount { get; set; }

    public long? PaymentsCount { get; set; }

    public DateTime? LastPaymentDate { get; set; }
}
