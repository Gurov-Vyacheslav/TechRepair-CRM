using TechRepair_CRM.DTOs.References.Parts;
using TechRepair_CRM.DTOs.References.Services;
using TechRepair_CRM.DTOs.References.Technicians;

namespace TechRepair_CRM.Services.References;

public interface IReferenceQueryService
{
    Task<List<ServiceItemResponse>> GetServicesAsync();
    Task<ServiceFormRequest?> GetServiceFormAsync(int id);

    Task<List<PartItemResponse>> GetPartsAsync();
    Task<PartFormRequest?> GetPartFormAsync(int id);

    Task<List<TechnicianItemResponse>> GetTechniciansAsync();
    Task<TechnicianFormRequest?> GetTechnicianFormAsync(int id);
}