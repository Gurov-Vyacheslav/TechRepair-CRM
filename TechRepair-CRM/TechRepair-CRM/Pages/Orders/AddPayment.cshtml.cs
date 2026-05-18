using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TechRepair_CRM.DTOs.Orders.Payments;
using TechRepair_CRM.Services.Orders;

namespace TechRepair_CRM.Pages.Orders;

[Authorize(Roles = "Admin,Manager")]
public class AddPaymentModel : PageModel
{
    private readonly IOrderCommandService _orderCommandService;
    private readonly IOrderQueryService _orderQueryService;

    public AddPaymentModel(IOrderCommandService orderCommandService, IOrderQueryService orderQueryService)
    {
        _orderCommandService = orderCommandService;
        _orderQueryService = orderQueryService;
    }

    [BindProperty]
    public AddPaymentRequest Input { get; set; } = new();
    public int OrderId { get; private set; }
    public OrderPaymentSummaryResponse Summary { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int orderId)
    {
        OrderId = orderId;
        var summary = await _orderQueryService.GetPaymentSummaryAsync(orderId);
        if (summary is null) 
            return NotFound();
        Summary = summary;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int orderId)
    {
        OrderId = orderId;
        var summary = await _orderQueryService.GetPaymentSummaryAsync(orderId);
        if (summary is null) 
            return NotFound();
        Summary = summary;
        if (!ModelState.IsValid) 
            return Page();
        
        try
        {
            await _orderCommandService.AddPaymentAsync(orderId, Input);
            return RedirectToPage("/Orders/Details", new { id = orderId });
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            ModelState.AddModelError(string.Empty, pgEx.MessageText);

            Summary = await _orderQueryService.GetPaymentSummaryAsync(orderId)
                      ?? summary;

            return Page();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            Summary = await _orderQueryService.GetPaymentSummaryAsync(orderId)
                      ?? summary;

            return Page();
        }
    }
}