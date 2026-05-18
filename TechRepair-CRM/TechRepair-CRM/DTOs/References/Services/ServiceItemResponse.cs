namespace TechRepair_CRM.DTOs.References.Services;

public record ServiceItemResponse(
    int ServiceId,
    string ServiceName,
    string? Description,
    decimal BasePrice,
    int? EstimatedDuration,
    bool IsActive
);