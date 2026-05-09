namespace TechRepair_CRM.DTOs.Devices;

public record ClientDeviceResponse(
    int DeviceId,
    string DeviceType,
    string? Brand,
    string? Model,
    string? SerialNumber,
    DateOnly? PurchaseDate,
    string? EquipmentDescription,
    string? ExternalCondition,
    string? Notes
);