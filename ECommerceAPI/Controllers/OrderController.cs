using ECommerceAPI.Data;
using ECommerceAPI.Dtos;
using ECommerceAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext context;

        public OrdersController(AppDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    CreatedAt = o.Date,
                    Items = o.OrderItems.Select(i => new OrderItemDto
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity
                    }).ToList()
                }).ToListAsync();

            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.Id == id)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    CreatedAt = o.Date,
                    Items = o.OrderItems.Select(i => new OrderItemDto
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity
                    }).ToList()
                }).FirstOrDefaultAsync();

            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            if (dto == null || dto.OrderItems == null || !dto.OrderItems.Any())
                return BadRequest("Order must have at least one item.");

            var userExists = await context.Users.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists) return NotFound("User not found.");

            var order = new Order
            {
                UserId = dto.UserId,
                Date = DateTime.UtcNow,
                OrderItems = dto.OrderItems.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = context.Products
                        .Where(p => p.Id == i.ProductId)
                        .Select(p => p.Price)
                        .FirstOrDefault() * i.Quantity
                }).ToList()
            };

            order.TotalAmount = order.OrderItems.Sum(i => i.Price);

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            return Ok(new { order.Id, order.TotalAmount });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] CreateOrderDto dto)
        {
            var existing = await context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (existing == null) return NotFound();

            existing.UserId = dto.UserId;
            existing.Date = DateTime.UtcNow;
            existing.OrderItems.Clear();

            foreach (var item in dto.OrderItems)
            {
                existing.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = context.Products
                        .Where(p => p.Id == item.ProductId)
                        .Select(p => p.Price)
                        .FirstOrDefault() * item.Quantity
                });
            }

            existing.TotalAmount = existing.OrderItems.Sum(i => i.Price);

            await context.SaveChangesAsync();
            return Ok(new { existing.Id, existing.TotalAmount });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var existing = await context.Orders.FindAsync(id);
            if (existing == null) return NotFound();

            context.Orders.Remove(existing);
            await context.SaveChangesAsync();
            return NoContent();
        }

        //Get All Users (only for Admin)



    }
}
