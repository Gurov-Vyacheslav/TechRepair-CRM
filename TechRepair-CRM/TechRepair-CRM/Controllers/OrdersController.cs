using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Data;
using TechRepair_CRM.DTOs;
using TechRepair_CRM.DTOs.Orders;
using TechRepair_CRM.Services;

namespace TechRepair_CRM.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderWorkflowService _orderService;

    public OrdersController(OrderWorkflowService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<ListResponse<OrderListItemResponse>>> GetAll()
    {
        var response = await _orderService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDetailsResponse>> GetById(int id)
    {
        var order = await _orderService.GetByIdAsync(id);

        return order is null ? NotFound() : Ok(order);
    }
    
    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    public async Task<ActionResult<CreateOrderResponse>> CreateOrder(CreateOrderRequest request)
    {
        var response = await _orderService.CreateOrderAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.OrderId }, response);
    }
}