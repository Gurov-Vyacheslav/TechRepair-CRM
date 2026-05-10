using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.Services.Orders;

namespace TechRepair_CRM.Pages.Orders;

[Authorize(Roles = "Admin,Manager")]
public class EditModel : PageModel
{
    private readonly IOrderQueryService _orderQueryService;
    private readonly IOrderCommandService _orderCommandService;

    public EditModel(IOrderQueryService orderQueryService, IOrderCommandService orderCommandService)
    {
        _orderQueryService = orderQueryService;
        _orderCommandService = orderCommandService;
    }

    [BindProperty]
    public OrderEditRequest Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var order = await _orderQueryService.GetOrderEditFormAsync(id);
        if (order is null)
            return NotFound();

        Input = order;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _orderCommandService.UpdateOrderAsync(id, Input);
            return RedirectToPage("/Orders/Details", new { id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}