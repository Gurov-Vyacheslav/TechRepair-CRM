using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.Services.Orders;


namespace TechRepair_CRM.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderCommandService _orderCommandService;
    private readonly IOrderQueryService  _orderQueryService;

    public OrdersController(IOrderCommandService orderCommandService,  IOrderQueryService orderQueryService)
    {
        _orderCommandService = orderCommandService;
        _orderQueryService = orderQueryService;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderListItemResponse>>> GetAll()
    {
        var response = await _orderQueryService.GetOrdersAsync();
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDetailsResponse>> GetById(int id)
    {
        var order = await _orderQueryService.GetOrderDetailsAsync(id);

        return order is null ? NotFound() : Ok(order);
    }
    
    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    public async Task<ActionResult> CreateOrder(CreateOrderRequest request)
    {
        var orderId = await _orderCommandService.CreateOrderAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = orderId }, orderId);
    }
}