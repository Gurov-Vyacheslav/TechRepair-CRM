namespace TechRepair_CRM.DTOs.Reports;

public record TechnicianWorkloadReportItem(
    int TechnicianId,
    string FirstName,
    string LastName,
    string? Specialization,
    bool IsActive,
    long AssignedServicesCount,
    long CompletedServicesCount,
    long TotalServiceQuantity,
    decimal TotalServiceAmount
);