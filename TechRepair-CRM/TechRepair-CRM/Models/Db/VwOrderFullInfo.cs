using System;
using System.Collections.Generic;

namespace TechRepair_CRM.Models.Db;

public partial class VwOrderFullInfo
{
    public int? OrderId { get; set; }

    public string? OrderNumber { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? IssuedAt { get; set; }

    public string? ProblemDescription { get; set; }

    public string? DiagnosticResult { get; set; }

    public decimal? EstimatedCost { get; set; }

    public decimal? TotalCost { get; set; }

    public bool? IsWarrantyRepair { get; set; }

    public short? WarrantyMonths { get; set; }

    public string? OrderNotes { get; set; }

    public string? OrderStatus { get; set; }

    public int? ClientId { get; set; }

    public string? ClientFirstName { get; set; }

    public string? ClientLastName { get; set; }

    public string? ClientPhone { get; set; }

    public string? ClientEmail { get; set; }

    public int? DeviceId { get; set; }

    public string? DeviceType { get; set; }

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? SerialNumber { get; set; }

    public string? EquipmentDescription { get; set; }

    public string? ExternalCondition { get; set; }

    public decimal? ServiceSum { get; set; }

    public decimal? PartSum { get; set; }

    public decimal? CalculatedTotal { get; set; }

    public decimal? PaidAmount { get; set; }

    public decimal? RemainingAmount { get; set; }

    public long? PaymentsCount { get; set; }
}
