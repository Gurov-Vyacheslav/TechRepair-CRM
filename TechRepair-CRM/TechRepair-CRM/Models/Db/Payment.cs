using System;
using System.Collections.Generic;

namespace TechRepair_CRM.Models.Db;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int OrderId { get; set; }

    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string? TransactionNumber { get; set; }

    public string? Notes { get; set; }

    public virtual RepairOrder Order { get; set; } = null!;
}
