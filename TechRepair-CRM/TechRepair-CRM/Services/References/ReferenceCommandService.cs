using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;
using TechRepair_CRM.DTOs.References.Parts;
using TechRepair_CRM.DTOs.References.Services;
using TechRepair_CRM.DTOs.References.Technicians;
using TechRepair_CRM.Models.Db;

namespace TechRepair_CRM.Services.References;

public class ReferenceCommandService : IReferenceCommandService
{
    private readonly RepairServiceDbContext _db;

    public ReferenceCommandService(RepairServiceDbContext db)
    {
        _db = db;
    }

    public async Task CreateServiceAsync(ServiceFormRequest request)
    {
        var service = new Service
        {
            ServiceName = request.ServiceName,
            Description = request.Description,
            BasePrice = request.BasePrice,
            EstimatedDuration = request.EstimatedDuration,
            IsActive = request.IsActive
        };

        _db.Services.Add(service);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateServiceAsync(int id, ServiceFormRequest request)
    {
        var service = await _db.Services.FindAsync(id);

        if (service is null)
            throw new InvalidOperationException("Услуга не найдена.");

        service.ServiceName = request.ServiceName;
        service.Description = request.Description;
        service.BasePrice = request.BasePrice;
        service.EstimatedDuration = request.EstimatedDuration;
        service.IsActive = request.IsActive;

        await _db.SaveChangesAsync();
    }

    public async Task CreatePartAsync(PartFormRequest request)
    {
        var part = new Part
        {
            PartNumber = request.PartNumber,
            PartName = request.PartName,
            Manufacturer = request.Manufacturer,
            DefaultPrice = request.DefaultPrice,
            IsActive = request.IsActive,
            Description = request.Description
        };

        _db.Parts.Add(part);
        await _db.SaveChangesAsync();
    }

    public async Task UpdatePartAsync(int id, PartFormRequest request)
    {
        var part = await CheckPartExistsAsync(id);

        part.PartNumber = request.PartNumber;
        part.PartName = request.PartName;
        part.Manufacturer = request.Manufacturer;
        part.DefaultPrice = request.DefaultPrice;
        part.IsActive = request.IsActive;
        part.Description = request.Description;

        await _db.SaveChangesAsync();
    }

    private async Task<Part> CheckPartExistsAsync(int id)
    {
        var part = await _db.Parts.FindAsync(id);

        return part ?? throw new InvalidOperationException("Деталь не найдена.");
    }

    public async Task<int> CreateTechnicianAsync(TechnicianFormRequest request)
    {
        await  CheckTechnicianEmailExistsAsync(request.Email);
        await  CheckTechnicianPhoneExistsAsync(request.Phone);

        var technician = new Technician
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Specialization = request.Specialization,
            IsActive = request.IsActive,
            Notes = request.Notes
        };

        _db.Technicians.Add(technician);
        await _db.SaveChangesAsync();

        return technician.TechnicianId;
    }

    private async Task CheckTechnicianEmailExistsAsync(string email, int exceptId = 0)
    {
        var emailExists = await _db.Technicians
            .AnyAsync(t => t.Email == email && t.TechnicianId != exceptId);

        if (emailExists)
            throw new InvalidOperationException("Мастер с таким email уже существует.");
    }

    private async Task CheckTechnicianPhoneExistsAsync(string phone, int exceptId = 0)
    {
        var phoneExists = await _db.Technicians
            .AnyAsync(t => t.Phone == phone && t.TechnicianId != exceptId);

        if (phoneExists)
            throw new InvalidOperationException("Мастер с таким телефоном уже существует.");
    }

    public async Task UpdateTechnicianAsync(int id, TechnicianFormRequest request)
    {
        var technician = await CheckTechnicianExistsAsync(id);

        await CheckTechnicianEmailExistsAsync(request.Email, id);
        await CheckTechnicianPhoneExistsAsync(request.Phone, id);

        technician.FirstName = request.FirstName;
        technician.LastName = request.LastName;
        technician.Email = request.Email;
        technician.Phone = request.Phone;
        technician.Specialization = request.Specialization;
        technician.IsActive = request.IsActive;
        technician.Notes = request.Notes;

        await _db.SaveChangesAsync();
    }

    private async Task<Technician> CheckTechnicianExistsAsync(int id)
    {
        var technician = await _db.Technicians.FindAsync(id);

        return technician ?? throw new InvalidOperationException("Мастер не найден.");
    }
}