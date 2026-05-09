using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.DTOs.Orders.Payments;
using TechRepair_CRM.Services.Orders;

namespace TechRepair_CRM.Pages.Orders;

[Authorize(Roles = "Admin,Manager")]
public class AddPaymentModel : PageModel
{
    private readonly IOrderCommandService _orderCommandService;

    public AddPaymentModel(IOrderCommandService orderCommandService)
    {
        _orderCommandService = orderCommandService;
    }

    [BindProperty]
    public AddPaymentRequest Input { get; set; } = new();

    public int OrderId { get; private set; }

    public void OnGet(int orderId)
    {
        OrderId = orderId;
    }

    public async Task<IActionResult> OnPostAsync(int orderId)
    {
        OrderId = orderId;

        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _orderCommandService.AddPaymentAsync(orderId, Input);
            return RedirectToPage("/Orders/Details", new { id = orderId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}