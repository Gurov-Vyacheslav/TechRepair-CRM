namespace TechRepair_CRM.DTOs.Admin;

public record UserFilterRequest
{
    public string? Search { get; set; }

    public string? Role { get; set; }

    public bool? IsBlocked { get; set; }
}