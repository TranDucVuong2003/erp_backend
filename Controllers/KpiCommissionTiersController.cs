using erp_backend.Data;
using erp_backend.Models;
using erp_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class KpiCommissionTiersController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly ICommissionCalculationService _commissionService;
		private readonly ILogger<KpiCommissionTiersController> _logger;

		public KpiCommissionTiersController(
			ApplicationDbContext context,
			ICommissionCalculationService commissionService,
			ILogger<KpiCommissionTiersController> logger)
		{
			_context = context;
			_commissionService = commissionService;
			_logger = logger;
		}

		// GET: api/KpiCommissionTiers
		[HttpGet]
		[Authorize]
		public async Task<ActionResult<IEnumerable<KpiCommissionTier>>> GetKpiCommissionTiers([FromQuery] int? kpiId = null)
		{
			try
			{
				var query = _context.KpiCommissionTiers
					.Include(t => t.Kpi)
					.AsQueryable();

				if (kpiId.HasValue)
				{
					query = query.Where(t => t.KpiId == kpiId.Value);
				}

				var tiers = await query
					.OrderBy(t => t.KpiId)
					.ThenBy(t => t.TierLevel)
					.ToListAsync();

				return Ok(tiers);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi lấy danh sách bậc hoa hồng");
				return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
			}
		}

		// GET: api/KpiCommissionTiers/5
		[HttpGet("{id}")]
		[Authorize]
		public async Task<ActionResult<KpiCommissionTier>> GetKpiCommissionTier(int id)
		{
			try
			{
				var tier = await _context.KpiCommissionTiers
					.Include(t => t.Kpi)
					.FirstOrDefaultAsync(t => t.Id == id);

				if (tier == null)
				{
					return NotFound(new { message = "Không tìm thấy bậc hoa hồng" });
				}

				return Ok(tier);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi lấy thông tin bậc hoa hồng với ID: {Id}", id);
				return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
			}
		}

		// GET: api/KpiCommissionTiers/kpi/{kpiId}
		[HttpGet("kpi/{kpiId}")]
		[Authorize]
		public async Task<ActionResult<IEnumerable<KpiCommissionTier>>> GetTiersByKpi(int kpiId)
		{
			try
			{
				var tiers = await _commissionService.GetCommissionTiersAsync(kpiId);
				return Ok(tiers);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi lấy các bậc hoa hồng của KPI: {KpiId}", kpiId);
				return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
			}
		}

		// POST: api/KpiCommissionTiers/calculate
		[HttpPost("calculate")]
		[Authorize]
		public async Task<ActionResult<CommissionResult>> CalculateCommission(
			[FromBody] CommissionCalculationRequest request)
		{
			try
			{
				if (request.Revenue < 0)
				{
					return BadRequest(new { message = "Doanh số phải lớn hơn hoặc bằng 0" });
				}

				var result = await _commissionService.CalculateCommissionAsync(request.KpiId, request.Revenue);
				return Ok(result);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi tính hoa hồng");
				return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
			}
		}

		// POST: api/KpiCommissionTiers
		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult<KpiCommissionTier>> CreateKpiCommissionTier(KpiCommissionTier tier)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var kpi = await _context.KPIs.FindAsync(tier.KpiId);
				if (kpi == null)
				{
					return BadRequest(new { message = "KPI không tồn tại" });
				}

				if (tier.MaxRevenue.HasValue && tier.MinRevenue >= tier.MaxRevenue)
				{
					return BadRequest(new { message = "MinRevenue phải nhỏ hơn MaxRevenue" });
				}

				var existingTier = await _context.KpiCommissionTiers
					.FirstOrDefaultAsync(t => t.KpiId == tier.KpiId && t.TierLevel == tier.TierLevel);

				if (existingTier != null)
				{
					return BadRequest(new { message = $"Đã tồn tại bậc {tier.TierLevel} cho KPI này" });
				}

				tier.CreatedAt = DateTime.UtcNow;

				_context.KpiCommissionTiers.Add(tier);
				await _context.SaveChangesAsync();

				_logger.LogInformation("Đã tạo bậc hoa hồng mới với ID: {Id}", tier.Id);

				return CreatedAtAction(nameof(GetKpiCommissionTier), new { id = tier.Id }, tier);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi tạo bậc hoa hồng mới");
				return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
			}
		}

		// PUT: api/KpiCommissionTiers/5
		[HttpPut("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdateKpiCommissionTier(int id, KpiCommissionTier tier)
		{
			if (id != tier.Id)
			{
				return BadRequest(new { message = "ID không khớp" });
			}

			try
			{
				var existing = await _context.KpiCommissionTiers.FindAsync(id);
				if (existing == null)
				{
					return NotFound(new { message = "Không tìm thấy bậc hoa hồng" });
				}

				if (tier.MaxRevenue.HasValue && tier.MinRevenue >= tier.MaxRevenue)
				{
					return BadRequest(new { message = "MinRevenue phải nhỏ hơn MaxRevenue" });
				}

				existing.TierLevel = tier.TierLevel;
				existing.MinRevenue = tier.MinRevenue;
				existing.MaxRevenue = tier.MaxRevenue;
				existing.CommissionPercentage = tier.CommissionPercentage;
				existing.Description = tier.Description;
				existing.IsActive = tier.IsActive;
				existing.UpdatedAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();

				_logger.LogInformation("Đã cập nhật bậc hoa hồng với ID: {Id}", id);

				return Ok(new { message = "Cập nhật bậc hoa hồng thành công" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi cập nhật bậc hoa hồng với ID: {Id}", id);
				return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
			}
		}

		// DELETE: api/KpiCommissionTiers/5
		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteKpiCommissionTier(int id)
		{
			try
			{
				var tier = await _context.KpiCommissionTiers.FindAsync(id);
				if (tier == null)
				{
					return NotFound(new { message = "Không tìm thấy bậc hoa hồng" });
				}

				_context.KpiCommissionTiers.Remove(tier);
				await _context.SaveChangesAsync();

				_logger.LogInformation("Đã xóa bậc hoa hồng ID: {Id}", id);

				return Ok(new { message = "Xóa bậc hoa hồng thành công", deletedId = id });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi xóa bậc hoa hồng ID: {Id}", id);
				return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
			}
		}

		// POST: api/KpiCommissionTiers/bulk
		[HttpPost("bulk")]
		[Authorize(Roles = "Admin")]
		public async Task<ActionResult> CreateBulkTiers([FromBody] BulkTiersRequest request)
		{
			try
			{
				var kpi = await _context.KPIs.FindAsync(request.KpiId);
				if (kpi == null)
				{
					return BadRequest(new { message = "KPI không tồn tại" });
				}

				if (request.ReplaceExisting)
				{
					var existingTiers = await _context.KpiCommissionTiers
						.Where(t => t.KpiId == request.KpiId)
						.ToListAsync();

					_context.KpiCommissionTiers.RemoveRange(existingTiers);
				}

				foreach (var tierData in request.Tiers)
				{
					var tier = new KpiCommissionTier
					{
						KpiId = request.KpiId,
						TierLevel = tierData.TierLevel,
						MinRevenue = tierData.MinRevenue,
						MaxRevenue = tierData.MaxRevenue,
						CommissionPercentage = tierData.CommissionPercentage,
						Description = tierData.Description,
						IsActive = true,
						CreatedAt = DateTime.UtcNow
					};

					_context.KpiCommissionTiers.Add(tier);
				}

				await _context.SaveChangesAsync();

				_logger.LogInformation("Đã tạo {Count} bậc hoa hồng cho KPI: {KpiId}",
					request.Tiers.Count, request.KpiId);

				return Ok(new { message = $"Đã tạo {request.Tiers.Count} bậc hoa hồng thành công" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi tạo bulk bậc hoa hồng");
				return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
			}
		}
	}

	// DTOs
	public class CommissionCalculationRequest
	{
		public int KpiId { get; set; }
		public decimal Revenue { get; set; }
	}

	public class BulkTiersRequest
	{
		public int KpiId { get; set; }
		public bool ReplaceExisting { get; set; }
		public List<TierData> Tiers { get; set; } = new();
	}

	public class TierData
	{
		public int TierLevel { get; set; }
		public decimal MinRevenue { get; set; }
		public decimal? MaxRevenue { get; set; }
		public decimal CommissionPercentage { get; set; }
		public string? Description { get; set; }
	}
}
