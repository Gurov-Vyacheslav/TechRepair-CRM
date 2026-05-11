using TechRepair_CRM.DTOs.Layout;
using TechRepair_CRM.Services.CurrentUser;

namespace TechRepair_CRM.Services.Layout;

public class LayoutUserContextService : ILayoutUserContextService
{
    private readonly ICurrentUserService _currentUserService;

    public LayoutUserContextService(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public async Task<LayoutUserContext> GetAsync()
    {
        var user = _currentUserService.User;

        var isAuthenticated = user.Identity?.IsAuthenticated == true;

        if (!isAuthenticated)
        {
            return new LayoutUserContext(
                IsAuthenticated: false,
                IsAdmin: false,
                IsManager: false,
                IsTechnician: false,
                IsLinkedToTechnician: false,
                UserName: null
            );
        }

        var technicianId = await _currentUserService.GetTechnicianIdAsync();

        return new LayoutUserContext(
            IsAuthenticated: true,
            IsAdmin: user.IsInRole("Admin"),
            IsManager: user.IsInRole("Manager"),
            IsTechnician: user.IsInRole("Technician"),
            IsLinkedToTechnician: technicianId is not null,
            UserName: user.Identity?.Name
        );
    }
}