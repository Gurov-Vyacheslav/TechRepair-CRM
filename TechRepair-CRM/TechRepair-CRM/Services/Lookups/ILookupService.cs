using Microsoft.AspNetCore.Mvc.Rendering;

namespace TechRepair_CRM.Services.Lookups;

public interface ILookupService
{
    Task<IReadOnlyList<SelectListItem>> GetDeviceTypesAsync();
    Task<IReadOnlyList<SelectListItem>> GetDevicesAsync();
    Task<IReadOnlyList<SelectListItem>> GetClientsAsync();
    Task<IReadOnlyList<SelectListItem>> GetOrderStatusesAsync();
    Task<IReadOnlyList<SelectListItem>> GetActiveServicesAsync();
    Task<IReadOnlyList<SelectListItem>> GetActiveTechniciansAsync();
    Task<IReadOnlyList<SelectListItem>> GetActivePartsAsync();
}