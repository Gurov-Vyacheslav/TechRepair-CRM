namespace TechRepair_CRM.DTOs.Clients;

public record ClientFilterRequest
{
    public string? FullName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }
}