using TechRepair_CRM.DTOs.References.Parts;
using TechRepair_CRM.DTOs.References.Services;
using TechRepair_CRM.DTOs.References.Technicians;

namespace TechRepair_CRM.Services.References;

public interface IReferenceCommandService
{
    Task CreateServiceAsync(ServiceFormRequest request);
    Task UpdateServiceAsync(int id, ServiceFormRequest request);

    Task CreatePartAsync(PartFormRequest request);
    Task UpdatePartAsync(int id, PartFormRequest request);

    Task<int> CreateTechnicianAsync(TechnicianFormRequest request);
    Task UpdateTechnicianAsync(int id, TechnicianFormRequest request);
}