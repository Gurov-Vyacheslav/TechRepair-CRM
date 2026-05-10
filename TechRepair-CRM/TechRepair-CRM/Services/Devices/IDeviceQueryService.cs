using TechRepair_CRM.DTOs.Devices;

namespace TechRepair_CRM.Services.Devices;

public interface IDeviceQueryService
{
    Task<DeviceFormRequest?> GetDeviceFormAsync(int deviceId);
}