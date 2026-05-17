using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.DTOs.Orders.Services.Parts;
using TechRepair_CRM.Services.Orders;

namespace TechRepair_CRM.Pages.Orders;

[Authorize(Roles = "Admin,Manager")]
public class EditServicePartModel : PageModel
{
    private readonly IOrderQueryService _orderQueryService;
    private readonly IOrderCommandService _orderCommandService;

    public EditServicePartModel(
        IOrderQueryService orderQueryService,
        IOrderCommandService orderCommandService)
    {
        _orderQueryService = orderQueryService;
        _orderCommandService = orderCommandService;
    }

    [BindProperty]
    public EditOrderServicePartRequest Input { get; set; } = new();

    public int OrderId { get; private set; }
    public int ServiceId { get; private set; }
    public int PartId { get; private set; }

    public async Task<IActionResult> OnGetAsync(int orderId, int serviceId, int partId)
    {
        OrderId = orderId;
        ServiceId = serviceId;
        PartId = partId;

        var input = await _orderQueryService.GetOrderServicePartEditFormAsync(orderId, serviceId, partId);

        if (input is null)
            return NotFound();

        Input = input;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int orderId, int serviceId, int partId)
    {
        OrderId = orderId;
        ServiceId = serviceId;
        PartId = partId;

        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _orderCommandService.UpdateOrderServicePartAsync(orderId, serviceId, partId, Input);
            return RedirectToPage("/Orders/Details", new { id = orderId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}