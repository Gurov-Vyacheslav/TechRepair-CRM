using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.Services.Orders;

namespace TechRepair_CRM.Pages.Orders;

[Authorize(Roles = "Admin,Manager,Technician")]
public class DetailsModel : PageModel
{
    private readonly IOrderQueryService _orderQueryService;
    private readonly IOrderCommandService _orderCommandService;

    public DetailsModel(
        IOrderQueryService orderQueryService,
        IOrderCommandService orderCommandService)
    {
        _orderQueryService = orderQueryService;
        _orderCommandService = orderCommandService;
    }

    public OrderDetailsResponse Order { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        return await LoadPageAsync(id);
    }

    public async Task<IActionResult> OnPostCompleteServiceAsync(int id, int serviceId)
    {
        try
        {
            await _orderCommandService.CompleteServiceAsync(id, serviceId);
            TempData["Success"] = "Услуга отмечена как выполненная.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostChangeStatusAsync(
        int id,
        string newStatus,
        string? comment)
    {
        try
        {
            await _orderCommandService.ChangeStatusAsync(
                id,
                newStatus,
                string.IsNullOrWhiteSpace(comment)
                    ? $"Статус изменён пользователем {User.Identity?.Name}"
                    : comment);

            TempData["Success"] = "Статус заказа изменён.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage(new { id });
    }

    private async Task<IActionResult> LoadPageAsync(int id)
    {
        var order = await _orderQueryService.GetOrderDetailsAsync(id);

        if (order is null)
            return NotFound();

        Order = order;
        return Page();
    }
}