using Microsoft.AspNetCore.Mvc.Rendering;

namespace TechRepair_CRM.Services.Lookups;

public interface ILookupService
{
    Task<List<SelectListItem>> GetDeviceTypesAsync();

    Task<List<SelectListItem>> GetDevicesAsync();

    Task<List<SelectListItem>> GetActiveServicesAsync();
    
    Task<List<SelectListItem>> GetActivePartsAsync();

    Task<List<SelectListItem>> GetActiveTechniciansAsync();
}