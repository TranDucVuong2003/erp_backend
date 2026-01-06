using erp_backend.Data;
using erp_backend.Models;
using erp_backend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "Admin")]
    public class ServicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ServicesController> _logger;

        public ServicesController(ApplicationDbContext context, ILogger<ServicesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // L?y danh s?ch t?t c? services
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Service>>> GetServices()
        {
            return await _context.Services
                .Include(s => s.Tax)
                .Include(s => s.CategoryServiceAddons)
                .ToListAsync();
        }

        // L?y services ?ang ho?t ??ng
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<Service>>> GetActiveServices()
        {
            return await _context.Services
                .Include(s => s.Tax)
                .Include(s => s.CategoryServiceAddons)
                .Where(s => s.IsActive)
                .ToListAsync();
        }

        // L?y services theo categoryId
        [HttpGet("by-category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<Service>>> GetServicesByCategoryId(int categoryId)
        {
            return await _context.Services
                .Include(s => s.Tax)
                .Include(s => s.CategoryServiceAddons)
                .Where(s => s.CategoryId == categoryId)
                .ToListAsync();
        }

        // L?y service theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Service>> GetService(int id)
        {
            var service = await _context.Services
                .Include(s => s.Tax)
                .Include(s => s.CategoryServiceAddons)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null)
            {
                return NotFound(new { message = "Kh?ng t?m th?y service" });
            }

            return service;
        }

        // T?o service m?i
        [HttpPost]
        public async Task<ActionResult<Service>> CreateService(Service service)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate price
                if (service.Price < 0)
                {
                    return BadRequest(new { message = "Gi? service ph?i l?n h?n ho?c b?ng 0" });
                }

                // Validate quantity
                if (service.Quantity.HasValue && service.Quantity.Value < 1)
                {
                    return BadRequest(new { message = "S? l??ng ph?i l?n h?n 0" });
                }

                service.CreatedAt = DateTime.UtcNow;
                _context.Services.Add(service);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetService), new { id = service.Id }, service);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi t?o service m?i");
                return StatusCode(500, new { message = "L?i server khi t?o service", error = ex.Message });
            }
        }

        // C?p nh?t service
        [HttpPut("{id}")]
        public async Task<ActionResult<UpdateServiceResponse>> UpdateService(int id, [FromBody] Dictionary<string, object?> updateData)
        {
            try
            {
                var existingService = await _context.Services.FindAsync(id);
                if (existingService == null)
                {
                    return NotFound(new { message = "Kh?ng t?m th?y service" });
                }

                foreach (var kvp in updateData)
                {
                    var propertyName = kvp.Key;
                    var value = kvp.Value?.ToString();

                    switch (propertyName.ToLower())
                    {
                        case "name":
                            if (!string.IsNullOrEmpty(value))
                            {
                                if (value.Length > 200)
                                {
                                    return BadRequest(new { message = "T?n service kh?ng ???c v??t qu? 200 k? t?" });
                                }
                                existingService.Name = value;
                            }
                            break;

                        case "description":
                            if (value != null)
                            {
                                if (!string.IsNullOrWhiteSpace(value) && value.Length > 1000)
                                {
                                    return BadRequest(new { message = "M? t? kh?ng ???c v??t qu? 1000 k? t?" });
                                }
                                existingService.Description = string.IsNullOrWhiteSpace(value) ? null : value;
                            }
                            break;

                        case "price":
                            if (kvp.Value != null)
                            {
                                if (decimal.TryParse(kvp.Value.ToString(), out decimal price))
                                {
                                    if (price < 0)
                                    {
                                        return BadRequest(new { message = "Gi? service ph?i l?n h?n ho?c b?ng 0" });
                                    }
                                    existingService.Price = price;
                                }
                                else
                                {
                                    return BadRequest(new { message = "Gi? service kh?ng h?p l?" });
                                }
                            }
                            break;

                        case "quantity":
                            if (kvp.Value != null)
                            {
                                if (int.TryParse(kvp.Value.ToString(), out int quantity))
                                {
                                    if (quantity < 1)
                                    {
                                        return BadRequest(new { message = "S? l??ng ph?i l?n h?n 0" });
                                    }
                                    existingService.Quantity = quantity;
                                }
                                else
                                {
                                    return BadRequest(new { message = "S? l??ng kh?ng h?p l?" });
                                }
                            }
                            break;

                        case "categoryid":
                            if (kvp.Value != null)
                            {
                                if (int.TryParse(kvp.Value.ToString(), out int categoryId))
                                {
                                    // Verify category exists
                                    var categoryExists = await _context.CategoryServiceAddons.AnyAsync(c => c.Id == categoryId);
                                    if (!categoryExists)
                                    {
                                        return BadRequest(new { message = "Category không t?n t?i" });
                                    }
                                    existingService.CategoryId = categoryId;
                                }
                                else
                                {
                                    return BadRequest(new { message = "CategoryId không h?p l?" });
                                }
                            }
                            break;

                        case "isactive":
                            if (kvp.Value != null)
                            {
                                if (bool.TryParse(kvp.Value.ToString(), out bool isActive))
                                {
                                    existingService.IsActive = isActive;
                                }
                                else
                                {
                                    return BadRequest(new { message = "Gi? tr? IsActive ph?i l? true ho?c false" });
                                }
                            }
                            break;

                        case "notes":
                            if (value != null)
                            {
                                if (!string.IsNullOrWhiteSpace(value) && value.Length > 2000)
                                {
                                    return BadRequest(new { message = "Ghi ch? kh?ng ???c v??t qu? 2000 k? t?" });
                                }
                                existingService.Notes = string.IsNullOrWhiteSpace(value) ? null : value;
                            }
                            break;

                        case "id":
                        case "createdat":
                        case "updatedat":
                        case "category": // Ignore old category field
                            // B? qua c?c tr??ng n?y
                            break;

                        default:
                            // B? qua c?c tr??ng kh?ng ???c h? tr?
                            break;
                    }
                }

                existingService.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var response = new UpdateServiceResponse
                {
                    Message = "C?p nh?t service th?nh c?ng",
                    Service = new ServiceInfo
                    {
                        Id = existingService.Id,
                        Name = existingService.Name,
                        Description = existingService.Description,
                        Price = existingService.Price,
                        Quantity = existingService.Quantity,
                        IsActive = existingService.IsActive,
                        Notes = existingService.Notes
                    },
                    UpdatedAt = existingService.UpdatedAt.Value
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi c?p nh?t service v?i ID: {ServiceId}", id);
                return StatusCode(500, new { message = "L?i server khi c?p nh?t service", error = ex.Message });
            }
        }

        // X?a service
        [HttpDelete("{id}")]
        public async Task<ActionResult<DeleteServiceResponse>> DeleteService(int id)
        {
            try
            {
                var service = await _context.Services.FindAsync(id);
                if (service == null)
                {
                    return NotFound(new { message = "Kh?ng t?m th?y service" });
                }

                var deletedServiceInfo = new ServiceInfo
                {
                    Id = service.Id,
                    Name = service.Name,
                    Description = service.Description,
                    Price = service.Price,
                    Quantity = service.Quantity,
                    IsActive = service.IsActive,
                    Notes = service.Notes
                };

                _context.Services.Remove(service);
                await _context.SaveChangesAsync();

                var response = new DeleteServiceResponse
                {
                    Message = "X?a service th?nh c?ng",
                    DeletedService = deletedServiceInfo,
                    DeletedAt = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi x?a service v?i ID: {ServiceId}", id);
                return StatusCode(500, new { message = "L?i server khi x?a service", error = ex.Message });
            }
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.Id == id);
        }
    }
}