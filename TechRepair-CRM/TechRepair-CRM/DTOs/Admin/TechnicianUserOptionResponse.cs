namespace TechRepair_CRM.DTOs.Admin;

public record TechnicianUserOptionResponse(
    int TechnicianId,
    string FullName,
    string Email
);