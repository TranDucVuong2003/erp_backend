using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using erp_backend.Data;
using erp_backend.Models;
using erp_backend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace erp_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class TicketCategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TicketCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TicketCategories
        [HttpGet]
        public async Task<ActionResult<object>> GetTicketCategories()
        {
            var categories = await _context.TicketCategories
                .OrderBy(tc => tc.Name)
                .ToListAsync();

            return Ok(new 
            { 
                message = "L?y danh sách danh m?c thành công!",
                data = categories,
                count = categories.Count,
                timestamp = DateTime.UtcNow
            });
        }

        // GET: api/TicketCategories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetTicketCategory(int id)
        {
            var ticketCategory = await _context.TicketCategories.FindAsync(id);

            if (ticketCategory == null)
            {
                return NotFound(new { message = "Danh m?c không t?n t?i." });
            }

            return Ok(new 
            { 
                message = "L?y thông tin danh m?c thành công!",
                data = ticketCategory,
                timestamp = DateTime.UtcNow
            });
        }

        // PUT: api/TicketCategories/5
        [HttpPut("{id}")]
        public async Task<ActionResult<object>> PutTicketCategory(int id, TicketCategory ticketCategory)
        {
            if (id != ticketCategory.Id)
            {
                return BadRequest(new { message = "ID không kh?p v?i d? li?u g?i lên." });
            }

            // Check if category with this name already exists (excluding current category)
            var existingCategory = await _context.TicketCategories
                .FirstOrDefaultAsync(tc => tc.Name == ticketCategory.Name && tc.Id != id);
            
            if (existingCategory != null)
            {
                return BadRequest(new { message = "Tên danh m?c ?ã t?n t?i." });
            }

            ticketCategory.UpdatedAt = DateTime.UtcNow;
            _context.Entry(ticketCategory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketCategoryExists(id))
                {
                    return NotFound(new { message = "Danh m?c không t?n t?i." });
                }
                else
                {
                    throw;
                }
            }

            // Reload the updated entity to get fresh data
            await _context.Entry(ticketCategory).ReloadAsync();

            return Ok(new 
            { 
                message = "C?p nh?t danh m?c thành công!",
                data = ticketCategory,
                timestamp = DateTime.UtcNow
            });
        }

        // PATCH: api/TicketCategories/5
        [HttpPatch("{id}")]
        public async Task<ActionResult<object>> PatchTicketCategory(int id, [FromBody] PatchTicketCategoryDto patchDto)
        {
            var ticketCategory = await _context.TicketCategories.FindAsync(id);
            if (ticketCategory == null)
            {
                return NotFound(new { message = "Danh m?c không t?n t?i." });
            }

            // Only update name if provided
            if (!string.IsNullOrEmpty(patchDto.Name))
            {
                // Check for duplicate name
                var existingCategory = await _context.TicketCategories
                    .FirstOrDefaultAsync(tc => tc.Name == patchDto.Name && tc.Id != id);
                
                if (existingCategory != null)
                {
                    return BadRequest(new { message = "Tên danh m?c ?ã t?n t?i." });
                }

                ticketCategory.Name = patchDto.Name;
            }

            ticketCategory.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketCategoryExists(id))
                {
                    return NotFound(new { message = "Danh m?c không t?n t?i." });
                }
                else
                {
                    throw;
                }
            }

            // Reload the updated entity
            await _context.Entry(ticketCategory).ReloadAsync();

            return Ok(new 
            { 
                message = "C?p nh?t danh m?c thành công!",
                data = ticketCategory,
                timestamp = DateTime.UtcNow
            });
        }

        // POST: api/TicketCategories
        [HttpPost]
        public async Task<ActionResult<object>> PostTicketCategory(TicketCategory ticketCategory)
        {
            // Check if category with this name already exists
            var existingCategory = await _context.TicketCategories
                .FirstOrDefaultAsync(tc => tc.Name == ticketCategory.Name);
            
            if (existingCategory != null)
            {
                return BadRequest(new { message = "Tên danh m?c ?ã t?n t?i." });
            }

            ticketCategory.CreatedAt = DateTime.UtcNow;
            _context.TicketCategories.Add(ticketCategory);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTicketCategory", new { id = ticketCategory.Id }, 
                new 
                { 
                    message = "T?o danh m?c thành công!",
                    data = ticketCategory,
                    timestamp = DateTime.UtcNow
                });
        }

        // DELETE: api/TicketCategories/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteTicketCategory(int id)
        {
            var ticketCategory = await _context.TicketCategories.FindAsync(id);
            if (ticketCategory == null)
            {
                return NotFound(new { message = "Danh m?c không t?n t?i." });
            }

            // Check if there are tickets using this category
            var hasTickets = await _context.Tickets.AnyAsync(t => t.CategoryId == id);
            if (hasTickets)
            {
                return BadRequest(new { message = "Không th? xóa danh m?c ?ang ???c s? d?ng b?i ticket." });
            }

            _context.TicketCategories.Remove(ticketCategory);
            await _context.SaveChangesAsync();

            return Ok(new 
            { 
                message = "Xóa danh m?c thành công!",
                deletedCategory = new { id = ticketCategory.Id, name = ticketCategory.Name },
                timestamp = DateTime.UtcNow
            });
        }

        private bool TicketCategoryExists(int id)
        {
            return _context.TicketCategories.Any(e => e.Id == id);
        }
    }
}