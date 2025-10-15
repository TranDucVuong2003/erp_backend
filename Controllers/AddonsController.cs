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
    public class AddonsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AddonsController> _logger;

        public AddonsController(ApplicationDbContext context, ILogger<AddonsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // L?y danh s�ch t?t c? addons
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Addon>>> GetAddons()
        {
            return await _context.Addons.ToListAsync();
        }

        // L?y addons ?ang ho?t ??ng
        [HttpGet("active")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Addon>>> GetActiveAddons()
        {
            return await _context.Addons.Where(a => a.IsActive).ToListAsync();
        }

        // L?y addons theo type
        [HttpGet("by-type/{type}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Addon>>> GetAddonsByType(string type)
        {
            return await _context.Addons
                .Where(a => a.Type == type)
                .ToListAsync();
        }

        // L?y addon theo ID
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Addon>> GetAddon(int id)
        {
            var addon = await _context.Addons.FindAsync(id);

            if (addon == null)
            {
                return NotFound(new { message = "Kh�ng t�m th?y addon" });
            }

            return addon;
        }

        // T?o addon m?i
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Addon>> CreateAddon(Addon addon)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate price
                if (addon.Price < 0)
                {
                    return BadRequest(new { message = "Gi� addon ph?i l?n h?n ho?c b?ng 0" });
                }

                // Validate quantity
                if (addon.Quantity < 1)
                {
                    return BadRequest(new { message = "S? l??ng ph?i l?n h?n 0" });
                }

                addon.CreatedAt = DateTime.UtcNow;
                _context.Addons.Add(addon);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAddon), new { id = addon.Id }, addon);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi t?o addon m?i");
                return StatusCode(500, new { message = "L?i server khi t?o addon", error = ex.Message });
            }
        }

        // C?p nh?t addon
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<UpdateAddonResponse>> UpdateAddon(int id, [FromBody] Dictionary<string, object?> updateData)
        {
            try
            {
                var existingAddon = await _context.Addons.FindAsync(id);
                if (existingAddon == null)
                {
                    return NotFound(new { message = "Kh�ng t�m th?y addon" });
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
                                    return BadRequest(new { message = "T�n addon kh�ng ???c v??t qu� 200 k� t?" });
                                }
                                existingAddon.Name = value;
                            }
                            break;

                        case "description":
                            if (value != null)
                            {
                                if (!string.IsNullOrWhiteSpace(value) && value.Length > 1000)
                                {
                                    return BadRequest(new { message = "M� t? kh�ng ???c v??t qu� 1000 k� t?" });
                                }
                                existingAddon.Description = string.IsNullOrWhiteSpace(value) ? null : value;
                            }
                            break;

                        case "price":
                            if (kvp.Value != null)
                            {
                                if (decimal.TryParse(kvp.Value.ToString(), out decimal price))
                                {
                                    if (price < 0)
                                    {
                                        return BadRequest(new { message = "Gi� addon ph?i l?n h?n ho?c b?ng 0" });
                                    }
                                    existingAddon.Price = price;
                                }
                                else
                                {
                                    return BadRequest(new { message = "Gi� addon kh�ng h?p l?" });
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
                                    existingAddon.Quantity = quantity;
                                }
                                else
                                {
                                    return BadRequest(new { message = "S? l??ng kh�ng h?p l?" });
                                }
                            }
                            break;

                        case "type":
                            if (value != null)
                            {
                                if (!string.IsNullOrWhiteSpace(value) && value.Length > 50)
                                {
                                    return BadRequest(new { message = "Type kh�ng ???c v??t qu� 50 k� t?" });
                                }
                                existingAddon.Type = string.IsNullOrWhiteSpace(value) ? null : value;
                            }
                            break;

                        case "isactive":
                            if (kvp.Value != null)
                            {
                                if (bool.TryParse(kvp.Value.ToString(), out bool isActive))
                                {
                                    existingAddon.IsActive = isActive;
                                }
                                else
                                {
                                    return BadRequest(new { message = "Gi� tr? IsActive ph?i l� true ho?c false" });
                                }
                            }
                            break;

                        case "notes":
                            if (value != null)
                            {
                                if (!string.IsNullOrWhiteSpace(value) && value.Length > 2000)
                                {
                                    return BadRequest(new { message = "Ghi ch� kh�ng ???c v??t qu� 2000 k� t?" });
                                }
                                existingAddon.Notes = string.IsNullOrWhiteSpace(value) ? null : value;
                            }
                            break;

                        case "id":
                        case "createdat":
                        case "updatedat":
                            // B? qua c�c tr??ng n�y
                            break;

                        default:
                            // B? qua c�c tr??ng kh�ng ???c h? tr?
                            break;
                    }
                }

                existingAddon.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var response = new UpdateAddonResponse
                {
                    Message = "C?p nh?t addon th�nh c�ng",
                    Addon = new AddonInfo
                    {
                        Id = existingAddon.Id,
                        Name = existingAddon.Name,
                        Description = existingAddon.Description,
                        Price = existingAddon.Price,
                        Quantity = existingAddon.Quantity,
                        Type = existingAddon.Type,
                        IsActive = existingAddon.IsActive,
                        Notes = existingAddon.Notes
                    },
                    UpdatedAt = existingAddon.UpdatedAt.Value
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi c?p nh?t addon v?i ID: {AddonId}", id);
                return StatusCode(500, new { message = "L?i server khi c?p nh?t addon", error = ex.Message });
            }
        }

        // X�a addon
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<DeleteAddonResponse>> DeleteAddon(int id)
        {
            try
            {
                var addon = await _context.Addons.FindAsync(id);
                if (addon == null)
                {
                    return NotFound(new { message = "Kh�ng t�m th?y addon" });
                }

                var deletedAddonInfo = new AddonInfo
                {
                    Id = addon.Id,
                    Name = addon.Name,
                    Description = addon.Description,
                    Price = addon.Price,
                    Quantity = addon.Quantity,
                    Type = addon.Type,
                    IsActive = addon.IsActive,
                    Notes = addon.Notes
                };

                _context.Addons.Remove(addon);
                await _context.SaveChangesAsync();

                var response = new DeleteAddonResponse
                {
                    Message = "X�a addon th�nh c�ng",
                    DeletedAddon = deletedAddonInfo,
                    DeletedAt = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi x�a addon v?i ID: {AddonId}", id);
                return StatusCode(500, new { message = "L?i server khi x�a addon", error = ex.Message });
            }
        }

        private bool AddonExists(int id)
        {
            return _context.Addons.Any(e => e.Id == id);
        }
    }
}