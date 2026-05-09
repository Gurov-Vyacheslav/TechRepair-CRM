namespace TechRepair_CRM.DTOs.References.Technicians;

public record TechnicianItemResponse(
    int TechnicianId,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Specialization,
    bool IsActive
);