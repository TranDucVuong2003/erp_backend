using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using erp_backend.Data;
using erp_backend.Models;
using Microsoft.AspNetCore.Authorization;

namespace erp_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CommissionRatesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CommissionRatesController> _logger;

        public CommissionRatesController(ApplicationDbContext context, ILogger<CommissionRatesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả các bậc hoa hồng
        /// </summary>
        /// <returns>Danh sách CommissionRate</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommissionRate>>> GetCommissionRates()
        {
            try
            {
                var commissionRates = await _context.CommissionRates
                    .OrderBy(c => c.TierLevel)
                    .ToListAsync();

                return Ok(commissionRates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách CommissionRate");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách bậc hoa hồng" });
            }
        }

        /// <summary>
        /// Lấy chi tiết một bậc hoa hồng theo ID
        /// </summary>
        /// <param name="id">ID của CommissionRate</param>
        /// <returns>Thông tin CommissionRate</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<CommissionRate>> GetCommissionRate(int id)
        {
            try
            {
                var commissionRate = await _context.CommissionRates.FindAsync(id);

                if (commissionRate == null)
                {
                    return NotFound(new { message = $"Không tìm thấy bậc hoa hồng với ID {id}" });
                }

                return Ok(commissionRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy CommissionRate ID {Id}", id);
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thông tin bậc hoa hồng" });
            }
        }

        /// <summary>
        /// Tìm bậc hoa hồng phù hợp theo số tiền
        /// </summary>
        /// <param name="amount">Số tiền cần tra cứu</param>
        /// <returns>CommissionRate phù hợp</returns>
        [HttpGet("by-amount/{amount}")]
        public async Task<ActionResult<CommissionRate>> GetCommissionRateByAmount(decimal amount)
        {
            try
            {
                var commissionRate = await _context.CommissionRates
                    .Where(c => c.IsActive
                        && amount >= c.MinAmount
                        && (c.MaxAmount == null || amount <= c.MaxAmount))
                    .OrderBy(c => c.TierLevel)
                    .FirstOrDefaultAsync();

                if (commissionRate == null)
                {
                    return NotFound(new { message = $"Không tìm thấy bậc hoa hồng phù hợp cho số tiền {amount:N0} VND" });
                }

                return Ok(commissionRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm CommissionRate theo số tiền {Amount}", amount);
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi tìm bậc hoa hồng" });
            }
        }

        /// <summary>
        /// Tạo mới một bậc hoa hồng
        /// </summary>
        /// <param name="commissionRate">Thông tin CommissionRate</param>
        /// <returns>CommissionRate vừa tạo</returns>
        [HttpPost]
        public async Task<ActionResult<CommissionRate>> CreateCommissionRate(CommissionRate commissionRate)
        {
            try
            {
                // Validate
                if (commissionRate.MinAmount < 0)
                {
                    return BadRequest(new { message = "Số tiền tối thiểu phải lớn hơn hoặc bằng 0" });
                }

                if (commissionRate.MaxAmount.HasValue && commissionRate.MaxAmount < commissionRate.MinAmount)
                {
                    return BadRequest(new { message = "Số tiền tối đa phải lớn hơn số tiền tối thiểu" });
                }

                if (commissionRate.CommissionPercentage < 0 || commissionRate.CommissionPercentage > 100)
                {
                    return BadRequest(new { message = "Tỷ lệ hoa hồng phải từ 0% đến 100%" });
                }

                // Kiểm tra trùng lặp khoảng tiền
                var overlapping = await _context.CommissionRates
                    .Where(c => c.IsActive
                        && c.Id != commissionRate.Id
                        && ((c.MinAmount <= commissionRate.MinAmount && (c.MaxAmount == null || c.MaxAmount >= commissionRate.MinAmount))
                            || (commissionRate.MaxAmount.HasValue && c.MinAmount <= commissionRate.MaxAmount && (c.MaxAmount == null || c.MaxAmount >= commissionRate.MaxAmount))))
                    .AnyAsync();

                if (overlapping)
                {
                    return BadRequest(new { message = "Khoảng tiền bị trùng lặp với bậc hoa hồng khác" });
                }

                commissionRate.CreatedAt = DateTime.UtcNow;
                commissionRate.UpdatedAt = null;

                _context.CommissionRates.Add(commissionRate);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã tạo CommissionRate mới: ID {Id}, Tier {Tier}, {Min}-{Max}: {Percentage}%",
                    commissionRate.Id, commissionRate.TierLevel, commissionRate.MinAmount, commissionRate.MaxAmount, commissionRate.CommissionPercentage);

                return CreatedAtAction(nameof(GetCommissionRate), new { id = commissionRate.Id }, commissionRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo CommissionRate mới");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi tạo bậc hoa hồng mới" });
            }
        }

        /// <summary>
        /// Cập nhật thông tin bậc hoa hồng
        /// </summary>
        /// <param name="id">ID của CommissionRate</param>
        /// <param name="commissionRate">Thông tin cập nhật</param>
        /// <returns>NoContent nếu thành công</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCommissionRate(int id, CommissionRate commissionRate)
        {
            if (id != commissionRate.Id)
            {
                return BadRequest(new { message = "ID không khớp" });
            }

            try
            {
                var existingRate = await _context.CommissionRates.FindAsync(id);
                if (existingRate == null)
                {
                    return NotFound(new { message = $"Không tìm thấy bậc hoa hồng với ID {id}" });
                }

                // Validate
                if (commissionRate.MinAmount < 0)
                {
                    return BadRequest(new { message = "Số tiền tối thiểu phải lớn hơn hoặc bằng 0" });
                }

                if (commissionRate.MaxAmount.HasValue && commissionRate.MaxAmount < commissionRate.MinAmount)
                {
                    return BadRequest(new { message = "Số tiền tối đa phải lớn hơn số tiền tối thiểu" });
                }

                if (commissionRate.CommissionPercentage < 0 || commissionRate.CommissionPercentage > 100)
                {
                    return BadRequest(new { message = "Tỷ lệ hoa hồng phải từ 0% đến 100%" });
                }

                // Kiểm tra trùng lặp khoảng tiền (bỏ qua bản ghi hiện tại)
                var overlapping = await _context.CommissionRates
                    .Where(c => c.IsActive
                        && c.Id != id
                        && ((c.MinAmount <= commissionRate.MinAmount && (c.MaxAmount == null || c.MaxAmount >= commissionRate.MinAmount))
                            || (commissionRate.MaxAmount.HasValue && c.MinAmount <= commissionRate.MaxAmount && (c.MaxAmount == null || c.MaxAmount >= commissionRate.MaxAmount))))
                    .AnyAsync();

                if (overlapping)
                {
                    return BadRequest(new { message = "Khoảng tiền bị trùng lặp với bậc hoa hồng khác" });
                }

                // Update fields
                existingRate.MinAmount = commissionRate.MinAmount;
                existingRate.MaxAmount = commissionRate.MaxAmount;
                existingRate.CommissionPercentage = commissionRate.CommissionPercentage;
                existingRate.TierLevel = commissionRate.TierLevel;
                existingRate.IsActive = commissionRate.IsActive;
                existingRate.Description = commissionRate.Description;
                existingRate.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã cập nhật CommissionRate ID {Id}", id);

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CommissionRateExists(id))
                {
                    return NotFound(new { message = $"Không tìm thấy bậc hoa hồng với ID {id}" });
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật CommissionRate ID {Id}", id);
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi cập nhật bậc hoa hồng" });
            }
        }

        /// <summary>
        /// Xóa một bậc hoa hồng (soft delete - set IsActive = false)
        /// </summary>
        /// <param name="id">ID của CommissionRate</param>
        /// <returns>NoContent nếu thành công</returns>
        [HttpDelete("{id}")]
		public async Task<IActionResult> DeleteCommissionRate(int id)
		{
			try
			{
				var commissionRate = await _context.CommissionRates.FindAsync(id);
				if (commissionRate == null)
				{
					return NotFound(new { message = $"Không tìm thấy bậc hoa hồng với ID {id}" });
				}

				// Hard delete
				_context.CommissionRates.Remove(commissionRate);

				await _context.SaveChangesAsync();

				_logger.LogInformation("Đã xóa (hard delete) CommissionRate ID {Id}", id);

				return NoContent();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi xóa CommissionRate ID {Id}", id);
				return StatusCode(500, new { message = "Đã xảy ra lỗi khi xóa bậc hoa hồng" });
			}
		}



		private async Task<bool> CommissionRateExists(int id)
        {
            return await _context.CommissionRates.AnyAsync(e => e.Id == id);
        }
    }
}
