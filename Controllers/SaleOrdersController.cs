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
    public class SaleOrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SaleOrdersController> _logger;

        public SaleOrdersController(ApplicationDbContext context, ILogger<SaleOrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // L?y danh sách t?t c? sale orders
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<SaleOrder>>> GetSaleOrders()
        {
            return await _context.SaleOrders.ToListAsync();
        }

        // L?y sale orders theo customer ID
        [HttpGet("by-customer/{customerId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<SaleOrder>>> GetSaleOrdersByCustomer(int customerId)
        {
            return await _context.SaleOrders
                .Where(d => d.CustomerId == customerId)
                .ToListAsync();
        }

        // Th?ng kê sale orders
        [HttpGet("statistics")]
        [Authorize]
        public async Task<ActionResult<object>> GetSaleOrderStatistics()
        {
            var totalSaleOrders = await _context.SaleOrders.CountAsync();
            var totalValue = await _context.SaleOrders.SumAsync(d => d.Value);
            var averageProbability = totalSaleOrders > 0 ? await _context.SaleOrders.AverageAsync(d => d.Probability) : 0;
            
            var saleOrders = await _context.SaleOrders.ToListAsync();
            var probabilityRanges = saleOrders
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
                TotalSaleOrders = totalSaleOrders,
                TotalValue = totalValue,
                AverageProbability = Math.Round(averageProbability, 2),
                ProbabilityRanges = probabilityRanges
            });
        }

        // L?y sale order theo ID
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<SaleOrder>> GetSaleOrder(int id)
        {
            var saleOrder = await _context.SaleOrders.FindAsync(id);

            if (saleOrder == null)
            {
                return NotFound(new { message = "Không tìm th?y sale order" });
            }

            return saleOrder;
        }

        // T?o sale order m?i
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<SaleOrder>> CreateSaleOrder(SaleOrder saleOrder)
        {
            try
            {
                // Ki?m tra model validation
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate customer exists
                var customerExists = await _context.Customers.AnyAsync(c => c.Id == saleOrder.CustomerId);
                if (!customerExists)
                {
                    return BadRequest(new { message = "Customer không t?n t?i" });
                }

                // Validate sale order value
                if (saleOrder.Value < 0)
                {
                    return BadRequest(new { message = "Giá tr? sale order ph?i l?n h?n ho?c b?ng 0" });
                }

                // Validate probability range
                if (saleOrder.Probability < 0 || saleOrder.Probability > 100)
                {
                    return BadRequest(new { message = "Xác su?t ph?i t? 0-100%" });
                }

                // Validate service exists if provided
                if (saleOrder.ServiceId.HasValue && saleOrder.ServiceId > 0)
                {
                    var serviceExists = await _context.Services.AnyAsync(s => s.Id == saleOrder.ServiceId);
                    if (!serviceExists)
                    {
                        return BadRequest(new { message = "Service không t?n t?i" });
                    }
                }

                // Validate addon exists if provided
                if (saleOrder.AddonId.HasValue && saleOrder.AddonId > 0)
                {
                    var addonExists = await _context.Addons.AnyAsync(a => a.Id == saleOrder.AddonId);
                    if (!addonExists)
                    {
                        return BadRequest(new { message = "Addon không t?n t?i" });
                    }
                }

                saleOrder.CreatedAt = DateTime.UtcNow;
                _context.SaleOrders.Add(saleOrder);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetSaleOrder), new { id = saleOrder.Id }, saleOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi t?o sale order m?i");
                return StatusCode(500, new { message = "L?i server khi t?o sale order", error = ex.Message });
            }
        }

        // C?p nh?t sale order
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<UpdateSaleOrderResponse>> UpdateSaleOrder(int id, [FromBody] Dictionary<string, object?> updateData)
        {
            try
            {
                // Ki?m tra xem sale order có t?n t?i không
                var existingSaleOrder = await _context.SaleOrders.FindAsync(id);
                if (existingSaleOrder == null)
                {
                    return NotFound(new { message = "Không tìm th?y sale order" });
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
                                existingSaleOrder.Title = value;
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
                                    existingSaleOrder.CustomerId = customerId;
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
                                if (decimal.TryParse(kvp.Value.ToString(), out decimal saleOrderValue))
                                {
                                    if (saleOrderValue < 0)
                                    {
                                        return BadRequest(new { message = "Giá tr? sale order ph?i l?n h?n ho?c b?ng 0" });
                                    }
                                    existingSaleOrder.Value = saleOrderValue;
                                }
                                else
                                {
                                    return BadRequest(new { message = "Giá tr? sale order không h?p l?" });
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
                                    existingSaleOrder.Probability = probability;
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
                                existingSaleOrder.Notes = string.IsNullOrWhiteSpace(value) ? null : value;
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
                                    existingSaleOrder.ServiceId = serviceId > 0 ? serviceId : null;
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
                                    existingSaleOrder.AddonId = addonId > 0 ? addonId : null;
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
                existingSaleOrder.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // T?o response
                var response = new UpdateSaleOrderResponse
                {
                    Message = "C?p nh?t thông tin sale order thành công",
                    SaleOrder = new SaleOrderInfo
                    {
                        Id = existingSaleOrder.Id,
                        Title = existingSaleOrder.Title,
                        CustomerId = existingSaleOrder.CustomerId,
                        Value = existingSaleOrder.Value,
                        Probability = existingSaleOrder.Probability,
                        Notes = existingSaleOrder.Notes,
                        ServiceId = existingSaleOrder.ServiceId,
                        AddonId = existingSaleOrder.AddonId
                    },
                    UpdatedAt = existingSaleOrder.UpdatedAt.Value
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi c?p nh?t sale order v?i ID: {SaleOrderId}", id);
                return StatusCode(500, new { message = "L?i server khi c?p nh?t sale order", error = ex.Message });
            }
        }

        // C?p nh?t xác su?t sale order
        [HttpPatch("{id}/probability")]
        [Authorize]
        public async Task<IActionResult> UpdateSaleOrderProbability(int id, [FromBody] Dictionary<string, int> request)
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

                var saleOrder = await _context.SaleOrders.FindAsync(id);
                if (saleOrder == null)
                {
                    return NotFound(new { message = "Không tìm th?y sale order" });
                }

                saleOrder.Probability = probability;
                saleOrder.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new 
                { 
                    message = "C?p nh?t xác su?t thành công",
                    id = saleOrder.Id, 
                    probability = saleOrder.Probability,
                    updatedAt = saleOrder.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi c?p nh?t xác su?t sale order v?i ID: {SaleOrderId}", id);
                return StatusCode(500, new { message = "L?i server khi c?p nh?t xác su?t", error = ex.Message });
            }
        }

        // Xóa sale order
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<DeleteSaleOrderResponse>> DeleteSaleOrder(int id)
        {
            try
            {
                var saleOrder = await _context.SaleOrders.FindAsync(id);
                if (saleOrder == null)
                {
                    return NotFound(new { message = "Không tìm th?y sale order" });
                }

                // L?u thông tin sale order tr??c khi xóa ?? tr? v? trong response
                var deletedSaleOrderInfo = new SaleOrderInfo
                {
                    Id = saleOrder.Id,
                    Title = saleOrder.Title,
                    CustomerId = saleOrder.CustomerId,
                    Value = saleOrder.Value,
                    Probability = saleOrder.Probability,
                    Notes = saleOrder.Notes,
                    ServiceId = saleOrder.ServiceId,
                    AddonId = saleOrder.AddonId
                };

                _context.SaleOrders.Remove(saleOrder);
                await _context.SaveChangesAsync();

                // T?o response
                var response = new DeleteSaleOrderResponse
                {
                    Message = "Xóa sale order thành công",
                    DeletedSaleOrder = deletedSaleOrderInfo,
                    DeletedAt = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi xóa sale order v?i ID: {SaleOrderId}", id);
                return StatusCode(500, new { message = "L?i server khi xóa sale order", error = ex.Message });
            }
        }

        private bool SaleOrderExists(int id)
        {
            return _context.SaleOrders.Any(e => e.Id == id);
        }
    }
}