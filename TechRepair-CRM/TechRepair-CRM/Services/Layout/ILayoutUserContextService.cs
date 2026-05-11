using TechRepair_CRM.DTOs.Layout;

namespace TechRepair_CRM.Services.Layout;

public interface ILayoutUserContextService
{
    Task<LayoutUserContext> GetAsync();
}