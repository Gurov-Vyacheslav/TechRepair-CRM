namespace TechRepair_CRM.DTOs.References.Parts;

public record PartItemResponse(
    int PartId,
    string? PartNumber,
    string PartName,
    string? Manufacturer,
    decimal? DefaultPrice,
    bool IsActive
);