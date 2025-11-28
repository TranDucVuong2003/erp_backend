using erp_backend.Data;
using erp_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserKpiAssignmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserKpiAssignmentsController> _logger;

        public UserKpiAssignmentsController(
            ApplicationDbContext context,
            ILogger<UserKpiAssignmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// L?y danh sách t?t c? UserKpiAssignments
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserKpiAssignment>>> GetUserKpiAssignments(
            [FromQuery] int? userId = null,
            [FromQuery] int? kpiId = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var query = _context.UserKpiAssignments
                    .Include(u => u.User)
                        .ThenInclude(u => u.Department)
                    .Include(u => u.User)
                        .ThenInclude(u => u.Position)
                    .Include(u => u.Kpi)
                        .ThenInclude(k => k.Department)
                    .Include(u => u.Assigner)
                    .AsQueryable();

                // Filter
                if (userId.HasValue)
                    query = query.Where(u => u.UserId == userId.Value);

                if (kpiId.HasValue)
                    query = query.Where(u => u.KpiId == kpiId.Value);

                if (isActive.HasValue)
                    query = query.Where(u => u.IsActive == isActive.Value);

                var assignments = await query.OrderByDescending(u => u.AssignedDate).ToListAsync();

                return Ok(assignments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi l?y danh sách UserKpiAssignments");
                return StatusCode(500, new { message = "L?i server", error = ex.Message });
            }
        }

        /// <summary>
        /// L?y chi ti?t 1 UserKpiAssignment
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserKpiAssignment>> GetUserKpiAssignment(int id)
        {
            try
            {
                var assignment = await _context.UserKpiAssignments
                    .Include(u => u.User)
                        .ThenInclude(u => u.Department)
                    .Include(u => u.User)
                        .ThenInclude(u => u.Position)
                    .Include(u => u.Kpi)
                        .ThenInclude(k => k.Department)
                    .Include(u => u.Kpi)
                        .ThenInclude(k => k.CommissionTiers)
                    .Include(u => u.Assigner)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (assignment == null)
                {
                    return NotFound(new { message = "Không tìm th?y assignment" });
                }

                return Ok(assignment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi l?y UserKpiAssignment {Id}", id);
                return StatusCode(500, new { message = "L?i server", error = ex.Message });
            }
        }

        /// <summary>
        /// Gán KPI cho User (Admin only)
        /// POST /api/UserKpiAssignments
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<UserKpiAssignment>> CreateUserKpiAssignment(
            [FromBody] UserKpiAssignment assignment)
        {
            try
            {
                // Validate User exists
                var userExists = await _context.Users.AnyAsync(u => u.Id == assignment.UserId);
                if (!userExists)
                {
                    return BadRequest(new { message = "User không t?n t?i" });
                }

                // Validate KPI exists
                var kpiExists = await _context.KPIs.AnyAsync(k => k.Id == assignment.KpiId);
                if (!kpiExists)
                {
                    return BadRequest(new { message = "KPI không t?n t?i" });
                }

                // Ki?m tra duplicate (User + KPI + Period)
                var duplicateExists = await _context.UserKpiAssignments
                    .Include(u => u.Kpi)
                    .AnyAsync(u => u.UserId == assignment.UserId &&
                                  u.KpiId == assignment.KpiId &&
                                  u.IsActive);

                if (duplicateExists)
                {
                    return BadRequest(new { message = "User ?ã ???c gán KPI này r?i (active)" });
                }

                // Set defaults
                assignment.AssignedDate = DateTime.UtcNow;
                assignment.CreatedAt = DateTime.UtcNow;
                assignment.IsActive = true;

                // L?y user hi?n t?i t? token (n?u c?n)
                // var currentUserId = GetCurrentUserId();
                // assignment.AssignedBy = currentUserId;

                _context.UserKpiAssignments.Add(assignment);
                await _context.SaveChangesAsync();

                // Load navigation properties
                await _context.Entry(assignment)
                    .Reference(u => u.User)
                    .LoadAsync();
                await _context.Entry(assignment)
                    .Reference(u => u.Kpi)
                    .LoadAsync();

                _logger.LogInformation(
                    "?ã gán KPI {KpiId} cho User {UserId}",
                    assignment.KpiId,
                    assignment.UserId);

                return CreatedAtAction(
                    nameof(GetUserKpiAssignment),
                    new { id = assignment.Id },
                    assignment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi t?o UserKpiAssignment");
                return StatusCode(500, new { message = "L?i server", error = ex.Message });
            }
        }

        /// <summary>
        /// Gán KPI hàng lo?t cho nhi?u Users
        /// POST /api/UserKpiAssignments/bulk
        /// </summary>
        [HttpPost("bulk")]
        public async Task<ActionResult> BulkCreateUserKpiAssignments(
            [FromBody] BulkAssignmentRequest request)
        {
            try
            {
                // Validate KPI exists
                var kpi = await _context.KPIs.FindAsync(request.KpiId);
                if (kpi == null)
                {
                    return BadRequest(new { message = "KPI không t?n t?i" });
                }

                var createdAssignments = new List<UserKpiAssignment>();

                foreach (var userId in request.UserIds)
                {
                    // Validate User exists
                    var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                    if (!userExists)
                    {
                        _logger.LogWarning("User {UserId} không t?n t?i, b? qua", userId);
                        continue;
                    }

                    // Ki?m tra duplicate
                    var duplicateExists = await _context.UserKpiAssignments
                        .AnyAsync(u => u.UserId == userId &&
                                      u.KpiId == request.KpiId &&
                                      u.IsActive);

                    if (duplicateExists)
                    {
                        _logger.LogWarning("User {UserId} ?ã ???c gán KPI {KpiId}, b? qua", userId, request.KpiId);
                        continue;
                    }

                    var assignment = new UserKpiAssignment
                    {
                        UserId = userId,
                        KpiId = request.KpiId,
                        CustomTargetValue = request.CustomTargetValue,
                        Weight = request.Weight ?? 100,
                        AssignedDate = DateTime.UtcNow,
                        StartDate = request.StartDate,
                        EndDate = request.EndDate,
                        IsActive = true,
                        Notes = request.Notes,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.UserKpiAssignments.Add(assignment);
                    createdAssignments.Add(assignment);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "?ã gán KPI {KpiId} cho {Count} users",
                    request.KpiId,
                    createdAssignments.Count);

                return Ok(new
                {
                    message = $"?ã gán KPI cho {createdAssignments.Count} users thành công",
                    count = createdAssignments.Count,
                    assignments = createdAssignments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi t?o bulk UserKpiAssignments");
                return StatusCode(500, new { message = "L?i server", error = ex.Message });
            }
        }

        /// <summary>
        /// C?p nh?t UserKpiAssignment
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<UserKpiAssignment>> UpdateUserKpiAssignment(
            int id,
            [FromBody] UserKpiAssignment assignment)
        {
            if (id != assignment.Id)
            {
                return BadRequest(new { message = "ID không kh?p" });
            }

            try
            {
                var existingAssignment = await _context.UserKpiAssignments.FindAsync(id);
                if (existingAssignment == null)
                {
                    return NotFound(new { message = "Không tìm th?y assignment" });
                }

                // Update fields
                existingAssignment.CustomTargetValue = assignment.CustomTargetValue;
                existingAssignment.Weight = assignment.Weight;
                existingAssignment.StartDate = assignment.StartDate;
                existingAssignment.EndDate = assignment.EndDate;
                existingAssignment.IsActive = assignment.IsActive;
                existingAssignment.Notes = assignment.Notes;
                existingAssignment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Load navigation properties
                await _context.Entry(existingAssignment)
                    .Reference(u => u.User)
                    .LoadAsync();
                await _context.Entry(existingAssignment)
                    .Reference(u => u.Kpi)
                    .LoadAsync();

                _logger.LogInformation("?ã c?p nh?t UserKpiAssignment {Id}", id);

                return Ok(existingAssignment);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.UserKpiAssignments.AnyAsync(e => e.Id == id))
                {
                    return NotFound(new { message = "Không tìm th?y assignment" });
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi c?p nh?t UserKpiAssignment {Id}", id);
                return StatusCode(500, new { message = "L?i server", error = ex.Message });
            }
        }

        /// <summary>
        /// Xóa/Vô hi?u hóa UserKpiAssignment
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUserKpiAssignment(int id)
        {
            try
            {
                var assignment = await _context.UserKpiAssignments.FindAsync(id);
                if (assignment == null)
                {
                    return NotFound(new { message = "Không tìm th?y assignment" });
                }

                // Soft delete: Set IsActive = false
                assignment.IsActive = false;
                assignment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("?ã vô hi?u hóa UserKpiAssignment {Id}", id);

                return Ok(new
                {
                    message = "?ã xóa assignment thành công",
                    id = assignment.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi xóa UserKpiAssignment {Id}", id);
                return StatusCode(500, new { message = "L?i server", error = ex.Message });
            }
        }

        /// <summary>
        /// L?y t?t c? KPI ?ã gán cho 1 User
        /// GET /api/UserKpiAssignments/user/{userId}
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserKpiAssignment>>> GetUserAssignments(
            int userId,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var query = _context.UserKpiAssignments
                    .Include(u => u.Kpi)
                        .ThenInclude(k => k.Department)
                    .Include(u => u.Kpi)
                        .ThenInclude(k => k.CommissionTiers)
                    .Where(u => u.UserId == userId);

                if (isActive.HasValue)
                    query = query.Where(u => u.IsActive == isActive.Value);

                var assignments = await query
                    .OrderByDescending(u => u.AssignedDate)
                    .ToListAsync();

                return Ok(assignments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi l?y assignments c?a User {UserId}", userId);
                return StatusCode(500, new { message = "L?i server", error = ex.Message });
            }
        }

        /// <summary>
        /// L?y t?t c? Users ???c gán KPI c? th?
        /// GET /api/UserKpiAssignments/kpi/{kpiId}
        /// </summary>
        [HttpGet("kpi/{kpiId}")]
        public async Task<ActionResult<IEnumerable<UserKpiAssignment>>> GetKpiAssignments(
            int kpiId,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var query = _context.UserKpiAssignments
                    .Include(u => u.User)
                        .ThenInclude(u => u.Department)
                    .Include(u => u.User)
                        .ThenInclude(u => u.Position)
                    .Where(u => u.KpiId == kpiId);

                if (isActive.HasValue)
                    query = query.Where(u => u.IsActive == isActive.Value);

                var assignments = await query
                    .OrderByDescending(u => u.AssignedDate)
                    .ToListAsync();

                return Ok(assignments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi l?y assignments c?a KPI {KpiId}", kpiId);
                return StatusCode(500, new { message = "L?i server", error = ex.Message });
            }
        }
    }

    /// <summary>
    /// DTO for bulk assignment
    /// </summary>
    public class BulkAssignmentRequest
    {
        public int KpiId { get; set; }
        public List<int> UserIds { get; set; } = new();
        public decimal? CustomTargetValue { get; set; }
        public int? Weight { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Notes { get; set; }
    }
}
