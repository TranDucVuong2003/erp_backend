using erp_backend.Data;
using erp_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InsurancePolicyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InsurancePolicyController> _logger;

        public InsurancePolicyController(ApplicationDbContext context, ILogger<InsurancePolicyController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/InsurancePolicy
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<InsurancePolicy>>> GetInsurancePolicies()
        {
            return await _context.InsurancePolicy.ToListAsync();
        }

        // GET: api/InsurancePolicy/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InsurancePolicy>> GetInsurancePolicy(int id)
        {
            var insurancePolicy = await _context.InsurancePolicy.FindAsync(id);

            if (insurancePolicy == null)
            {
                return NotFound(new { message = "Không tìm th?y chính sách b?o hi?m" });
            }

            return insurancePolicy;
        }

        // PUT: api/InsurancePolicy/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInsurancePolicy(int id, InsurancePolicy insurancePolicy)
        {
            if (id != insurancePolicy.Id)
            {
                return BadRequest(new { message = "ID không kh?p" });
            }

            insurancePolicy.UpdatedAt = DateTime.UtcNow;
            _context.Entry(insurancePolicy).State = EntityState.Modified;
            
            // Không cho phép s?a CreatedAt
            _context.Entry(insurancePolicy).Property(x => x.CreatedAt).IsModified = false;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InsurancePolicyExists(id))
                {
                    return NotFound(new { message = "Không tìm th?y chính sách b?o hi?m" });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { message = "C?p nh?t thành công", data = insurancePolicy });
        }

        // POST: api/InsurancePolicy
        [HttpPost]
        public async Task<ActionResult<InsurancePolicy>> PostInsurancePolicy(InsurancePolicy insurancePolicy)
        {
            // Ki?m tra trùng mã Code
            if (await _context.InsurancePolicy.AnyAsync(p => p.Code == insurancePolicy.Code))
            {
                return BadRequest(new { message = $"Mã b?o hi?m '{insurancePolicy.Code}' ?ã t?n t?i" });
            }

            insurancePolicy.CreatedAt = DateTime.UtcNow;
            _context.InsurancePolicy.Add(insurancePolicy);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInsurancePolicy", new { id = insurancePolicy.Id }, insurancePolicy);
        }

        // DELETE: api/InsurancePolicy/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInsurancePolicy(int id)
        {
            var insurancePolicy = await _context.InsurancePolicy.FindAsync(id);
            if (insurancePolicy == null)
            {
                return NotFound(new { message = "Không tìm th?y chính sách b?o hi?m" });
            }

            _context.InsurancePolicy.Remove(insurancePolicy);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa thành công" });
        }

        private bool InsurancePolicyExists(int id)
        {
            return _context.InsurancePolicy.Any(e => e.Id == id);
        }
    }
}
