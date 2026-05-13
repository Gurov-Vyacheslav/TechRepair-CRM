using TechRepair_CRM.Auth;

namespace TechRepair_CRM.Services.Admin;

public interface IUserValidationService
{
    Task<ApplicationUser> GetUserOrThrowAsync(string id);
    Task EnsureRoleExistsAsync(string role);
}