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
    [Authorize]
    public class SaleKpiTargetsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SaleKpiTargetsController> _logger;

        public SaleKpiTargetsController(ApplicationDbContext context, ILogger<SaleKpiTargetsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả KPI targets (response gọn)
        /// </summary>
        /// <returns>Danh sách SaleKpiTargetListDto</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SaleKpiTargetListDto>>> GetSaleKpiTargets()
        {
            try
            {
                var kpiTargets = await _context.SaleKpiTargets
                    .Include(k => k.SaleUser)
                    .Include(k => k.AssignedByUser)
                    .OrderByDescending(k => k.Year)
                    .ThenByDescending(k => k.Month)
                    .ThenBy(k => k.SaleUser!.Name)
                    .Select(k => new SaleKpiTargetListDto
                    {
                        Id = k.Id,
                        UserId = k.UserId,
                        SaleUserName = k.SaleUser != null ? k.SaleUser.Name : "",
                        SaleUserEmail = k.SaleUser != null ? k.SaleUser.Email : "",
                        Month = k.Month,
                        Year = k.Year,
                        TargetAmount = k.TargetAmount,
                        AssignedByUserId = k.AssignedByUserId,
                        AssignedByUserName = k.AssignedByUser != null ? k.AssignedByUser.Name : "",
                        AssignedAt = k.AssignedAt,
                        Notes = k.Notes,
                        IsActive = k.IsActive,
                        CreatedAt = k.CreatedAt
                    })
                    .ToListAsync();

                return Ok(kpiTargets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách KPI Target");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách KPI Target" });
            }
        }

        /// <summary>
        /// Lấy chi tiết KPI target theo ID
        /// </summary>
        /// <param name="id">ID của SaleKpiTarget</param>
        /// <returns>Thông tin SaleKpiTargetDetailDto</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<SaleKpiTargetDetailDto>> GetSaleKpiTarget(int id)
        {
            try
            {
                var kpiTarget = await _context.SaleKpiTargets
                    .Include(k => k.SaleUser)
                        .ThenInclude(u => u!.Department)
                    .Include(k => k.SaleUser)
                        .ThenInclude(u => u!.Position)
                    .Include(k => k.AssignedByUser)
                        .ThenInclude(u => u!.Department)
                    .Include(k => k.AssignedByUser)
                        .ThenInclude(u => u!.Position)
                    .Where(k => k.Id == id)
                    .Select(k => new SaleKpiTargetDetailDto
                    {
                        Id = k.Id,
                        UserId = k.UserId,
                        SaleUser = k.SaleUser != null ? new KpiUserDto
                        {
                            Id = k.SaleUser.Id,
                            Name = k.SaleUser.Name,
                            Email = k.SaleUser.Email,
                            PhoneNumber = k.SaleUser.PhoneNumber,
                            DepartmentId = k.SaleUser.DepartmentId,
                            DepartmentName = k.SaleUser.Department != null ? k.SaleUser.Department.Name : null,
                            PositionId = k.SaleUser.PositionId,
                            PositionName = k.SaleUser.Position != null ? k.SaleUser.Position.PositionName : null
                        } : null,
                        Month = k.Month,
                        Year = k.Year,
                        TargetAmount = k.TargetAmount,
                        AssignedByUserId = k.AssignedByUserId,
                        AssignedByUser = k.AssignedByUser != null ? new KpiUserDto
                        {
                            Id = k.AssignedByUser.Id,
                            Name = k.AssignedByUser.Name,
                            Email = k.AssignedByUser.Email,
                            PhoneNumber = k.AssignedByUser.PhoneNumber,
                            DepartmentId = k.AssignedByUser.DepartmentId,
                            DepartmentName = k.AssignedByUser.Department != null ? k.AssignedByUser.Department.Name : null,
                            PositionId = k.AssignedByUser.PositionId,
                            PositionName = k.AssignedByUser.Position != null ? k.AssignedByUser.Position.PositionName : null
                        } : null,
                        AssignedAt = k.AssignedAt,
                        Notes = k.Notes,
                        IsActive = k.IsActive,
                        CreatedAt = k.CreatedAt,
                        UpdatedAt = k.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (kpiTarget == null)
                {
                    return NotFound(new { message = $"Không tìm thấy KPI Target với ID {id}" });
                }

                return Ok(kpiTarget);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy KPI Target ID {Id}", id);
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thông tin KPI Target" });
            }
        }

        /// <summary>
        /// Lấy KPI target theo UserId, Month, Year
        /// </summary>
        /// <param name="userId">ID của Sale User</param>
        /// <param name="month">Tháng (1-12)</param>
        /// <param name="year">Năm</param>
        /// <returns>Thông tin SaleKpiTargetDetailDto</returns>
        [HttpGet("by-user-period")]
        public async Task<ActionResult<SaleKpiTargetDetailDto>> GetKpiTargetByUserPeriod(
            [FromQuery] int userId, 
            [FromQuery] int month, 
            [FromQuery] int year)
        {
            try
            {
                if (month < 1 || month > 12)
                {
                    return BadRequest(new { message = "Tháng phải từ 1 đến 12" });
                }

                if (year < 2020 || year > 2100)
                {
                    return BadRequest(new { message = "Năm không hợp lệ" });
                }

                var kpiTarget = await _context.SaleKpiTargets
                    .Include(k => k.SaleUser)
                        .ThenInclude(u => u!.Department)
                    .Include(k => k.SaleUser)
                        .ThenInclude(u => u!.Position)
                    .Include(k => k.AssignedByUser)
                    .Where(k => k.UserId == userId 
                        && k.Month == month 
                        && k.Year == year
                        && k.IsActive)
                    .Select(k => new SaleKpiTargetDetailDto
                    {
                        Id = k.Id,
                        UserId = k.UserId,
                        SaleUser = k.SaleUser != null ? new KpiUserDto
                        {
                            Id = k.SaleUser.Id,
                            Name = k.SaleUser.Name,
                            Email = k.SaleUser.Email,
                            PhoneNumber = k.SaleUser.PhoneNumber,
                            DepartmentId = k.SaleUser.DepartmentId,
                            DepartmentName = k.SaleUser.Department != null ? k.SaleUser.Department.Name : null,
                            PositionId = k.SaleUser.PositionId,
                            PositionName = k.SaleUser.Position != null ? k.SaleUser.Position.PositionName : null
                        } : null,
                        Month = k.Month,
                        Year = k.Year,
                        TargetAmount = k.TargetAmount,
                        AssignedByUserId = k.AssignedByUserId,
                        AssignedByUser = k.AssignedByUser != null ? new KpiUserDto
                        {
                            Id = k.AssignedByUser.Id,
                            Name = k.AssignedByUser.Name,
                            Email = k.AssignedByUser.Email,
                            PhoneNumber = k.AssignedByUser.PhoneNumber
                        } : null,
                        AssignedAt = k.AssignedAt,
                        Notes = k.Notes,
                        IsActive = k.IsActive,
                        CreatedAt = k.CreatedAt,
                        UpdatedAt = k.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (kpiTarget == null)
                {
                    return NotFound(new { message = $"Không tìm thấy KPI Target cho User {userId} tháng {month}/{year}" });
                }

                return Ok(kpiTarget);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy KPI Target theo User/Period");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thông tin KPI Target" });
            }
        }

        /// <summary>
        /// Lấy tất cả KPI targets của một user
        /// </summary>
        /// <param name="userId">ID của Sale User</param>
        /// <returns>Danh sách SaleKpiTargetListDto</returns>
        [HttpGet("by-user/{userId}")]
        public async Task<ActionResult<IEnumerable<SaleKpiTargetListDto>>> GetKpiTargetsByUser(int userId)
        {
            try
            {
                var kpiTargets = await _context.SaleKpiTargets
                    .Include(k => k.SaleUser)
                    .Include(k => k.AssignedByUser)
                    .Where(k => k.UserId == userId)
                    .OrderByDescending(k => k.Year)
                    .ThenByDescending(k => k.Month)
                    .Select(k => new SaleKpiTargetListDto
                    {
                        Id = k.Id,
                        UserId = k.UserId,
                        SaleUserName = k.SaleUser != null ? k.SaleUser.Name : "",
                        SaleUserEmail = k.SaleUser != null ? k.SaleUser.Email : "",
                        Month = k.Month,
                        Year = k.Year,
                        TargetAmount = k.TargetAmount,
                        AssignedByUserId = k.AssignedByUserId,
                        AssignedByUserName = k.AssignedByUser != null ? k.AssignedByUser.Name : "",
                        AssignedAt = k.AssignedAt,
                        Notes = k.Notes,
                        IsActive = k.IsActive,
                        CreatedAt = k.CreatedAt
                    })
                    .ToListAsync();

                return Ok(kpiTargets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy KPI Target theo User {UserId}", userId);
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách KPI Target" });
            }
        }

        /// <summary>
        /// Lấy tất cả KPI targets theo tháng/năm
        /// </summary>
        /// <param name="month">Tháng (1-12)</param>
        /// <param name="year">Năm</param>
        /// <returns>Danh sách SaleKpiTargetListDto</returns>
        [HttpGet("by-period")]
        public async Task<ActionResult<IEnumerable<SaleKpiTargetListDto>>> GetKpiTargetsByPeriod(
            [FromQuery] int month, 
            [FromQuery] int year)
        {
            try
            {
                if (month < 1 || month > 12)
                {
                    return BadRequest(new { message = "Tháng phải từ 1 đến 12" });
                }

                if (year < 2020 || year > 2100)
                {
                    return BadRequest(new { message = "Năm không hợp lệ" });
                }

                var kpiTargets = await _context.SaleKpiTargets
                    .Include(k => k.SaleUser)
                    .Include(k => k.AssignedByUser)
                    .Where(k => k.Month == month && k.Year == year)
                    .OrderBy(k => k.SaleUser!.Name)
                    .Select(k => new SaleKpiTargetListDto
                    {
                        Id = k.Id,
                        UserId = k.UserId,
                        SaleUserName = k.SaleUser != null ? k.SaleUser.Name : "",
                        SaleUserEmail = k.SaleUser != null ? k.SaleUser.Email : "",
                        Month = k.Month,
                        Year = k.Year,
                        TargetAmount = k.TargetAmount,
                        AssignedByUserId = k.AssignedByUserId,
                        AssignedByUserName = k.AssignedByUser != null ? k.AssignedByUser.Name : "",
                        AssignedAt = k.AssignedAt,
                        Notes = k.Notes,
                        IsActive = k.IsActive,
                        CreatedAt = k.CreatedAt
                    })
                    .ToListAsync();

                return Ok(kpiTargets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy KPI Target theo Period");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách KPI Target" });
            }
        }

        /// <summary>
        /// Tạo mới KPI target
        /// </summary>
        /// <param name="request">Thông tin CreateSaleKpiTargetRequest</param>
        /// <returns>SaleKpiTargetDetailDto vừa tạo</returns>
        [HttpPost]
        public async Task<ActionResult<SaleKpiTargetDetailDto>> CreateSaleKpiTarget(CreateSaleKpiTargetRequest request)
        {
            try
            {
                // Validate
                if (request.Month < 1 || request.Month > 12)
                {
                    return BadRequest(new { message = "Tháng phải từ 1 đến 12" });
                }

                if (request.Year < 2020 || request.Year > 2100)
                {
                    return BadRequest(new { message = "Năm không hợp lệ" });
                }

                if (request.TargetAmount <= 0)
                {
                    return BadRequest(new { message = "KPI Target phải lớn hơn 0" });
                }

                // Kiểm tra User có tồn tại không
                var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
                if (!userExists)
                {
                    return BadRequest(new { message = "User không tồn tại" });
                }

                // Kiểm tra AssignedByUser có tồn tại không
                var assignedByUserExists = await _context.Users.AnyAsync(u => u.Id == request.AssignedByUserId);
                if (!assignedByUserExists)
                {
                    return BadRequest(new { message = "Admin giao KPI không tồn tại" });
                }

                // Kiểm tra đã có KPI target cho user này trong tháng/năm này chưa
                var existingKpi = await _context.SaleKpiTargets
                    .AnyAsync(k => k.UserId == request.UserId 
                        && k.Month == request.Month 
                        && k.Year == request.Year
                        && k.IsActive);

                if (existingKpi)
                {
                    return BadRequest(new { message = $"Đã có KPI Target cho User này trong tháng {request.Month}/{request.Year}" });
                }

                var kpiTarget = new SaleKpiTarget
                {
                    UserId = request.UserId,
                    Month = request.Month,
                    Year = request.Year,
                    TargetAmount = request.TargetAmount,
                    AssignedByUserId = request.AssignedByUserId,
                    Notes = request.Notes,
                    IsActive = request.IsActive,
                    AssignedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                };

                _context.SaleKpiTargets.Add(kpiTarget);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã tạo KPI Target mới: ID {Id}, User {UserId}, {Month}/{Year}, Target {Target}",
                    kpiTarget.Id, kpiTarget.UserId, kpiTarget.Month, kpiTarget.Year, kpiTarget.TargetAmount);

                // Lấy lại với DTO
                var createdKpi = await _context.SaleKpiTargets
                    .Include(k => k.SaleUser)
                        .ThenInclude(u => u!.Department)
                    .Include(k => k.SaleUser)
                        .ThenInclude(u => u!.Position)
                    .Include(k => k.AssignedByUser)
                    .Where(k => k.Id == kpiTarget.Id)
                    .Select(k => new SaleKpiTargetDetailDto
                    {
                        Id = k.Id,
                        UserId = k.UserId,
                        SaleUser = k.SaleUser != null ? new KpiUserDto
                        {
                            Id = k.SaleUser.Id,
                            Name = k.SaleUser.Name,
                            Email = k.SaleUser.Email,
                            PhoneNumber = k.SaleUser.PhoneNumber,
                            DepartmentId = k.SaleUser.DepartmentId,
                            DepartmentName = k.SaleUser.Department != null ? k.SaleUser.Department.Name : null,
                            PositionId = k.SaleUser.PositionId,
                            PositionName = k.SaleUser.Position != null ? k.SaleUser.Position.PositionName : null
                        } : null,
                        Month = k.Month,
                        Year = k.Year,
                        TargetAmount = k.TargetAmount,
                        AssignedByUserId = k.AssignedByUserId,
                        AssignedByUser = k.AssignedByUser != null ? new KpiUserDto
                        {
                            Id = k.AssignedByUser.Id,
                            Name = k.AssignedByUser.Name,
                            Email = k.AssignedByUser.Email,
                            PhoneNumber = k.AssignedByUser.PhoneNumber
                        } : null,
                        AssignedAt = k.AssignedAt,
                        Notes = k.Notes,
                        IsActive = k.IsActive,
                        CreatedAt = k.CreatedAt,
                        UpdatedAt = k.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                return CreatedAtAction(nameof(GetSaleKpiTarget), new { id = kpiTarget.Id }, createdKpi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo KPI Target mới");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi tạo KPI Target mới" });
            }
        }

        /// <summary>
        /// Cập nhật KPI target
        /// </summary>
        /// <param name="id">ID của SaleKpiTarget</param>
        /// <param name="request">Thông tin cập nhật UpdateSaleKpiTargetRequest</param>
        /// <returns>NoContent nếu thành công</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSaleKpiTarget(int id, UpdateSaleKpiTargetRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest(new { message = "ID không khớp" });
            }

            try
            {
                var existingKpi = await _context.SaleKpiTargets.FindAsync(id);
                if (existingKpi == null)
                {
                    return NotFound(new { message = $"Không tìm thấy KPI Target với ID {id}" });
                }

                // Validate
                if (request.Month < 1 || request.Month > 12)
                {
                    return BadRequest(new { message = "Tháng phải từ 1 đến 12" });
                }

                if (request.Year < 2020 || request.Year > 2100)
                {
                    return BadRequest(new { message = "Năm không hợp lệ" });
                }

                if (request.TargetAmount <= 0)
                {
                    return BadRequest(new { message = "KPI Target phải lớn hơn 0" });
                }

                // Kiểm tra User có tồn tại không
                var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
                if (!userExists)
                {
                    return BadRequest(new { message = "User không tồn tại" });
                }

                // Kiểm tra trùng lặp (trừ bản ghi hiện tại)
                if (existingKpi.UserId != request.UserId 
                    || existingKpi.Month != request.Month 
                    || existingKpi.Year != request.Year)
                {
                    var duplicateKpi = await _context.SaleKpiTargets
                        .AnyAsync(k => k.Id != id
                            && k.UserId == request.UserId 
                            && k.Month == request.Month 
                            && k.Year == request.Year
                            && k.IsActive);

                    if (duplicateKpi)
                    {
                        return BadRequest(new { message = $"Đã có KPI Target cho User này trong tháng {request.Month}/{request.Year}" });
                    }
                }

                // Update fields
                existingKpi.UserId = request.UserId;
                existingKpi.Month = request.Month;
                existingKpi.Year = request.Year;
                existingKpi.TargetAmount = request.TargetAmount;
                existingKpi.AssignedByUserId = request.AssignedByUserId;
                existingKpi.Notes = request.Notes;
                existingKpi.IsActive = request.IsActive;
                existingKpi.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã cập nhật KPI Target ID {Id}", id);

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await SaleKpiTargetExists(id))
                {
                    return NotFound(new { message = $"Không tìm thấy KPI Target với ID {id}" });
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật KPI Target ID {Id}", id);
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi cập nhật KPI Target" });
            }
        }

        /// <summary>
        /// Xóa KPI target (soft delete - set IsActive = false)
        /// </summary>
        /// <param name="id">ID của SaleKpiTarget</param>
        /// <returns>NoContent nếu thành công</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSaleKpiTarget(int id)
        {
            try
            {
                var kpiTarget = await _context.SaleKpiTargets.FindAsync(id);
                if (kpiTarget == null)
                {
                    return NotFound(new { message = $"Không tìm thấy KPI Target với ID {id}" });
                }

                // Soft delete
                kpiTarget.IsActive = false;
                kpiTarget.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã xóa (soft delete) KPI Target ID {Id}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa KPI Target ID {Id}", id);
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi xóa KPI Target" });
            }
        }

        private async Task<bool> SaleKpiTargetExists(int id)
        {
            return await _context.SaleKpiTargets.AnyAsync(e => e.Id == id);
        }
    }
}
