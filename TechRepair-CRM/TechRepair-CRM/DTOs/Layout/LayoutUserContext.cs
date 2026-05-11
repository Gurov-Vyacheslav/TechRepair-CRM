namespace TechRepair_CRM.DTOs.Layout;

public record LayoutUserContext(
    bool IsAuthenticated,
    bool IsAdmin,
    bool IsManager,
    bool IsTechnician,
    bool IsLinkedToTechnician,
    string? UserName
)
{
    public bool CanManageSystem => IsAdmin || IsManager;

    public bool CanShowMyWork => IsLinkedToTechnician;

    public bool CanShowAdminPanel => IsAdmin;
}