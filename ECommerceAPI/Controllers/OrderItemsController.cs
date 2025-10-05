using ECommerceAPI.Data;
using ECommerceAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ECommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] //Entire controller restricted to Admin only
    public class OrderItemsController : ControllerBase
    {
        private readonly AppDbContext context;

        public OrderItemsController(AppDbContext context)
        {
            this.context = context;
        }

        // Get All OrderItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderItem>>> GetOrderItems()
        {
            var orderItems = await context.OrderItems
                .Include(o => o.Order)
                .Include(p => p.Product)
                .ToListAsync();

            return Ok(orderItems);
        }

        // Get OrderItem by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderItem>> GetOrderItem(int id)
        {
            var orderItem = await context.OrderItems
                .Include(o => o.Order)
                .Include(p => p.Product)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (orderItem == null)
                return NotFound(new { Message = "Order item not found" });

            return Ok(orderItem);
        }

        // Create a new OrderItem
        [HttpPost]
        public async Task<IActionResult> CreateOrderItem([FromBody] OrderItem orderItem)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var orderExists = await context.Orders.AnyAsync(o => o.Id == orderItem.OrderId);
            var productExists = await context.Products.AnyAsync(p => p.Id == orderItem.ProductId);

            if (!orderExists)
                return BadRequest(new { Message = "Invalid Order ID" });
            if (!productExists)
                return BadRequest(new { Message = "Invalid Product ID" });

            context.OrderItems.Add(orderItem);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrderItem), new { id = orderItem.Id }, orderItem);
        }

        // Update an existing OrderItem
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrderItem(int id, [FromBody] OrderItem updatedOrderItem)
        {
            if (id != updatedOrderItem.Id)
                return BadRequest("OrderItem ID mismatch");

            var existingOrderItem = await context.OrderItems.FindAsync(id);
            if (existingOrderItem == null)
                return NotFound(new { Message = "Order item not found" });

            existingOrderItem.Quantity = updatedOrderItem.Quantity;
            existingOrderItem.Price = updatedOrderItem.Price;
            existingOrderItem.OrderId = updatedOrderItem.OrderId;
            existingOrderItem.ProductId = updatedOrderItem.ProductId;

            await context.SaveChangesAsync();
            return Ok(existingOrderItem);
        }

        // Delete OrderItem
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderItem(int id)
        {
            var orderItem = await context.OrderItems.FindAsync(id);
            if (orderItem == null)
                return NotFound(new { Message = "Order item not found" });

            context.OrderItems.Remove(orderItem);
            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
