using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderApi.src.Application.Services;
using System.Security.Claims;

namespace OrderApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrderController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orders = await _orderService.GetAllAsync();
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order is null)
            return NotFound(new { message = "Pedido não encontrado." });
        return Ok(order);
    }

    [HttpPost]
    [Authorize(Roles = "Cliente")]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        try
        {
            
            var customerIdClaim = User.FindFirst("customerId")?.Value;
            if (customerIdClaim is null)
                return Unauthorized(new { message = "Token inválido." });

            var customerId = Guid.Parse(customerIdClaim);

            var order = await _orderService.CreateAsync(
                customerId, request.ProductId, request.Quantity,
                request.ZipCode, request.DeliveryAddress);

            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public record CreateOrderRequest(Guid ProductId, int Quantity, string ZipCode, string DeliveryAddress);