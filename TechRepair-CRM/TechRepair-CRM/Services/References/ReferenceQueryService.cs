using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;
using TechRepair_CRM.DTOs.References;
using TechRepair_CRM.DTOs.References.Parts;
using TechRepair_CRM.DTOs.References.Services;
using TechRepair_CRM.DTOs.References.Technicians;
using TechRepair_CRM.Models.Db;

namespace TechRepair_CRM.Services.References;

public class ReferenceQueryService : IReferenceQueryService
{
    private readonly RepairServiceDbContext _db;

    public ReferenceQueryService(RepairServiceDbContext db)
    {
        _db = db;
    }
    

    public async Task<IReadOnlyList<ServiceItemResponse>> GetServicesAsync(ReferenceFilterRequest? filter = null)
    {
        var query = GetFilteredServices(filter);

        return await query
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

    private IQueryable<Service> GetFilteredServices(ReferenceFilterRequest? filter)
    {
        var services = _db.Services.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter?.Search))
        {
            var search = filter.Search.Trim().ToLower();

            services = services.Where(s =>
                s.ServiceName.ToLower().Contains(search) ||
                (s.Description != null && s.Description.ToLower().Contains(search)));
        }

        if (filter?.IsActive is not null)
        {
            services = services.Where(s => s.IsActive == filter.IsActive.Value);
        }

        return services;
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

    public async Task<IReadOnlyList<PartItemResponse>> GetPartsAsync(ReferenceFilterRequest? filter = null)
    {
        var query = GetFilteredParts(filter);

        return await query
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

    private IQueryable<Part> GetFilteredParts(ReferenceFilterRequest? filter)
    {
        var parts = _db.Parts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter?.Search))
        {
            var search = filter.Search.Trim().ToLower();

            parts = parts.Where(p =>
                p.PartName.ToLower().Contains(search) ||
                (p.PartNumber != null && p.PartNumber.ToLower().Contains(search)) ||
                (p.Manufacturer != null && p.Manufacturer.ToLower().Contains(search)) ||
                (p.Description != null && p.Description.ToLower().Contains(search)));
        }

        if (filter?.IsActive is not null)
        {
            parts = parts.Where(p => p.IsActive == filter.IsActive.Value);
        }

        return parts;
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

    public async Task<IReadOnlyList<TechnicianItemResponse>> GetTechniciansAsync(ReferenceFilterRequest? filter = null)
    {
        var query = GetFilteredTechnicians(filter);

        return await query
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

    private IQueryable<Technician> GetFilteredTechnicians(ReferenceFilterRequest? filter)
    {
        var technicians = _db.Technicians.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter?.Search))
        {
            var search = filter.Search.Trim().ToLower();

            technicians = technicians.Where(t =>
                (t.LastName + " " + t.FirstName).ToLower().Contains(search) ||
                (t.FirstName + " " + t.LastName).ToLower().Contains(search) ||
                t.Email.ToLower().Contains(search) ||
                t.Phone.Contains(search) ||
                (t.Specialization != null && t.Specialization.ToLower().Contains(search)));
        }

        if (filter?.IsActive is not null)
        {
            technicians = technicians.Where(t => t.IsActive == filter.IsActive.Value);
        }
        
        return technicians;
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