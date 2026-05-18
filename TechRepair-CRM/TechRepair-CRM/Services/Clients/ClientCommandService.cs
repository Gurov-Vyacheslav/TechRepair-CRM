using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;
using TechRepair_CRM.DTOs.Clients;
using TechRepair_CRM.DTOs.Devices;

using TechRepair_CRM.Models.Db;
using TechRepair_CRM.Services.Devices;
using TechRepair_CRM.Services.Entity;

namespace TechRepair_CRM.Services.Clients;

public class ClientCommandService : IClientCommandService
{
    private readonly RepairServiceDbContext _db;
    private readonly IEntityValidationService _entityValidationService;
    private readonly IDeviceValidationService _deviceValidationService;

    public ClientCommandService(
        RepairServiceDbContext db,
        IEntityValidationService entityValidationService,
        IDeviceValidationService deviceValidationService)

    {
        _db = db;
        _entityValidationService = entityValidationService;
        _deviceValidationService = deviceValidationService;
    }

    public async Task<int> CreateClientWithDeviceAsync(CreateClientWithDeviceRequest request)
    {
        if (request.DeviceTypeId is null)
            throw new InvalidOperationException("Выберите тип устройства.");

        await _entityValidationService.EnsureDeviceTypeExistsAsync(request.DeviceTypeId.Value);
        await EnsureClientPhoneIsUniqueAsync(request.Phone);
        await _deviceValidationService.EnsureSerialNumberIsUniqueAsync(request.SerialNumber);

        var client = new Client
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone.Trim(),
            Email = request.Email,
            Address = request.Address,
            RegistrationDate = DateTime.Now,
            Notes = request.Notes
        };

        var device = new Device
        {
            Client = client,
            DeviceTypeId = request.DeviceTypeId.Value,
            Brand = request.Brand,
            Model = request.Model,
            SerialNumber = _deviceValidationService.NormalizeNullable(request.SerialNumber),
            EquipmentDescription = request.EquipmentDescription,
            ExternalCondition = request.ExternalCondition,
            Notes = request.Notes
        };

        _db.Clients.Add(client);
        _db.Devices.Add(device);

        await _db.SaveChangesAsync();

        return device.DeviceId;
    }
    
    private async Task EnsureClientPhoneIsUniqueAsync(string phone, int exceptClientId = 0)
    {
        var normalizedPhone = phone.Trim();

        var exists = await _db.Clients
            .AnyAsync(c =>
                c.Phone == normalizedPhone &&
                c.ClientId != exceptClientId);

        if (exists)
            throw new InvalidOperationException("Клиент с таким телефоном уже существует.");
    }
    
    public async Task UpdateClientAsync(int clientId, ClientFormRequest request)
    {
        var client = await _entityValidationService.GetClientOrThrowAsync(clientId);
        await EnsureClientPhoneIsUniqueAsync(request.Phone, clientId);

        client.FirstName = request.FirstName;
        client.LastName = request.LastName;
        client.Phone = request.Phone;
        client.Email = request.Email;
        client.Address = request.Address;
        client.Notes = request.Notes;

        await _db.SaveChangesAsync();
    }

    public async Task<int> AddDeviceToClientAsync(int clientId, AddDeviceRequest request)
    {
        if (request.DeviceTypeId is null)
            throw new InvalidOperationException("Выберите тип устройства.");

        await _entityValidationService.EnsureClientExistsAsync(clientId);
        await _entityValidationService.EnsureDeviceTypeExistsAsync(request.DeviceTypeId.Value);
        await _deviceValidationService.EnsureSerialNumberIsUniqueAsync(request.SerialNumber);

        var device = new Device
        {
            ClientId = clientId,
            DeviceTypeId = request.DeviceTypeId.Value,
            Brand = request.Brand,
            Model = request.Model,
            SerialNumber = _deviceValidationService.NormalizeNullable(request.SerialNumber),
            PurchaseDate = request.PurchaseDate,
            EquipmentDescription = request.EquipmentDescription,
            ExternalCondition = request.ExternalCondition,
            Notes = request.Notes
        };

        _db.Devices.Add(device);
        await _db.SaveChangesAsync();

        return device.DeviceId;
    }

}