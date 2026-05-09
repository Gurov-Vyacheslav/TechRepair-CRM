using TechRepair_CRM.DTOs.Clients;
using TechRepair_CRM.DTOs.Devices;

namespace TechRepair_CRM.Services.Clients;

public interface IClientCommandService
{
    Task<int> CreateClientWithDeviceAsync(CreateClientWithDeviceRequest request);
    Task<int> AddDeviceToClientAsync(int clientId, AddDeviceRequest request);
}