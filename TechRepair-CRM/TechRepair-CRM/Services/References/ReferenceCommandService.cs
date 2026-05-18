using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;
using TechRepair_CRM.DTOs.References.DeviceTypes;
using TechRepair_CRM.DTOs.References.Parts;
using TechRepair_CRM.DTOs.References.Services;
using TechRepair_CRM.DTOs.References.Technicians;
using TechRepair_CRM.Models.Db;
using TechRepair_CRM.Services.Entity;

namespace TechRepair_CRM.Services.References;

public class ReferenceCommandService : IReferenceCommandService
{
    private readonly RepairServiceDbContext _db;
    private readonly IEntityValidationService _entityValidationService;

    public ReferenceCommandService(
        RepairServiceDbContext db, 
        IEntityValidationService entityValidationService)
    {
        _db = db;
        _entityValidationService = entityValidationService;
        
    }

    public async Task CreateServiceAsync(ServiceFormRequest request)
    {
        await EnsureServiceNameIsUniqueAsync(request.ServiceName);

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
    
    private async Task EnsureServiceNameIsUniqueAsync(string serviceName, int exceptId = 0)
    {
        var exists = await _db.Services
            .AnyAsync(s =>
                s.ServiceName == serviceName &&
                s.ServiceId != exceptId);

        if (exists)
            throw new InvalidOperationException("Услуга с таким названием уже существует.");
    }

    public async Task UpdateServiceAsync(int id, ServiceFormRequest request)
    {
        var service = await _entityValidationService.GetServiceOrThrowAsync(id);

        await EnsureServiceNameIsUniqueAsync(request.ServiceName, id);

        service.ServiceName = request.ServiceName;
        service.Description = request.Description;
        service.BasePrice = request.BasePrice;
        service.EstimatedDuration = request.EstimatedDuration;
        service.IsActive = request.IsActive;

        await _db.SaveChangesAsync();
    }


    public async Task CreatePartAsync(PartFormRequest request)
    {
        await EnsurePartNumberIsUniqueAsync(request.PartNumber);

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
    
    private async Task EnsurePartNumberIsUniqueAsync(string? partNumber, int exceptId = 0)
    {
        if (string.IsNullOrWhiteSpace(partNumber))
            return;

        var normalizedPartNumber = partNumber.Trim();

        var exists = await _db.Parts
            .AnyAsync(p =>
                p.PartNumber == normalizedPartNumber &&
                p.PartId != exceptId);

        if (exists)
            throw new InvalidOperationException("Деталь с таким артикулом уже существует.");
    }
    

    public async Task UpdatePartAsync(int id, PartFormRequest request)
    {
        var part = await _entityValidationService.GetPartOrThrowAsync(id);

        await EnsurePartNumberIsUniqueAsync(request.PartNumber, id);

        part.PartNumber = request.PartNumber;
        part.PartName = request.PartName;
        part.Manufacturer = request.Manufacturer;
        part.DefaultPrice = request.DefaultPrice;
        part.IsActive = request.IsActive;
        part.Description = request.Description;

        await _db.SaveChangesAsync();
    }
    
    public async Task<int> CreateTechnicianAsync(TechnicianFormRequest request)
    {
        await EnsureTechnicianEmailIsUniqueAsync(request.Email);
        await EnsureTechnicianPhoneIsUniqueAsync(request.Phone);

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

    private async Task EnsureTechnicianEmailIsUniqueAsync(string email, int exceptId = 0)
    {
        var emailExists = await _db.Technicians
            .AnyAsync(t => t.Email == email && t.TechnicianId != exceptId);

        if (emailExists)
            throw new InvalidOperationException("Мастер с таким email уже существует.");
    }

    private async Task EnsureTechnicianPhoneIsUniqueAsync(string phone, int exceptId = 0)
    {
        var phoneExists = await _db.Technicians
            .AnyAsync(t => t.Phone == phone && t.TechnicianId != exceptId);

        if (phoneExists)
            throw new InvalidOperationException("Мастер с таким телефоном уже существует.");
    }

    public async Task UpdateTechnicianAsync(int id, TechnicianFormRequest request)
    {
        var technician = await _entityValidationService.GetTechnicianOrThrowAsync(id);

        await EnsureTechnicianEmailIsUniqueAsync(request.Email, id);
        await EnsureTechnicianPhoneIsUniqueAsync(request.Phone, id);

        technician.FirstName = request.FirstName;
        technician.LastName = request.LastName;
        technician.Email = request.Email;
        technician.Phone = request.Phone;
        technician.Specialization = request.Specialization;
        technician.IsActive = request.IsActive;
        technician.Notes = request.Notes;

        await _db.SaveChangesAsync();
    }
    
    public async Task CreateDeviceTypeAsync(DeviceTypeFormRequest request)
    {
        await EnsureDeviceTypeNameIsUniqueAsync(request.TypeName);
        
        var deviceType = new DeviceType { TypeName = request.TypeName.Trim() };
        
        _db.DeviceTypes.Add(deviceType);
        await _db.SaveChangesAsync();
    }
    
    private async Task EnsureDeviceTypeNameIsUniqueAsync(string typeName, int exceptId = 0)
    {
        var normalizedTypeName = typeName.Trim();
        
        var exists = await _db.DeviceTypes.AnyAsync(t => 
            t.TypeName == normalizedTypeName && t.DeviceTypeId != exceptId);
        
        if (exists) throw new InvalidOperationException("Тип устройства с таким названием уже существует.");
    }

    public async Task UpdateDeviceTypeAsync(int id, DeviceTypeFormRequest request)
    {
        var deviceType = await _entityValidationService.GetDeviceTypeOrThrowAsync(id);
        
        await EnsureDeviceTypeNameIsUniqueAsync(request.TypeName, id);
        
        deviceType.TypeName = request.TypeName.Trim();
        await _db.SaveChangesAsync();
    }

    public async Task DeleteDeviceTypeAsync(int id)
    {
        var deviceType = await _entityValidationService.GetDeviceTypeOrThrowAsync(id);
        
        await EnsureDeviceTypeIsNotUsedAsync(id);
        
        _db.DeviceTypes.Remove(deviceType);
        await _db.SaveChangesAsync();
    }
    
    private async Task EnsureDeviceTypeIsNotUsedAsync(int id)
    {
        var isUsed = await _db.Devices.AnyAsync(d => d.DeviceTypeId == id);
        if (isUsed) 
            throw new InvalidOperationException("Нельзя удалить тип устройства, который используется устройствами.");
    }

}