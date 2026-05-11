using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using TechRepair_CRM.Auth;

namespace TechRepair_CRM.Services.CurrentUser;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    public ClaimsPrincipal User =>
        _httpContextAccessor.HttpContext?.User
        ?? new ClaimsPrincipal(new ClaimsIdentity());

    public string? UserId => _userManager.GetUserId(User);

    public async Task<ApplicationUser?> GetApplicationUserAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
            return null;

        return await _userManager.GetUserAsync(User);
    }

    public async Task<int?> GetTechnicianIdAsync()
    {
        var user = await GetApplicationUserAsync();
        return user?.TechnicianId;
    }

    public bool IsInRole(string role)
    {
        return User.IsInRole(role);
    }

    public bool IsAdminOrManager()
    {
        return IsInRole("Admin") || IsInRole("Manager");
    }

    public bool IsTechnician()
    {
        return IsInRole("Technician");
    }
}