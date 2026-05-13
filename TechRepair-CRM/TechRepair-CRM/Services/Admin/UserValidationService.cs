using Microsoft.AspNetCore.Identity;
using TechRepair_CRM.Auth;

namespace TechRepair_CRM.Services.Admin;

public class UserValidationService:IUserValidationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    
    public UserValidationService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }
    
    public async Task<ApplicationUser> GetUserOrThrowAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        return user ?? throw new InvalidOperationException("Пользователь не найден.");
    }
    
    public async Task EnsureRoleExistsAsync(string role)
    {
        if (!await _roleManager.RoleExistsAsync(role))
            throw new InvalidOperationException("Указанная роль не найдена.");
    }

}