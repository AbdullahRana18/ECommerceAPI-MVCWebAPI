using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceAPI.Data;
using ECommerceAPI.Models;

namespace ECommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext context;

        public CategoriesController(AppDbContext context)
        {
            this.context = context;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await context.Categories.ToListAsync();
            return Ok(categories);
        }


        // GET: api/categories/1

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await context.Categories.FindAsync(id);
            if (category ==null) return NotFound();
            return Ok(category);
        }

        // POST: api/categories
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] Category category)
        {
            context.Categories.Add(category);
            await context.SaveChangesAsync();
            return Ok(category);
        }

        // PUT: api/categories/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
        {
            var existing = await context.Categories.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = category.Name;
            await context.SaveChangesAsync();
            return Ok(category);
        }

        // DELETE: api/categories/1

        [HttpDelete("{id}")]

        public async Task<IActionResult> DeleteCategory(int id)
        {
            var existing = await context.Categories.FindAsync(id);
            if (existing == null) return NotFound();
            context.Categories.Remove(existing);
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
