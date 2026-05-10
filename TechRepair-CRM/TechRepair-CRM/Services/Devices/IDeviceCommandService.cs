using TechRepair_CRM.DTOs.Devices;

namespace TechRepair_CRM.Services.Devices;

public partial interface IDeviceCommandService
{
    Task<int> UpdateDeviceAsync(int deviceId, DeviceFormRequest request);
}