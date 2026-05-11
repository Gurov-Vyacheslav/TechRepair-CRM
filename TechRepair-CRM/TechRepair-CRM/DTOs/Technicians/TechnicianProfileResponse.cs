namespace TechRepair_CRM.DTOs.Technicians;

public record TechnicianProfileResponse(
    int TechnicianId,
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string? Specialization,
    bool IsActive,
    string? Notes
);