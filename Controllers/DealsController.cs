using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using erp_backend.Data;
using erp_backend.Models;
using erp_backend.Models.DTOs;

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

        // L?y danh sách t?t c? deals
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Deal>>> GetDeals()
        {
            return await _context.Deals.ToListAsync();
        }

        // L?y deals theo customer ID
        [HttpGet("by-customer/{customerId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Deal>>> GetDealsByCustomer(int customerId)
        {
            return await _context.Deals
                .Where(d => d.CustomerId == customerId)
                .ToListAsync();
        }

        // Th?ng kê deals
        [HttpGet("statistics")]
        [Authorize]
        public async Task<ActionResult<object>> GetDealStatistics()
        {
            var totalDeals = await _context.Deals.CountAsync();
            var totalValue = await _context.Deals.SumAsync(d => d.Value);
            var averageProbability = totalDeals > 0 ? await _context.Deals.AverageAsync(d => d.Probability) : 0;
            
            var deals = await _context.Deals.ToListAsync();
            var probabilityRanges = deals
                .GroupBy(d => d.Probability switch
                {
                    >= 0 and <= 25 => "Low (0-25%)",
                    > 25 and <= 50 => "Medium (26-50%)",
                    > 50 and <= 75 => "High (51-75%)",
                    > 75 and <= 100 => "Very High (76-100%)",
                    _ => "Unknown"
                })
                .Select(g => new
                {
                    ProbabilityRange = g.Key,
                    Count = g.Count(),
                    TotalValue = g.Sum(d => d.Value)
                })
                .ToList();

            return Ok(new
            {
                TotalDeals = totalDeals,
                TotalValue = totalValue,
                AverageProbability = Math.Round(averageProbability, 2),
                ProbabilityRanges = probabilityRanges
            });
        }

        // L?y deal theo ID
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Deal>> GetDeal(int id)
        {
            var deal = await _context.Deals.FindAsync(id);

            if (deal == null)
            {
                return NotFound(new { message = "Không tìm th?y deal" });
            }

            return deal;
        }

        // T?o deal m?i
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Deal>> CreateDeal(Deal deal)
        {
            try
            {
                // Ki?m tra model validation
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate customer exists
                var customerExists = await _context.Customers.AnyAsync(c => c.Id == deal.CustomerId);
                if (!customerExists)
                {
                    return BadRequest(new { message = "Customer không t?n t?i" });
                }

                // Validate deal value
                if (deal.Value < 0)
                {
                    return BadRequest(new { message = "Giá tr? deal ph?i l?n h?n ho?c b?ng 0" });
                }

                // Validate probability range
                if (deal.Probability < 0 || deal.Probability > 100)
                {
                    return BadRequest(new { message = "Xác su?t ph?i t? 0-100%" });
                }

                // Validate service exists if provided
                if (deal.ServiceId.HasValue && deal.ServiceId > 0)
                {
                    var serviceExists = await _context.Services.AnyAsync(s => s.Id == deal.ServiceId);
                    if (!serviceExists)
                    {
                        return BadRequest(new { message = "Service không t?n t?i" });
                    }
                }

                // Validate addon exists if provided
                if (deal.AddonId.HasValue && deal.AddonId > 0)
                {
                    var addonExists = await _context.Addons.AnyAsync(a => a.Id == deal.AddonId);
                    if (!addonExists)
                    {
                        return BadRequest(new { message = "Addon không t?n t?i" });
                    }
                }

                deal.CreatedAt = DateTime.UtcNow;
                _context.Deals.Add(deal);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetDeal), new { id = deal.Id }, deal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi t?o deal m?i");
                return StatusCode(500, new { message = "L?i server khi t?o deal", error = ex.Message });
            }
        }

        // C?p nh?t deal
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<UpdateDealResponse>> UpdateDeal(int id, [FromBody] Dictionary<string, object?> updateData)
        {
            try
            {
                // Ki?m tra xem deal có t?n t?i không
                var existingDeal = await _context.Deals.FindAsync(id);
                if (existingDeal == null)
                {
                    return NotFound(new { message = "Không tìm th?y deal" });
                }

                // C?p nh?t t?ng tr??ng n?u có trong request
                foreach (var kvp in updateData)
                {
                    var propertyName = kvp.Key;
                    var value = kvp.Value?.ToString();

                    switch (propertyName.ToLower())
                    {
                        case "title":
                            if (!string.IsNullOrEmpty(value))
                            {
                                if (value.Length > 255)
                                {
                                    return BadRequest(new { message = "Tiêu ?? không ???c v??t quá 255 ký t?" });
                                }
                                existingDeal.Title = value;
                            }
                            break;

                        case "customerid":
                            if (kvp.Value != null)
                            {
                                if (int.TryParse(kvp.Value.ToString(), out int customerId))
                                {
                                    // Validate customer exists
                                    var customerExists = await _context.Customers.AnyAsync(c => c.Id == customerId);
                                    if (!customerExists)
                                    {
                                        return BadRequest(new { message = "Customer không t?n t?i" });
                                    }
                                    existingDeal.CustomerId = customerId;
                                }
                                else
                                {
                                    return BadRequest(new { message = "Customer ID không h?p l?" });
                                }
                            }
                            break;

                        case "value":
                            if (kvp.Value != null)
                            {
                                if (decimal.TryParse(kvp.Value.ToString(), out decimal dealValue))
                                {
                                    if (dealValue < 0)
                                    {
                                        return BadRequest(new { message = "Giá tr? deal ph?i l?n h?n ho?c b?ng 0" });
                                    }
                                    existingDeal.Value = dealValue;
                                }
                                else
                                {
                                    return BadRequest(new { message = "Giá tr? deal không h?p l?" });
                                }
                            }
                            break;

                        case "probability":
                            if (kvp.Value != null)
                            {
                                if (int.TryParse(kvp.Value.ToString(), out int probability))
                                {
                                    if (probability < 0 || probability > 100)
                                    {
                                        return BadRequest(new { message = "Xác su?t ph?i t? 0-100%" });
                                    }
                                    existingDeal.Probability = probability;
                                }
                                else
                                {
                                    return BadRequest(new { message = "Xác su?t không h?p l?" });
                                }
                            }
                            break;

                        case "notes":
                            if (value != null)
                            {
                                if (!string.IsNullOrWhiteSpace(value) && value.Length > 2000)
                                {
                                    return BadRequest(new { message = "Ghi chú không ???c v??t quá 2000 ký t?" });
                                }
                                existingDeal.Notes = string.IsNullOrWhiteSpace(value) ? null : value;
                            }
                            break;

                        case "serviceid":
                            if (kvp.Value != null)
                            {
                                if (int.TryParse(kvp.Value.ToString(), out int serviceId))
                                {
                                    // Validate service exists if not null
                                    if (serviceId > 0)
                                    {
                                        var serviceExists = await _context.Services.AnyAsync(s => s.Id == serviceId);
                                        if (!serviceExists)
                                        {
                                            return BadRequest(new { message = "Service không t?n t?i" });
                                        }
                                    }
                                    existingDeal.ServiceId = serviceId > 0 ? serviceId : null;
                                }
                                else
                                {
                                    return BadRequest(new { message = "Service ID không h?p l?" });
                                }
                            }
                            break;

                        case "addonid":
                            if (kvp.Value != null)
                            {
                                if (int.TryParse(kvp.Value.ToString(), out int addonId))
                                {
                                    // Validate addon exists if not null
                                    if (addonId > 0)
                                    {
                                        var addonExists = await _context.Addons.AnyAsync(a => a.Id == addonId);
                                        if (!addonExists)
                                        {
                                            return BadRequest(new { message = "Addon không t?n t?i" });
                                        }
                                    }
                                    existingDeal.AddonId = addonId > 0 ? addonId : null;
                                }
                                else
                                {
                                    return BadRequest(new { message = "Addon ID không h?p l?" });
                                }
                            }
                            break;

                        case "id":
                        case "createdat":
                        case "updatedat":
                            // B? qua các tr??ng này vì chúng ???c qu?n lý t? ??ng
                            break;

                        default:
                            // B? qua các tr??ng không ???c h? tr?
                            break;
                    }
                }

                // C?p nh?t th?i gian
                existingDeal.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // T?o response
                var response = new UpdateDealResponse
                {
                    Message = "C?p nh?t thông tin deal thành công",
                    Deal = new DealInfo
                    {
                        Id = existingDeal.Id,
                        Title = existingDeal.Title,
                        CustomerId = existingDeal.CustomerId,
                        Value = existingDeal.Value,
                        Probability = existingDeal.Probability,
                        Notes = existingDeal.Notes,
                        ServiceId = existingDeal.ServiceId,
                        AddonId = existingDeal.AddonId
                    },
                    UpdatedAt = existingDeal.UpdatedAt.Value
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi c?p nh?t deal v?i ID: {DealId}", id);
                return StatusCode(500, new { message = "L?i server khi c?p nh?t deal", error = ex.Message });
            }
        }

        // C?p nh?t xác su?t deal
        [HttpPatch("{id}/probability")]
        [Authorize]
        public async Task<IActionResult> UpdateDealProbability(int id, [FromBody] Dictionary<string, int> request)
        {
            try
            {
                if (!request.ContainsKey("probability"))
                {
                    return BadRequest(new { message = "Thi?u tr??ng probability" });
                }

                var probability = request["probability"];

                if (probability < 0 || probability > 100)
                {
                    return BadRequest(new { message = "Xác su?t ph?i t? 0-100%" });
                }

                var deal = await _context.Deals.FindAsync(id);
                if (deal == null)
                {
                    return NotFound(new { message = "Không tìm th?y deal" });
                }

                deal.Probability = probability;
                deal.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new 
                { 
                    message = "C?p nh?t xác su?t thành công",
                    id = deal.Id, 
                    probability = deal.Probability,
                    updatedAt = deal.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi c?p nh?t xác su?t deal v?i ID: {DealId}", id);
                return StatusCode(500, new { message = "L?i server khi c?p nh?t xác su?t", error = ex.Message });
            }
        }

        // Xóa deal
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<DeleteDealResponse>> DeleteDeal(int id)
        {
            try
            {
                var deal = await _context.Deals.FindAsync(id);
                if (deal == null)
                {
                    return NotFound(new { message = "Không tìm th?y deal" });
                }

                // L?u thông tin deal tr??c khi xóa ?? tr? v? trong response
                var deletedDealInfo = new DealInfo
                {
                    Id = deal.Id,
                    Title = deal.Title,
                    CustomerId = deal.CustomerId,
                    Value = deal.Value,
                    Probability = deal.Probability,
                    Notes = deal.Notes,
                    ServiceId = deal.ServiceId,
                    AddonId = deal.AddonId
                };

                _context.Deals.Remove(deal);
                await _context.SaveChangesAsync();

                // T?o response
                var response = new DeleteDealResponse
                {
                    Message = "Xóa deal thành công",
                    DeletedDeal = deletedDealInfo,
                    DeletedAt = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi xóa deal v?i ID: {DealId}", id);
                return StatusCode(500, new { message = "L?i server khi xóa deal", error = ex.Message });
            }
        }

        private bool DealExists(int id)
        {
            return _context.Deals.Any(e => e.Id == id);
        }
    }
}