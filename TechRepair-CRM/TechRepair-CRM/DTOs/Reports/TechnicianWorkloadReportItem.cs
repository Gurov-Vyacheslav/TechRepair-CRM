namespace TechRepair_CRM.DTOs.Reports;

public record TechnicianWorkloadReportItem(
    int TechnicianId,
    string FullName,
    string? Specialization,
    bool IsActive,
    long AssignedServiceRowsCount,
    long AssignedServiceQuantity,
    long CompletedServiceQuantity,
    decimal AssignedServiceAmount,
    decimal CompletedServiceAmount,
    DateTime? FirstCompletedAt,
    DateTime? LastCompletedAt
);