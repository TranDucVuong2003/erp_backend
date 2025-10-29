using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using erp_backend.Data;
using erp_backend.Models;

namespace erp_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaxController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TaxController> _logger;

        public TaxController(ApplicationDbContext context, ILogger<TaxController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Tax
        [HttpGet]
        //[Authorize]
        public async Task<ActionResult<IEnumerable<Tax>>> GetTaxes()
        {
            return await _context.Taxes.ToListAsync();
        }

        // GET: api/Tax/5
        [HttpGet("{id}")]
        //[Authorize]
        public async Task<ActionResult<Tax>> GetTax(int id)
        {
            var tax = await _context.Taxes.FindAsync(id);

            if (tax == null)
            {
                return NotFound(new { message = "Không tìm thấy loại thuế" });
            }

            return tax;
        }

        // POST: api/Tax
        [HttpPost]
        //[Authorize]
        public async Task<ActionResult<Tax>> CreateTax(Tax tax)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate rate
                if (tax.Rate < 0 || tax.Rate > 100)
                {
                    return BadRequest(new { message = "Tỷ lệ thuế phải từ 0-100%" });
                }

                tax.CreatedAt = DateTime.UtcNow;
                _context.Taxes.Add(tax);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetTax), new { id = tax.Id }, tax);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo thuế mới");
                return StatusCode(500, new { message = "Lỗi server khi tạo thuế", error = ex.Message });
            }
        }

        // PUT: api/Tax/5
        [HttpPut("{id}")]
        //[Authorize]
        public async Task<ActionResult<Tax>> UpdateTax(int id, Tax tax)
        {
            try
            {
                if (id != tax.Id)
                {
                    return BadRequest(new { message = "ID không khớp" });
                }

                // Kiểm tra tax có tồn tại không
                var existingTax = await _context.Taxes.FindAsync(id);
                if (existingTax == null)
                {
                    return NotFound(new { message = "Không tìm thấy loại thuế" });
                }

                // Validate rate
                if (tax.Rate < 0 || tax.Rate > 100)
                {
                    return BadRequest(new { message = "Tỷ lệ thuế phải từ 0-100%" });
                }

                // Cập nhật các trường
                existingTax.Rate = tax.Rate;
                existingTax.Notes = tax.Notes;
                existingTax.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Trả về tax sau khi đã cập nhật
                return Ok(existingTax);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaxExists(id))
                {
                    return NotFound(new { message = "Không tìm thấy loại thuế" });
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật thuế với ID: {TaxId}", id);
                return StatusCode(500, new { message = "Lỗi server khi cập nhật thuế", error = ex.Message });
            }
        }

        // DELETE: api/Tax/5
        [HttpDelete("{id}")]
        //[Authorize]
        public async Task<IActionResult> DeleteTax(int id)
        {
            var tax = await _context.Taxes.FindAsync(id);
            if (tax == null)
            {
                return NotFound(new { message = "Không tìm thấy loại thuế" });
            }

            _context.Taxes.Remove(tax);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TaxExists(int id)
        {
            return _context.Taxes.Any(e => e.Id == id);
        }
    }
}
