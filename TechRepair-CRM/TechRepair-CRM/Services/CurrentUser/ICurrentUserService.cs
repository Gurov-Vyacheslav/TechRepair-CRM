using System.Security.Claims;
using TechRepair_CRM.Auth;

namespace TechRepair_CRM.Services.CurrentUser;

public interface ICurrentUserService
{
    ClaimsPrincipal User { get; }

    string? UserId { get; }

    Task<ApplicationUser?> GetApplicationUserAsync();

    Task<int?> GetTechnicianIdAsync();

    bool IsInRole(string role);

    bool IsAdminOrManager();

    bool IsTechnician();
}