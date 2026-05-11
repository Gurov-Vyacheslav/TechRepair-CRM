using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.DTOs.Orders.Parts;
using TechRepair_CRM.Services.Orders;

namespace TechRepair_CRM.Pages.Orders;

[Authorize(Roles = "Admin,Manager")]
public class EditPartModel : PageModel
{
    private readonly IOrderQueryService _orderQueryService;
    private readonly IOrderCommandService _orderCommandService;

    public EditPartModel(
        IOrderQueryService orderQueryService,
        IOrderCommandService orderCommandService)
    {
        _orderQueryService = orderQueryService;
        _orderCommandService = orderCommandService;
    }

    [BindProperty]
    public EditOrderPartRequest Input { get; set; } = new();

    public int OrderId { get; private set; }
    public int PartId { get; private set; }

    public async Task<IActionResult> OnGetAsync(int orderId, int partId)
    {
        OrderId = orderId;
        PartId = partId;

        var input = await _orderQueryService.GetOrderPartEditFormAsync(orderId, partId);

        if (input is null)
            return NotFound();

        Input = input;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int orderId, int partId)
    {
        OrderId = orderId;
        PartId = partId;

        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _orderCommandService.UpdateOrderPartAsync(orderId, partId, Input);
            return RedirectToPage("/Orders/Details", new { id = orderId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}