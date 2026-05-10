namespace TechRepair_CRM.DTOs.References;

public record ReferenceFilterRequest
{
    public string? Search { get; set; }

    public bool? IsActive { get; set; }
}