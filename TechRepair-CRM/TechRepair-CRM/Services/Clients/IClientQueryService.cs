using TechRepair_CRM.DTOs.Clients;

namespace TechRepair_CRM.Services.Clients;

public interface IClientQueryService
{
    Task<List<ClientListItemResponse>> GetClientsAsync();

    Task<ClientDetailsResponse?> GetClientDetailsAsync(int clientId);
}