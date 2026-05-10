using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;
using TechRepair_CRM.DTOs.Clients;
using TechRepair_CRM.DTOs.Devices;
using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.Models.Db;

namespace TechRepair_CRM.Services.Clients;

public class ClientQueryService : IClientQueryService
{
    private readonly RepairServiceDbContext _db;

    public ClientQueryService(RepairServiceDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ClientListItemResponse>> GetClientsAsync(ClientFilterRequest? filter = null)
    {
        var query = GetFilteredClients(filter);

        return await query
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

    private IQueryable<Client> GetFilteredClients(ClientFilterRequest? filter)
    {
        var clients = _db.Clients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter?.FullName))
        {
            var fullName = filter.FullName.Trim().ToLower();

            clients = clients.Where(c =>
                (c.LastName + " " + c.FirstName).ToLower().Contains(fullName) ||
                (c.FirstName + " " + c.LastName).ToLower().Contains(fullName));
        }

        if (!string.IsNullOrWhiteSpace(filter?.Phone))
        {
            var phone = filter.Phone.Trim();

            clients = clients.Where(c => c.Phone.Contains(phone));
        }

        if (!string.IsNullOrWhiteSpace(filter?.Email))
        {
            var email = filter.Email.Trim().ToLower();

            clients = clients.Where(c => c.Email != null && c.Email.ToLower().Contains(email));
        }
        
        return clients;
    }
    
    public async Task<ClientFormRequest?> GetClientFormAsync(int clientId)
    {
        return await _db.Clients
            .Where(c => c.ClientId == clientId)
            .Select(c => new ClientFormRequest
            {
                FirstName = c.FirstName,
                LastName = c.LastName,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                Notes = c.Notes
            })
            .FirstOrDefaultAsync();
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

    private async Task<IReadOnlyList<ClientDeviceResponse>> GetDevicesAsync(int clientId)
    {
        var devices = await _db.Devices
            .Where(d => d.ClientId == clientId)
            .Select(d => new
            {
                d.DeviceId,
                DeviceType = d.DeviceType.TypeName,
                d.Brand,
                d.Model,
                d.SerialNumber,
                d.PurchaseDate,
                d.EquipmentDescription,
                d.ExternalCondition,
                d.Notes,
       
                LatestOrder = d.RepairOrders
                    .OrderByDescending(o => o.CreatedAt)
                    .Select(o => new { o.OrderId, o.OrderNumber })
                    .FirstOrDefault()
            })
            .OrderBy(x => x.DeviceId)
            .ToListAsync();

        return devices
            .Select(d => new ClientDeviceResponse(
                d.DeviceId,
                d.DeviceType,
                d.Brand,
                d.Model,
                d.SerialNumber,
                d.PurchaseDate,
                d.EquipmentDescription,
                d.ExternalCondition,
                d.Notes,
                d.LatestOrder?.OrderId,
                d.LatestOrder?.OrderNumber
            ))
            .ToList();
    }

    private async Task<IReadOnlyList<OrderListItemResponse>> GetOrdersAsync(int clientId)
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