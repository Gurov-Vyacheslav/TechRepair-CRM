using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;
using TechRepair_CRM.DTOs.References.Parts;
using TechRepair_CRM.DTOs.References.Services;
using TechRepair_CRM.DTOs.References.Technicians;

namespace TechRepair_CRM.Services.References;

public class ReferenceQueryService : IReferenceQueryService
{
    private readonly RepairServiceDbContext _db;

    public ReferenceQueryService(RepairServiceDbContext db)
    {
        _db = db;
    }

    public async Task<List<ServiceItemResponse>> GetServicesAsync()
    {
        return await _db.Services
            .OrderBy(s => s.ServiceName)
            .Select(s => new ServiceItemResponse(
                s.ServiceId,
                s.ServiceName,
                s.BasePrice,
                s.EstimatedDuration,
                s.IsActive
            ))
            .ToListAsync();
    }

    public async Task<ServiceFormRequest?> GetServiceFormAsync(int id)
    {
        return await _db.Services
            .Where(s => s.ServiceId == id)
            .Select(s => new ServiceFormRequest
            {
                ServiceName = s.ServiceName,
                Description = s.Description,
                BasePrice = s.BasePrice,
                EstimatedDuration = s.EstimatedDuration,
                IsActive = s.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<PartItemResponse>> GetPartsAsync()
    {
        return await _db.Parts
            .OrderBy(p => p.PartName)
            .Select(p => new PartItemResponse(
                p.PartId,
                p.PartNumber,
                p.PartName,
                p.Manufacturer,
                p.DefaultPrice,
                p.IsActive
            ))
            .ToListAsync();
    }

    public async Task<PartFormRequest?> GetPartFormAsync(int id)
    {
        return await _db.Parts
            .Where(p => p.PartId == id)
            .Select(p => new PartFormRequest
            {
                PartNumber = p.PartNumber,
                PartName = p.PartName,
                Manufacturer = p.Manufacturer,
                DefaultPrice = p.DefaultPrice,
                IsActive = p.IsActive,
                Description = p.Description
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<TechnicianItemResponse>> GetTechniciansAsync()
    {
        return await _db.Technicians
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .Select(t => new TechnicianItemResponse(
                t.TechnicianId,
                t.FirstName,
                t.LastName,
                t.Email,
                t.Phone,
                t.Specialization,
                t.IsActive
            ))
            .ToListAsync();
    }

    public async Task<TechnicianFormRequest?> GetTechnicianFormAsync(int id)
    {
        return await _db.Technicians
            .Where(t => t.TechnicianId == id)
            .Select(t => new TechnicianFormRequest
            {
                FirstName = t.FirstName,
                LastName = t.LastName,
                Email = t.Email,
                Phone = t.Phone,
                Specialization = t.Specialization,
                IsActive = t.IsActive,
                Notes = t.Notes
            })
            .FirstOrDefaultAsync();
    }
}