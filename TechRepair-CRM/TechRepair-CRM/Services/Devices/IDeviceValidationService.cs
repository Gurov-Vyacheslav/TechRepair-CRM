namespace TechRepair_CRM.Services.Devices;

public interface IDeviceValidationService
{
    Task EnsureSerialNumberIsUniqueAsync(string? serialNumber, int exceptDeviceId = 0);
    string? NormalizeNullable(string? value);
}