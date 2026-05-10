using TechRepair_CRM.DTOs.Clients;

namespace TechRepair_CRM.Services.Clients;

public interface IClientQueryService
{
    Task<IReadOnlyList<ClientListItemResponse>> GetClientsAsync(ClientFilterRequest? filter = null);
    Task<ClientFormRequest?> GetClientFormAsync(int clientId);
    Task<ClientDetailsResponse?> GetClientDetailsAsync(int clientId);
}