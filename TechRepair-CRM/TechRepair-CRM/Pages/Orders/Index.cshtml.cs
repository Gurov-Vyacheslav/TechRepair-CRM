using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.Services.Lookups;
using TechRepair_CRM.Services.Orders;

namespace TechRepair_CRM.Pages.Orders;

[Authorize]
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

    public List<OrderListItemResponse> Orders { get; private set; } = [];
    public List<SelectListItem> Statuses { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Statuses = await _lookupService.GetOrderStatusesAsync();
        Orders = await _orderQueryService.GetOrdersAsync(Filter);
    }
}