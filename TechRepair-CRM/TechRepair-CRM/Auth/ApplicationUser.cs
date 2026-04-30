using Microsoft.AspNetCore.Identity;

namespace TechRepair_CRM.Auth;

public class ApplicationUser : IdentityUser
{
    public int? TechnicianId { get; set; }
}