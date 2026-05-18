namespace TechRepair_CRM.DTOs.Admin;

public record UserListItemResponse(
    string Id,
    string Email,
    string? Role,
    int? TechnicianId,
    string? TechnicianFullName,
    bool IsBlocked
);