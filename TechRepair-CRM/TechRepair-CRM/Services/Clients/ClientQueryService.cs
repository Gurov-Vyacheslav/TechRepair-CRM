using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;
using TechRepair_CRM.DTOs.Clients;
using TechRepair_CRM.DTOs.Devices;
using TechRepair_CRM.DTOs.Orders;

namespace TechRepair_CRM.Services.Clients;

public class ClientQueryService : IClientQueryService
{
    private readonly RepairServiceDbContext _db;

    public ClientQueryService(RepairServiceDbContext db)
    {
        _db = db;
    }

    public async Task<List<ClientListItemResponse>> GetClientsAsync()
    {
        return await _db.Clients
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .Select(c => new ClientListItemResponse(
                c.ClientId,
                c.FirstName,
                c.LastName,
                c.Phone,
                c.Email,
                c.RegistrationDate
            ))
            .ToListAsync();
    }

    public async Task<ClientDetailsResponse?> GetClientDetailsAsync(int clientId)
    {
        var client = await GetClientMainInfoAsync(clientId);

        if (client is null)
            return null;

        var devices = await GetDevicesAsync(clientId);

        var orders = await GetOrdersAsync(clientId);

        return client with { Devices = devices, Orders = orders };
    }

    private async Task<ClientDetailsResponse?> GetClientMainInfoAsync(int clientId)
    {
        return await _db.Clients
            .Where(c => c.ClientId == clientId)
            .Select(c => new ClientDetailsResponse
            (
                c.ClientId,
                c.FirstName,
                c.LastName,
                c.Phone,
                c.Email,
                c.Address,
                c.RegistrationDate,
                c.Notes,
                new List<ClientDeviceResponse>(),
                new List<OrderListItemResponse>()
            ))
            .FirstOrDefaultAsync();
    }

    private async Task<List<ClientDeviceResponse>> GetDevicesAsync(int clientId)
    {
        return await (
            from d in _db.Devices
            join dt in _db.DeviceTypes on d.DeviceTypeId equals dt.DeviceTypeId
            where d.ClientId == clientId
            orderby d.DeviceId
            select new ClientDeviceResponse(
                d.DeviceId,
                dt.TypeName,
                d.Brand,
                d.Model,
                d.SerialNumber,
                d.PurchaseDate,
                d.EquipmentDescription,
                d.ExternalCondition,
                d.Notes
            )
        ).ToListAsync();
    }

    private async Task<List<OrderListItemResponse>> GetOrdersAsync(int clientId)
    {
        return await _db.VwOrderFullInfos
            .Where(o => o.ClientId == clientId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderListItemResponse(
                o.OrderId.Value,
                o.OrderNumber,
                o.CreatedAt.Value,
                o.OrderStatus,
                o.ClientFirstName + " " + o.ClientLastName,
                o.ClientPhone,
                o.DeviceType,
                o.Brand,
                o.Model,
                o.TotalCost.Value,
                o.PaidAmount.Value,
                o.RemainingAmount.Value
            ))
            .ToListAsync();
    }
}