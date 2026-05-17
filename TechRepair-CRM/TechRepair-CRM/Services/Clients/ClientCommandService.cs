using TechRepair_CRM.Data;
using TechRepair_CRM.DTOs.Clients;
using TechRepair_CRM.DTOs.Devices;

using TechRepair_CRM.Models.Db;
using TechRepair_CRM.Services.Entity;

namespace TechRepair_CRM.Services.Clients;

public class ClientCommandService : IClientCommandService
{
    private readonly RepairServiceDbContext _db;
    private readonly IEntityValidationService _entityValidationService;

    public ClientCommandService(
        RepairServiceDbContext db,
        IEntityValidationService entityValidationService)

    {
        _db = db;
        _entityValidationService = entityValidationService;
    }

    public async Task<int> CreateClientWithDeviceAsync(CreateClientWithDeviceRequest request)
    {
        await _entityValidationService.EnsureDeviceTypeExistsAsync(request.DeviceTypeId);

        var client = new Client
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            RegistrationDate = DateTime.Now,
            Notes = request.Notes
        };

        var device = new Device
        {
            Client = client,
            DeviceTypeId = request.DeviceTypeId,
            Brand = request.Brand,
            Model = request.Model,
            SerialNumber = request.SerialNumber,
            EquipmentDescription = request.EquipmentDescription,
            ExternalCondition = request.ExternalCondition,
            Notes = request.Notes
        };

        _db.Clients.Add(client);
        _db.Devices.Add(device);

        await _db.SaveChangesAsync();

        return device.DeviceId;
    }
    
    public async Task UpdateClientAsync(int clientId, ClientFormRequest request)
    {
        var client = await _entityValidationService.GetClientOrThrowAsync(clientId);
        
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
        await _entityValidationService.EnsureClientExistsAsync(clientId);

        await _entityValidationService.EnsureDeviceTypeExistsAsync(request.DeviceTypeId);
        
        var device = new Device
        {
            ClientId = clientId,
            DeviceTypeId = request.DeviceTypeId,
            Brand = request.Brand,
            Model = request.Model,
            SerialNumber = request.SerialNumber,
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