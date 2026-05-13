using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;
using TechRepair_CRM.DTOs.Devices;
using TechRepair_CRM.Models.Db;
using TechRepair_CRM.Services.Entity;

namespace TechRepair_CRM.Services.Devices;

public class DeviceCommandService : IDeviceCommandService
{
    private readonly RepairServiceDbContext _db;
    private readonly IEntityValidationService _entityValidationService;

    public DeviceCommandService(
        RepairServiceDbContext db,
        IEntityValidationService entityValidationService)
    {
        _db = db;
        _entityValidationService = entityValidationService;
    }

    public async Task<int> UpdateDeviceAsync(int deviceId, DeviceFormRequest request)
    {
        var device = await _entityValidationService.GetDeviceOrThrowAsync(deviceId);
        
        await _entityValidationService.EnsureDeviceTypeExistsAsync(request.DeviceTypeId);
        
        device.DeviceTypeId = request.DeviceTypeId;
        device.Brand = request.Brand;
        device.Model = request.Model;
        device.SerialNumber = request.SerialNumber;
        device.PurchaseDate = request.PurchaseDate;
        device.EquipmentDescription = request.EquipmentDescription;
        device.ExternalCondition = request.ExternalCondition;
        device.Notes = request.Notes;

        await _db.SaveChangesAsync();

        return device.ClientId;
    }
}