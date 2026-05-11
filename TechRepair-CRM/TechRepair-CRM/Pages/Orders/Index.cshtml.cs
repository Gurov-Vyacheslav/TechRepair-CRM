using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.Services.Lookups;
using TechRepair_CRM.Services.Orders;

namespace TechRepair_CRM.Pages.Orders;

[Authorize(Roles = "Admin,Manager")]
public class IndexModel : PageModel
{
    private readonly IOrderQueryService _orderQueryService;
    private readonly ILookupService _lookupService;

    public IndexModel(IOrderQueryService orderQueryService, ILookupService lookupService)
    {
        _orderQueryService = orderQueryService;
        _lookupService = lookupService;
    }

    [BindProperty(SupportsGet = true)]
    public OrderFilterRequest Filter { get; set; } = new();

    public IReadOnlyList<OrderListItemResponse> Orders { get; private set; } = [];
    public IReadOnlyList<SelectListItem> Statuses { get; private set; } = [];
    public IReadOnlyList<SelectListItem> Technicians { get; private set; } = [];
    public IReadOnlyList<SelectListItem> DeviceTypes { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Orders = await _orderQueryService.GetOrdersAsync(Filter);
        Statuses = await _lookupService.GetOrderStatusesAsync();
        Technicians = await _lookupService.GetActiveTechniciansAsync();
        DeviceTypes = await _lookupService.GetDeviceTypesAsync();
    }
}