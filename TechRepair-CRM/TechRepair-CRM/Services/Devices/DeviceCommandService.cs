using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;
using TechRepair_CRM.DTOs.Devices;
using TechRepair_CRM.Services.Entity;

namespace TechRepair_CRM.Services.Devices;

public class DeviceCommandService : IDeviceCommandService
{
    private readonly RepairServiceDbContext _db;
    private readonly IEntityValidationService _entityValidationService;
    private readonly IDeviceValidationService _deviceValidationService;

    public DeviceCommandService(
        RepairServiceDbContext db,
        IEntityValidationService entityValidationService,
        IDeviceValidationService deviceValidationService)
    {
        _db = db;
        _entityValidationService = entityValidationService;
        _deviceValidationService = deviceValidationService;
    }

    public async Task<int> UpdateDeviceAsync(int deviceId, DeviceFormRequest request)
    {
        if (request.DeviceTypeId is null)
            throw new InvalidOperationException("Выберите тип устройства.");

        var device = await _entityValidationService.GetDeviceOrThrowAsync(deviceId);

        await _entityValidationService.EnsureDeviceTypeExistsAsync(request.DeviceTypeId.Value);
        await _deviceValidationService.EnsureSerialNumberIsUniqueAsync(request.SerialNumber, deviceId);

        device.DeviceTypeId = request.DeviceTypeId.Value;
        device.Brand = request.Brand;
        device.Model = request.Model;
        device.SerialNumber = _deviceValidationService.NormalizeNullable(request.SerialNumber);
        device.PurchaseDate = request.PurchaseDate;
        device.EquipmentDescription = request.EquipmentDescription;
        device.ExternalCondition = request.ExternalCondition;
        device.Notes = request.Notes;

        await _db.SaveChangesAsync();

        return device.ClientId;
    }
}
