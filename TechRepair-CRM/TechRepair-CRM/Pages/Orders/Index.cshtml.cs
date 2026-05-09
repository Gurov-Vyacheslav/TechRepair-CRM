using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.Services.Orders;

namespace TechRepair_CRM.Pages.Orders;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IOrderQueryService _orderQueryService;

    public IndexModel(IOrderQueryService orderQueryService)
    {
        _orderQueryService = orderQueryService;
    }

    public List<OrderListItemResponse> Orders { get; private set; } 

    public async Task OnGetAsync()
    {
        Orders = await _orderQueryService.GetOrdersAsync();
    }
}