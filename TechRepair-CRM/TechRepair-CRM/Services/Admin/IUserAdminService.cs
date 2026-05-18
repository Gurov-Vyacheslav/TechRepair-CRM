using TechRepair_CRM.DTOs.Admin;

namespace TechRepair_CRM.Services.Admin;

public interface IUserAdminService
{
    Task<IReadOnlyList<UserListItemResponse>> GetUsersAsync(UserFilterRequest? filter = null);
    Task<UserEditRequest?> GetUserForEditAsync(string id);
    Task CreateUserAsync(UserCreateRequest request);
    Task UpdateUserAsync(string id, UserEditRequest request);
    Task<IReadOnlyList<string>> GetRolesAsync();
    Task<string?> GetTechnicianEmailAsync(int technicianId);
    Task<IReadOnlyList<TechnicianUserOptionResponse>> GetTechnicianOptionsAsync();
    Task<IReadOnlyList<TechnicianUserOptionResponse>> GetAvailableTechnicianOptionsForEditAsync(string userId);
    Task<IReadOnlyList<TechnicianUserOptionResponse>> GetAvailableTechnicianOptionsForCreateAsync();
}