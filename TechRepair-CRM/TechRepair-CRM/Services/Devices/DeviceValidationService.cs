using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;

namespace TechRepair_CRM.Services.Devices;

public class DeviceValidationService : IDeviceValidationService
{
    private readonly RepairServiceDbContext _db;

    public DeviceValidationService(RepairServiceDbContext db)
    {
        _db = db;
    }

    public async Task EnsureSerialNumberIsUniqueAsync(string? serialNumber, int exceptDeviceId = 0)
    {
        var normalized = NormalizeNullable(serialNumber);
        if (normalized is null) return;

        var exists = await _db.Devices
            .AnyAsync(d => d.SerialNumber == normalized && d.DeviceId != exceptDeviceId);
        if (exists)
            throw new InvalidOperationException("Устройство с таким серийным номером уже существует.");
    }

    public string? NormalizeNullable(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}