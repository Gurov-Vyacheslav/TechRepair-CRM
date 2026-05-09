using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechRepair_CRM.DTOs.Orders.Parts;
using TechRepair_CRM.Services.Lookups;
using TechRepair_CRM.Services.Orders;

namespace TechRepair_CRM.Pages.Orders;

[Authorize(Roles = "Admin,Manager")]
public class AddPartModel : PageModel
{
    private readonly ILookupService _lookupService;
    private readonly IOrderCommandService _orderCommandService;

    public AddPartModel(
        ILookupService lookupService,
        IOrderCommandService orderCommandService)
    {
        _lookupService = lookupService;
        _orderCommandService = orderCommandService;
    }

    [BindProperty]
    public AddOrderPartRequest Input { get; set; } = new();

    public int OrderId { get; private set; }

    public List<SelectListItem> Parts { get; private set; } = [];

    public async Task OnGetAsync(int orderId)
    {
        OrderId = orderId;
        await LoadSelectListsAsync();
    }

    public async Task<IActionResult> OnPostAsync(int orderId)
    {
        OrderId = orderId;

        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return Page();
        }

        try
        {
            await _orderCommandService.AddPartToOrderAsync(orderId, Input);

            TempData["Success"] = "Деталь добавлена к заказу.";
            return RedirectToPage("/Orders/AddPart", new { orderId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadSelectListsAsync();
            return Page();
        }
    }

    private async Task LoadSelectListsAsync()
    {
        Parts = await _lookupService.GetActivePartsAsync();
    }
}