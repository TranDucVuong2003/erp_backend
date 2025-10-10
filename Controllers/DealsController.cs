using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using erp_backend.Data;
using erp_backend.Models;

namespace erp_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DealsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DealsController> _logger;

        public DealsController(ApplicationDbContext context, ILogger<DealsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Deal>>> GetDeals()
        {
            return await _context.Deals
                .Include(d => d.Customer)
                .Include(d => d.AssignedUser)
                .Include(d => d.CreatedByUser)
                .ToListAsync();
        }

        [HttpGet("by-stage/{stage}")]
        public async Task<ActionResult<IEnumerable<Deal>>> GetDealsByStage(DealStage stage)
        {
            return await _context.Deals
                .Where(d => d.Stage == stage)
                .Include(d => d.Customer)
                .Include(d => d.AssignedUser)
                .ToListAsync();
        }

        [HttpGet("by-customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<Deal>>> GetDealsByCustomer(int customerId)
        {
            return await _context.Deals
                .Where(d => d.CustomerId == customerId)
                .Include(d => d.Customer)
                .Include(d => d.AssignedUser)
                .ToListAsync();
        }

        [HttpGet("assigned-to/{userId}")]
        public async Task<ActionResult<IEnumerable<Deal>>> GetDealsAssignedTo(int userId)
        {
            return await _context.Deals
                .Where(d => d.AssignedTo == userId)
                .Include(d => d.Customer)
                .Include(d => d.AssignedUser)
                .ToListAsync();
        }

        [HttpGet("pipeline")]
        public async Task<ActionResult<object>> GetPipeline()
        {
            var pipeline = await _context.Deals
                .Where(d => d.Stage != DealStage.ClosedWon && d.Stage != DealStage.ClosedLost)
                .GroupBy(d => d.Stage)
                .Select(g => new
                {
                    Stage = g.Key.ToString(),
                    Count = g.Count(),
                    TotalValue = g.Sum(d => d.Value),
                    AverageProbability = g.Average(d => d.Probability)
                })
                .ToListAsync();

            return Ok(pipeline);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Deal>> GetDeal(int id)
        {
            var deal = await _context.Deals
                .Include(d => d.Customer)
                .Include(d => d.AssignedUser)
                .Include(d => d.CreatedByUser)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deal == null)
            {
                return NotFound();
            }

            return deal;
        }

        [HttpPost]
        public async Task<ActionResult<Deal>> CreateDeal(Deal deal)
        {
            // Validate customer exists
            var customerExists = await _context.Customers.AnyAsync(c => c.Id == deal.CustomerId);
            if (!customerExists)
            {
                return BadRequest("Customer không t?n t?i");
            }

            // Validate assigned user exists (if provided)
            if (deal.AssignedTo.HasValue)
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == deal.AssignedTo);
                if (!userExists)
                {
                    return BadRequest("User ???c assign không t?n t?i");
                }
            }

            deal.CreatedAt = DateTime.UtcNow;
            _context.Deals.Add(deal);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDeal), new { id = deal.Id }, deal);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDeal(int id, Deal deal)
        {
            if (id != deal.Id)
            {
                return BadRequest();
            }

            // Validate customer exists
            var customerExists = await _context.Customers.AnyAsync(c => c.Id == deal.CustomerId);
            if (!customerExists)
            {
                return BadRequest("Customer không t?n t?i");
            }

            // Validate assigned user exists (if provided)
            if (deal.AssignedTo.HasValue)
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == deal.AssignedTo);
                if (!userExists)
                {
                    return BadRequest("User ???c assign không t?n t?i");
                }
            }

            deal.UpdatedAt = DateTime.UtcNow;
            _context.Entry(deal).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DealExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPatch("{id}/stage")]
        public async Task<IActionResult> UpdateDealStage(int id, [FromBody] DealStage newStage)
        {
            var deal = await _context.Deals.FindAsync(id);
            if (deal == null)
            {
                return NotFound();
            }

            deal.Stage = newStage;
            deal.UpdatedAt = DateTime.UtcNow;

            // If closing the deal, set actual close date
            if (newStage == DealStage.ClosedWon || newStage == DealStage.ClosedLost)
            {
                deal.ActualCloseDate = DateOnly.FromDateTime(DateTime.Now);
            }

            await _context.SaveChangesAsync();

            return Ok(new { id = deal.Id, stage = deal.Stage.ToString() });
        }

        [HttpPatch("{id}/probability")]
        public async Task<IActionResult> UpdateDealProbability(int id, [FromBody] int probability)
        {
            if (probability < 0 || probability > 100)
            {
                return BadRequest("Xác su?t ph?i t? 0-100%");
            }

            var deal = await _context.Deals.FindAsync(id);
            if (deal == null)
            {
                return NotFound();
            }

            deal.Probability = probability;
            deal.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { id = deal.Id, probability = deal.Probability });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeal(int id)
        {
            var deal = await _context.Deals.FindAsync(id);
            if (deal == null)
            {
                return NotFound();
            }

            _context.Deals.Remove(deal);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DealExists(int id)
        {
            return _context.Deals.Any(e => e.Id == id);
        }
    }
}