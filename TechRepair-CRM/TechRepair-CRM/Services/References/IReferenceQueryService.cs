using TechRepair_CRM.DTOs.References;
using TechRepair_CRM.DTOs.References.Parts;
using TechRepair_CRM.DTOs.References.Services;
using TechRepair_CRM.DTOs.References.Technicians;

namespace TechRepair_CRM.Services.References;

public interface IReferenceQueryService
{
    Task<IReadOnlyList<ServiceItemResponse>> GetServicesAsync(ReferenceFilterRequest? filter = null);
    Task<ServiceFormRequest?> GetServiceFormAsync(int id);

    Task<IReadOnlyList<PartItemResponse>> GetPartsAsync(ReferenceFilterRequest? filter = null);
    Task<PartFormRequest?> GetPartFormAsync(int id);

    Task<IReadOnlyList<TechnicianItemResponse>> GetTechniciansAsync(ReferenceFilterRequest? filter = null);
    Task<TechnicianFormRequest?> GetTechnicianFormAsync(int id);
}