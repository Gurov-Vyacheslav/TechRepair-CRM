namespace TechRepair_CRM.DTOs.References.Services;

public record ServiceItemResponse(
    int ServiceId,
    string ServiceName,
    decimal BasePrice,
    int? EstimatedDuration,
    bool IsActive
);