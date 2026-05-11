using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Auth;
using TechRepair_CRM.Data;
using TechRepair_CRM.DTOs.Admin;

namespace TechRepair_CRM.Services.Admin;

public class UserAdminService : IUserAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly RepairServiceDbContext _db;

    public UserAdminService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        RepairServiceDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }

   public async Task<IReadOnlyList<UserListItemResponse>> GetUsersAsync(UserFilterRequest? filter = null)
    {
        var usersQuery = GetFilteredUsers(filter);

        // Получаем данные пользователей, которые точно понадобятся
        var usersData = await usersQuery
            .OrderBy(u => u.Email)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.UserName,
                u.TechnicianId,
                u.LockoutEnd
            })
            .ToListAsync();

        if (!usersData.Any()) return new List<UserListItemResponse>();

        var userIds = usersData.Select(u => u.Id).ToList();

        // 2. Загружаем роли для всех этих пользователей 
        var rolesDictionary = await GetUsersRoles(userIds);

        // 3. Загружаем мастеров для всех пользователей
        var technicianIds = usersData.Where(u => u.TechnicianId.HasValue)
                                     .Select(u => u.TechnicianId.Value)
                                     .Distinct()
                                     .ToList();
        
        var technicians = await GetTechnicianFullNames(technicianIds);

        // 4. Формируем финальный результат
        var result = new List<UserListItemResponse>();
        foreach (var user in usersData)
        {
            // Проверка фильтра по роли (в памяти)
            var userRoles = rolesDictionary.GetValueOrDefault(user.Id) ?? new List<string>();
            var userRole = userRoles.FirstOrDefault();
            if (filter?.Role != null && userRole != filter.Role) continue;

            var isLocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.Now;
            var technicianFullName = user.TechnicianId.HasValue
                ? technicians.GetValueOrDefault(user.TechnicianId.Value)
                : null;

            result.Add(new UserListItemResponse(
                user.Id,
                user.Email ?? string.Empty,
                userRole,
                user.TechnicianId,
                technicianFullName,
                isLocked
            ));
        }

        return result;
    }

    private IQueryable<ApplicationUser> GetFilteredUsers(UserFilterRequest? filter)
    {
        var usersQuery = _userManager.Users.AsQueryable();

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim().ToLower();
                usersQuery = usersQuery.Where(u 
                    => u.Email.ToLower().Contains(search) 
                       || u.UserName.ToLower().Contains(search));
            }

            if (filter.IsLocked.HasValue)
            {
                var now = DateTimeOffset.UtcNow;
                if (filter.IsLocked.Value)
                    usersQuery = usersQuery.Where(u => u.LockoutEnd != null && u.LockoutEnd > now);
                else
                    usersQuery = usersQuery.Where(u => u.LockoutEnd == null || u.LockoutEnd <= now);
            }
        }

        return usersQuery;
    }

    private async Task<Dictionary<string, List<string>>> GetUsersRoles(List<string> userIds)
    {
        var rolesDictionary = new Dictionary<string, List<string>>();
        foreach (var userId in userIds)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Any())
                    rolesDictionary[userId] = roles.ToList();
            }
        }

        return rolesDictionary;
    }

    private async Task<Dictionary<int, string>> GetTechnicianFullNames(List<int> technicianIds)
    {
        var technicians = new Dictionary<int, string>();
        if (technicianIds.Any())
        {
            var techs = await _db.Technicians
                .Where(t => technicianIds.Contains(t.TechnicianId))
                .Select(t => new { t.TechnicianId, FullName = t.LastName + " " + t.FirstName })
                .ToListAsync();
            technicians = techs.ToDictionary(t => t.TechnicianId, t => t.FullName);
        }

        return technicians;
    }

    public async Task<UserEditRequest?> GetUserForEditAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new UserEditRequest
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            Role = roles.FirstOrDefault() ?? string.Empty,
            TechnicianId = user.TechnicianId,
            IsActive = user.LockoutEnd is null || user.LockoutEnd <= DateTimeOffset.UtcNow
        };
    }

    public async Task CreateUserAsync(UserCreateRequest request)
    {
        await EnsureRoleExistsAsync(request.Role);
        await EnsureTechnicianIsValidAsync(request.TechnicianId);

        var email = await ResolveUserEmailAsync(
            request.TechnicianId,
            request.Email);

        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser is not null)
            throw new InvalidOperationException("Пользователь с таким email уже существует.");
        
        if (request.Role == "Technician" && request.TechnicianId is null)
            throw new InvalidOperationException("Для роли Technician нужно выбрать мастера.");

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            TechnicianId = request.TechnicianId,
            LockoutEnabled = true
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        ThrowIfFailed(createResult);

        var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
        ThrowIfFailed(roleResult);
    }
    
    private async Task<string> ResolveUserEmailAsync(int? technicianId, string requestEmail)
    {
        if (technicianId is null)
            return requestEmail;

        var technicianEmail = await _db.Technicians
            .Where(t => t.TechnicianId == technicianId.Value && t.IsActive)
            .Select(t => t.Email)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(technicianEmail))
            throw new InvalidOperationException("Email активного мастера не найден.");

        return technicianEmail;
    }
    public async Task UpdateUserAsync(string id, UserEditRequest request)
    {
        var user = await CheckUserExist(id);

        await EnsureRoleExistsAsync(request.Role);
        await EnsureTechnicianIsValidAsync(request.TechnicianId);

        var email = await ResolveUserEmailAsync(
            request.TechnicianId,
            request.Email);

        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser is not null && existingUser.Id != user.Id)
            throw new InvalidOperationException("Пользователь с таким email уже существует.");
        
        if (request.Role == "Technician" && request.TechnicianId is null)
            throw new InvalidOperationException("Для роли Technician нужно выбрать мастера.");

        user.Email = email;
        user.UserName = email;
        user.NormalizedEmail = _userManager.NormalizeEmail(email);
        user.NormalizedUserName = _userManager.NormalizeName(email);
        user.TechnicianId = request.TechnicianId;
        user.LockoutEnabled = true;

        var updateResult = await _userManager.UpdateAsync(user);
        ThrowIfFailed(updateResult);

        var currentRoles = await _userManager.GetRolesAsync(user);

        if (currentRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            ThrowIfFailed(removeResult);
        }

        var addRoleResult = await _userManager.AddToRoleAsync(user, request.Role);
        ThrowIfFailed(addRoleResult);

        if (request.IsActive)
        {
            var unlockResult = await _userManager.SetLockoutEndDateAsync(user, null);
            ThrowIfFailed(unlockResult);
        }
        else
        {
            var lockResult = await _userManager.SetLockoutEndDateAsync(
                user,
                DateTimeOffset.UtcNow.AddYears(100));

            ThrowIfFailed(lockResult);
        }
    }

    private async Task<ApplicationUser> CheckUserExist(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        return user ?? throw new InvalidOperationException("Пользователь не найден.");
    }

    public async Task<IReadOnlyList<string>> GetRolesAsync()
    {
        return await _roleManager.Roles
            .OrderBy(r => r.Name)
            .Select(r => r.Name!)
            .ToListAsync();
    }

    private async Task EnsureRoleExistsAsync(string role)
    {
        if (!await _roleManager.RoleExistsAsync(role))
            throw new InvalidOperationException("Указанная роль не найдена.");
    }

    private async Task EnsureTechnicianIsValidAsync(int? technicianId)
    {
        if (technicianId is null)
            return;

        var technicianExists = await _db.Technicians
            .AnyAsync(t => t.TechnicianId == technicianId.Value && t.IsActive);

        if (!technicianExists)
            throw new InvalidOperationException("Выбранный мастер не найден или неактивен.");
    }

    private static void ThrowIfFailed(IdentityResult result)
    {
        if (result.Succeeded)
            return;

        var errors = string.Join("; ", result.Errors.Select(e => e.Description));
        throw new InvalidOperationException(errors);
    }
    
    public async Task<string?> GetTechnicianEmailAsync(int technicianId)
    {
        return await _db.Technicians
            .Where(t => t.TechnicianId == technicianId)
            .Select(t => t.Email)
            .FirstOrDefaultAsync();
    }
    
    public async Task<IReadOnlyList<TechnicianUserOptionResponse>> GetTechnicianOptionsAsync()
    {
        return await _db.Technicians
            .Where(t => t.IsActive)
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .Select(t => new TechnicianUserOptionResponse(
                t.TechnicianId,
                t.LastName + " " + t.FirstName,
                t.Email
            ))
            .ToListAsync();
    }
}
