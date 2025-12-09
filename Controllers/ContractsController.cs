using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using erp_backend.Data;
using erp_backend.Models;
using erp_backend.Models.DTOs;
using erp_backend.Services;
using System.Web;

namespace erp_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
	public class ContractsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContractsController> _logger;
        private readonly IKpiCalculationService _kpiCalculationService;
        private readonly IConfiguration _configuration;

        public ContractsController(
            ApplicationDbContext context, 
            ILogger<ContractsController> logger,
            IKpiCalculationService kpiCalculationService,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _kpiCalculationService = kpiCalculationService;
            _configuration = configuration;
        }

        // GET: api/Contracts
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ContractResponse>>> GetContracts()
        {
            // Lấy role từ JWT token
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role);
            var role = roleClaim?.Value;

            // Lấy UserId từ JWT token
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userid");
            
            var query = _context.Contracts
                .Include(c => c.User)
                .Include(c => c.SaleOrder)
                    .ThenInclude(so => so!.Customer)
                .Include(c => c.SaleOrder)
                    .ThenInclude(so => so!.SaleOrderServices)
                        .ThenInclude(sos => sos.Service)
                            .ThenInclude(s => s!.Tax)
                .Include(c => c.SaleOrder)
                    .ThenInclude(so => so!.SaleOrderAddons)
                        .ThenInclude(soa => soa.Addon)
                            .ThenInclude(a => a!.Tax)
                .AsQueryable();

            // Nếu role là "user" thì chỉ lấy hợp đồng có SaleOrder được tạo bởi user đó
            if (role != null && role.ToLower() == "user")
            {
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    // Lọc theo CreatedByUserId của SaleOrder
                    query = query.Where(c => c.SaleOrder!.CreatedByUserId == userId);
                    _logger.LogInformation($"User role detected. Filtering contracts for UserId: {userId}");
                }
                else
                {
                    _logger.LogWarning("User role detected but UserId claim not found");
                    return Forbid();
                }
            }
            else if (role != null && role.ToLower() == "admin")
            {
                // Admin có thể xem tất cả hợp đồng
                _logger.LogInformation("Admin role detected. Returning all contracts");
            }
            else
            {
                // Nếu không có role hoặc role không hợp lệ
                _logger.LogWarning($"Invalid or missing role: {role}");
                return Forbid();
            }

            var contracts = await query.ToListAsync();

            var response = contracts.Select(c => MapToContractResponse(c));

            return Ok(response);
        }

        // GET: api/Contracts/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ContractResponse>> GetContract(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.User)
                .Include(c => c.SaleOrder)
                    .ThenInclude(so => so!.Customer)
                .Include(c => c.SaleOrder)
                    .ThenInclude(so => so!.SaleOrderServices)
                        .ThenInclude(sos => sos.Service)
                            .ThenInclude(s => s!.Tax)
                .Include(c => c.SaleOrder)
                    .ThenInclude(so => so!.SaleOrderAddons)
                        .ThenInclude(soa => soa.Addon)
                            .ThenInclude(a => a!.Tax)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
            {
                return NotFound(new { message = "Không tìm thấy hợp đồng" });
            }

            var response = MapToContractResponse(contract);
            return Ok(response);
        }

        // POST: api/Contracts
        [HttpPost]
        [Authorize]
		public async Task<ActionResult<ContractResponse>> CreateContract([FromBody] CreateContractRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Kiểm tra SaleOrder tồn tại
                var saleOrder = await _context.SaleOrders
                    .Include(so => so.SaleOrderServices)
                        .ThenInclude(sos => sos.Service)
                            .ThenInclude(s => s!.Tax)
                    .Include(so => so.SaleOrderAddons)
                        .ThenInclude(soa => soa.Addon)
                            .ThenInclude(a => a!.Tax)
                    .FirstOrDefaultAsync(so => so.Id == request.SaleOrderId);

                if (saleOrder == null)
                {
                    return BadRequest(new { message = "SaleOrder không tồn tại" });
                }

                // Kiểm tra user tồn tại
                var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
                if (!userExists)
                {
                    return BadRequest(new { message = "User không tồn tại" });
                }

                // ✅ TỰ ĐỘNG TẠO SỐ HỢP ĐỒNG (bắt đầu từ 128)
                var maxContractNumber = await _context.Contracts
                    .OrderByDescending(c => c.NumberContract)
                    .Select(c => c.NumberContract)
                    .FirstOrDefaultAsync();

                int nextNumber = maxContractNumber > 0 ? maxContractNumber + 1 : 128;

                // ✅ Tính toán SubTotal, TaxAmount, TotalAmount
                decimal subTotal = 0;
                decimal taxAmount = 0;

                // Tính từ Services
                if (saleOrder.SaleOrderServices != null && saleOrder.SaleOrderServices.Any())
                {
                    foreach (var sos in saleOrder.SaleOrderServices)
                    {
                        var quantity = sos.Quantity ?? (sos.Service?.Quantity ?? 1);
                        var lineTotal = sos.UnitPrice * quantity;
                        subTotal += lineTotal;

                        // Tính thuế cho từng service
                        if (sos.Service?.Tax != null)
                        {
                            var taxRate = (decimal)sos.Service.Tax.Rate / 100m;
                            taxAmount += lineTotal * taxRate;
                        }
                    }
                }

                // Tính từ Addons
                if (saleOrder.SaleOrderAddons != null && saleOrder.SaleOrderAddons.Any())
                {
                    foreach (var soa in saleOrder.SaleOrderAddons)
                    {
                        var quantity = soa.Quantity ?? (soa.Addon?.Quantity ?? 1);
                        var lineTotal = soa.UnitPrice * quantity;
                        subTotal += lineTotal;

                        // Tính thuế cho từng addon
                        if (soa.Addon?.Tax != null)
                        {
                            var taxRate = (decimal)soa.Addon.Tax.Rate / 100m;
                            taxAmount += lineTotal * taxRate;
                        }
                    }
                }

                // Nếu không có services/addons, sử dụng Value từ SaleOrder
                if (subTotal == 0)
                {
                    subTotal = saleOrder.Value;
                }

                decimal totalAmount = subTotal + taxAmount;

                // Tạo Contract
                var contract = new Contract
                {
                    SaleOrderId = request.SaleOrderId,
                    UserId = request.UserId,
                    NumberContract = nextNumber, // ✅ Tự động gán
                    Status = request.Status,
                    PaymentMethod = request.PaymentMethod,
                    SubTotal = subTotal,
                    TaxAmount = taxAmount,
                    TotalAmount = totalAmount,
                    Expiration = request.Expiration,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Contracts.Add(contract);
                await _context.SaveChangesAsync();

                // Load lại contract với đầy đủ navigation properties
                contract = await _context.Contracts
                    .Include(c => c.User)
                    .Include(c => c.SaleOrder)
                        .ThenInclude(so => so!.Customer)
                    .Include(c => c.SaleOrder)
                        .ThenInclude(so => so!.SaleOrderServices)
                            .ThenInclude(sos => sos.Service)
                                .ThenInclude(s => s!.Tax)
                    .Include(c => c.SaleOrder)
                        .ThenInclude(so => so!.SaleOrderAddons)
                            .ThenInclude(soa => soa.Addon)
                                .ThenInclude(a => a!.Tax)
                    .FirstAsync(c => c.Id == contract.Id);

                var response = MapToContractResponse(contract);

                return CreatedAtAction(nameof(GetContract), new { id = contract.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo hợp đồng mới");
                return StatusCode(500, new { message = "Lỗi server khi tạo hợp đồng", error = ex.Message });
            }
        }

        // PUT: api/Contracts/5
        [HttpPut("{id}")]
        [Authorize]
		public async Task<ActionResult<ContractResponse>> UpdateContract(int id, [FromBody] CreateContractRequest request)
        {
            try
            {
                // Kiểm tra contract có tồn tại không
                var existingContract = await _context.Contracts
                    .Include(c => c.SaleOrder) // ✅ Load SaleOrder để lấy CreatedByUserId
                    .FirstOrDefaultAsync(c => c.Id == id);
                    
                if (existingContract == null)
                {
                    return NotFound(new { message = "Không tìm thấy hợp đồng" });
                }

                // ✅ Lưu trạng thái cũ để so sánh
                var oldStatus = existingContract.Status;

                // Kiểm tra SaleOrder tồn tại (nếu thay đổi)
                if (request.SaleOrderId != existingContract.SaleOrderId)
                {
                    var saleOrderExists = await _context.SaleOrders.AnyAsync(so => so.Id == request.SaleOrderId);
                    if (!saleOrderExists)
                    {
                        return BadRequest(new { message = "SaleOrder không tồn tại" });
                    }
                }

                // Kiểm tra user tồn tại (nếu thay đổi)
                if (request.UserId != existingContract.UserId)
                {
                    var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
                    if (!userExists)
                    {
                        return BadRequest(new { message = "User không tồn tại" });
                    }
                }

                // ✅ Invalidate PDF cache nếu có thay đổi quan trọng
                bool needsRegenerate = request.SaleOrderId != existingContract.SaleOrderId ||
                                      request.UserId != existingContract.UserId ||
                                      // NumberContract không thể thay đổi khi update
                                      request.Status != existingContract.Status ||
                                      request.PaymentMethod != existingContract.PaymentMethod ||
                                      request.Expiration != existingContract.Expiration;

                if (needsRegenerate && !string.IsNullOrEmpty(existingContract.ContractPdfPath))
                {
                    // Xóa file PDF cũ
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingContract.ContractPdfPath);
                    
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                        _logger.LogInformation("Đã xóa file PDF cũ do cập nhật Contract: {FilePath}", existingContract.ContractPdfPath);
                    }

                    // Reset thông tin PDF
                    existingContract.ContractPdfPath = null;
                    existingContract.PdfGeneratedAt = null;
                    existingContract.PdfFileSize = null;
                }

                // Cập nhật các trường (NumberContract KHÔNG được cập nhật)
                existingContract.SaleOrderId = request.SaleOrderId;
                existingContract.UserId = request.UserId;
                // existingContract.NumberContract - KHÔNG thay đổi
                existingContract.Status = request.Status;
                existingContract.PaymentMethod = request.PaymentMethod;
                existingContract.Expiration = request.Expiration;
                existingContract.Notes = request.Notes;
                existingContract.UpdatedAt = DateTime.UtcNow;

                // ✅ Tính lại SubTotal, TaxAmount, TotalAmount nếu SaleOrder thay đổi
                if (request.SaleOrderId != existingContract.SaleOrderId)
                {
                    var saleOrder = await _context.SaleOrders
                        .Include(so => so.SaleOrderServices)
                            .ThenInclude(sos => sos.Service)
                                .ThenInclude(s => s!.Tax)
                        .Include(so => so.SaleOrderAddons)
                            .ThenInclude(soa => soa.Addon)
                                .ThenInclude(a => a!.Tax)
                        .FirstAsync(so => so.Id == request.SaleOrderId);

                    decimal subTotal = 0;
                    decimal taxAmount = 0;

                    // Tính từ Services
                    if (saleOrder.SaleOrderServices != null && saleOrder.SaleOrderServices.Any())
                    {
                        foreach (var sos in saleOrder.SaleOrderServices)
                        {
                            var quantity = sos.Quantity ?? (sos.Service?.Quantity ?? 1);
                            var lineTotal = sos.UnitPrice * quantity;
                            subTotal += lineTotal;

                            // Tính thuế cho từng service
                            if (sos.Service?.Tax != null)
                            {
                                var taxRate = (decimal)sos.Service.Tax.Rate / 100m;
                                taxAmount += lineTotal * taxRate;
                            }
                        }
                    }

                    // Tính từ Addons
                    if (saleOrder.SaleOrderAddons != null && saleOrder.SaleOrderAddons.Any())
                    {
                        foreach (var soa in saleOrder.SaleOrderAddons)
                        {
                            var quantity = soa.Quantity ?? (soa.Addon?.Quantity ?? 1);
                            var lineTotal = soa.UnitPrice * quantity;
                            subTotal += lineTotal;

                            // Tính thuế cho từng addon
                            if (soa.Addon?.Tax != null)
                            {
                                var taxRate = (decimal)soa.Addon.Tax.Rate / 100m;
                                taxAmount += lineTotal * taxRate;
                            }
                        }
                    }

                    // Nếu không có services/addons, sử dụng Value từ SaleOrder
                    if (subTotal == 0)
                    {
                        subTotal = saleOrder.Value;
                    }

                    existingContract.SubTotal = subTotal;
                    existingContract.TaxAmount = taxAmount;
                    existingContract.TotalAmount = subTotal + taxAmount;
                }

                await _context.SaveChangesAsync();

                // ✅ KPI AUTO-CALCULATION: Khi status đổi thành "Paid"
                if (oldStatus?.ToLower() != "paid" && request.Status?.ToLower() == "paid")
                {
                    _logger.LogInformation("Contract {ContractId} status changed to Paid. Triggering KPI calculation...", id);

                    // Lấy thông tin Sale User từ SaleOrder.CreatedByUserId
                    var saleOrderForKpi = existingContract.SaleOrder;
                    if (saleOrderForKpi == null)
                    {
                        // Reload nếu chưa có
                        saleOrderForKpi = await _context.SaleOrders.FindAsync(existingContract.SaleOrderId);
                    }

                    if (saleOrderForKpi?.CreatedByUserId.HasValue == true)
                    {
                        var saleUserId = saleOrderForKpi.CreatedByUserId.Value;
                        var contractMonth = existingContract.CreatedAt.Month;
                        var contractYear = existingContract.CreatedAt.Year;

                        _logger.LogInformation(
                            "Auto-calculating KPI for Sale User {UserId} (Contract {ContractId}, Month {Month}/{Year})",
                            saleUserId, id, contractMonth, contractYear);

                        try
                        {
                            // Gọi service tính KPI cho user này trong tháng tạo hợp đồng
                            await _kpiCalculationService.CalculateKpiForUserAsync(saleUserId, contractMonth, contractYear);
                            
                            _logger.LogInformation(
                                "✅ Successfully calculated KPI for User {UserId} after Contract {ContractId} marked as Paid",
                                saleUserId, id);
                        }
                        catch (Exception kpiEx)
                        {
                            // Log lỗi nhưng không fail toàn bộ request
                            _logger.LogError(kpiEx, 
                                "❌ Failed to calculate KPI for User {UserId} after Contract {ContractId} marked as Paid. " +
                                "KPI calculation can be triggered manually later.",
                                saleUserId, id);
                        }
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Contract {ContractId} marked as Paid but SaleOrder.CreatedByUserId is null. Cannot auto-calculate KPI.",
                            id);
                    }
                }

                // Load lại contract với đầy đủ navigation properties
                var contract = await _context.Contracts
                    .Include(c => c.User)
                    .Include(c => c.SaleOrder)
                        .ThenInclude(so => so!.Customer)
                    .Include(c => c.SaleOrder)
                        .ThenInclude(so => so!.SaleOrderServices)
                            .ThenInclude(sos => sos.Service)
                                .ThenInclude(s => s!.Tax)
                    .Include(c => c.SaleOrder)
                        .ThenInclude(so => so!.SaleOrderAddons)
                            .ThenInclude(soa => soa.Addon)
                                .ThenInclude(a => a!.Tax)
                    .FirstAsync(c => c.Id == id);

                var response = MapToContractResponse(contract);

                return Ok(response);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContractExists(id))
                {
                    return NotFound(new { message = "Không tìm thấy hợp đồng" });
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật hợp đồng với ID: {ContractId}", id);
                return StatusCode(500, new { message = "Lỗi server khi cập nhật hợp đồng", error = ex.Message });
            }
        }

        // DELETE: api/Contracts/5
        [HttpDelete("{id}")]
        [Authorize]
		public async Task<IActionResult> DeleteContract(int id)
		{
			try
			{
				// Load contract với SaleOrder để lấy CreatedByUserId
				var contract = await _context.Contracts
					.Include(c => c.SaleOrder)
					.FirstOrDefaultAsync(c => c.Id == id);

				if (contract == null)
				{
					return NotFound(new { message = "Không tìm thấy hợp đồng" });
				}

				// ✅ Lưu thông tin cần thiết để tính lại KPI sau khi xóa
				int? saleUserId = contract.SaleOrder?.CreatedByUserId;
				int contractMonth = contract.CreatedAt.Month;
				int contractYear = contract.CreatedAt.Year;

				// ✅ 1. Xóa các MatchedTransactions liên quan đến Contract này
				var matchedTransactions = await _context.MatchedTransactions
					.Where(mt => mt.ContractId == id)
					.ToListAsync();

				if (matchedTransactions.Any())
				{
					_context.MatchedTransactions.RemoveRange(matchedTransactions);
					_logger.LogInformation("Đã xóa {Count} MatchedTransaction(s) liên quan đến Contract {ContractId}", 
						matchedTransactions.Count, id);
				}

				// ✅ 2. Xóa file PDF nếu tồn tại
				if (!string.IsNullOrEmpty(contract.ContractPdfPath))
				{
					var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", contract.ContractPdfPath);
					
					if (System.IO.File.Exists(filePath))
					{
						try
						{
							System.IO.File.Delete(filePath);
							_logger.LogInformation("Đã xóa file PDF: {FilePath}", contract.ContractPdfPath);
						}
						catch (Exception ex)
						{
							_logger.LogWarning(ex, "Không thể xóa file PDF: {FilePath}", contract.ContractPdfPath);
							// Tiếp tục xóa record trong database dù file không xóa được
						}
					}
				}

				// ✅ 3. Xóa Contract
				_context.Contracts.Remove(contract);
				await _context.SaveChangesAsync();

				_logger.LogInformation("Đã xóa Contract {ContractId}", id);

				// ✅ 4. Tính lại KPI cho user nếu contract có liên kết với Sale User
				if (saleUserId.HasValue)
				{
					_logger.LogInformation(
						"Contract {ContractId} đã bị xóa. Đang tính lại KPI cho User {UserId} (Tháng {Month}/{Year})...",
						id, saleUserId.Value, contractMonth, contractYear);

					try
					{
						// Gọi service để tính lại KPI
						await _kpiCalculationService.CalculateKpiForUserAsync(saleUserId.Value, contractMonth, contractYear);
						
						_logger.LogInformation(
							"✅ Đã tính lại KPI thành công cho User {UserId} sau khi xóa Contract {ContractId}",
							saleUserId.Value, id);
					}
					catch (Exception kpiEx)
					{
						// Log lỗi nhưng không fail toàn bộ request (Contract đã xóa thành công)
						_logger.LogError(kpiEx, 
							"❌ Lỗi khi tính lại KPI cho User {UserId} sau khi xóa Contract {ContractId}. " +
							"KPI có thể được tính lại thủ công sau.",
							saleUserId.Value, id);
					}
				}
				else
				{
					_logger.LogWarning(
						"Contract {ContractId} đã bị xóa nhưng không có SaleOrder.CreatedByUserId. Không thể tính lại KPI.",
						id);
				}

				return NoContent();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi xóa Contract {ContractId}", id);
				return StatusCode(500, new { message = "Lỗi server khi xóa hợp đồng", error = ex.Message });
			}
		}

        // GET: api/Contracts/saleorder/5
        [HttpGet("saleorder/{saleOrderId}")]
        [Authorize]
		public async Task<ActionResult<IEnumerable<ContractListItemDto>>> GetContractsBySaleOrder(int saleOrderId)
        {
            var contracts = await _context.Contracts
                .Where(c => c.SaleOrderId == saleOrderId)
                .Include(c => c.User)
                .Include(c => c.SaleOrder)
                    .ThenInclude(so => so!.Customer)
                .ToListAsync();

            var response = contracts.Select(c => new ContractListItemDto
            {
                Id = c.Id,
                SaleOrderId = c.SaleOrderId,
                SaleOrderTitle = c.SaleOrder?.Title ?? "",
                CustomerId = c.SaleOrder?.CustomerId ?? 0,
                CustomerName = c.SaleOrder?.Customer?.Name ?? c.SaleOrder?.Customer?.CompanyName ?? "",
                UserId = c.UserId,
                UserName = c.User?.Name ?? "",
                NumberContract = c.NumberContract,
                Status = c.Status,
                PaymentMethod = c.PaymentMethod,
                TotalAmount = c.TotalAmount,
                Expiration = c.Expiration,
                CreatedAt = c.CreatedAt
            });

            return Ok(response);
        }

        // Helper method: Map Contract entity to ContractResponse DTO
        private ContractResponse MapToContractResponse(Contract contract)
        {
            return new ContractResponse
            {
                Id = contract.Id,
                SaleOrderId = contract.SaleOrderId,
                SaleOrder = contract.SaleOrder != null ? new SaleOrderBasicDto
                {
                    Id = contract.SaleOrder.Id,
                    Title = contract.SaleOrder.Title,
                    CustomerId = contract.SaleOrder.CustomerId,
                    Customer = contract.SaleOrder.Customer != null ? new CustomerBasicDto
                    {
                        Id = contract.SaleOrder.Customer.Id,
                        Name = contract.SaleOrder.Customer.Name,
                        CompanyName = contract.SaleOrder.Customer.CompanyName,
                        Email = contract.SaleOrder.Customer.Email,
                        PhoneNumber = contract.SaleOrder.Customer.PhoneNumber
                    } : null,
                    Value = contract.SaleOrder.Value,
                    Probability = contract.SaleOrder.Probability,
                    Services = contract.SaleOrder.SaleOrderServices?.Select(sos => new ServiceItemDto
                    {
                        ServiceId = sos.ServiceId,
                        ServiceName = sos.Service?.Name ?? "",
                        UnitPrice = sos.UnitPrice,
                        Quantity = sos.Quantity,
                        Duration = sos.duration,
                        Template = sos.template,
                        TaxId = sos.Service?.TaxId,
                        Tax = sos.Service?.Tax != null ? new TaxBasicDto
                        {
                            Id = sos.Service.Tax.Id,
                            Rate = sos.Service.Tax.Rate
                        } : null
                    }).ToList() ?? new(),
                    Addons = contract.SaleOrder.SaleOrderAddons?.Select(soa => new AddonItemDto
                    {
                        AddonId = soa.AddonId,
                        AddonName = soa.Addon?.Name ?? "",
                        UnitPrice = soa.UnitPrice,
                        Quantity = soa.Quantity,
                        Duration = soa.duration,
                        Template = soa.template,
                        TaxId = soa.Addon?.TaxId,
                        Tax = soa.Addon?.Tax != null ? new TaxBasicDto
                        {
                            Id = soa.Addon.Tax.Id,
                            Rate = soa.Addon.Tax.Rate
                        } : null
                    }).ToList() ?? new()
                } : null,
                UserId = contract.UserId,
                User = contract.User != null ? new UserBasicDto
                {
                    Id = contract.User.Id,
                    Name = contract.User.Name,
                    Email = contract.User.Email
                } : null,
                NumberContract = contract.NumberContract,
                Status = contract.Status,
                PaymentMethod = contract.PaymentMethod,
                SubTotal = contract.SubTotal,
                TaxAmount = contract.TaxAmount,
                TotalAmount = contract.TotalAmount,
                Expiration = contract.Expiration,
                Notes = contract.Notes,
                CreatedAt = contract.CreatedAt,
                UpdatedAt = contract.UpdatedAt
            };
        }

        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.Id == id);
        }

		// ==================== PREVIEW CONTRACT HTML ====================
		
		// GET: api/Contracts/5/preview
		[HttpGet("{id}/preview")]
		[Authorize]
		public async Task<IActionResult> PreviewContract(int id)
		{
			try
			{
				// Load Contract với đầy đủ relations
				var contract = await _context.Contracts
					.Include(c => c.User)
						.ThenInclude(u => u.Position)
					.Include(c => c.SaleOrder)
						.ThenInclude(so => so!.Customer)
					.Include(c => c.SaleOrder)
						.ThenInclude(so => so!.SaleOrderServices)
							.ThenInclude(sos => sos.Service)
								.ThenInclude(s => s!.Tax)
					.Include(c => c.SaleOrder)
						.ThenInclude(so => so!.SaleOrderAddons)
							.ThenInclude(soa => soa.Addon)
								.ThenInclude(a => a!.Tax)
					.FirstOrDefaultAsync(c => c.Id == id);

				if (contract == null)
					return NotFound(new { message = "Không tìm thấy hợp đồng" });

				if (contract.SaleOrder == null)
					return BadRequest(new { message = "SaleOrder không tồn tại" });

				var customer = contract.SaleOrder.Customer;
				if (customer == null)
					return BadRequest(new { message = "Customer không tồn tại" });

				// Chọn template dựa vào CustomerType
				var templateFileName = customer.CustomerType?.ToLower() == "individual" 
					? "generate_contract_individual.html" 
					: "generate_contract_business.html";
				
				var templatePath = Path.Combine(
					Directory.GetCurrentDirectory(), 
					"wwwroot", 
					"Templates", 
					templateFileName
				);
				
				if (!System.IO.File.Exists(templatePath))
					return NotFound(new { message = $"Không tìm thấy template: {templateFileName}" });

				var htmlTemplate = await System.IO.File.ReadAllTextAsync(templatePath);
				
				// Sử dụng cùng method BindContractDataToTemplate như export PDF
				var htmlContent = BindContractDataToTemplate(htmlTemplate, contract);

				// Trả về HTML content để preview trong browser
				return Content(htmlContent, "text/html");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi preview hợp đồng ID: {ContractId}", id);
				return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
			}
		}

		// ==================== EXPORT CONTRACT PDF ====================

		// Xuất hợp đồng PDF
		[HttpGet("{id}/export-contract")]
		[Authorize]
		public async Task<IActionResult> ExportContract(int id)
		{
			try
			{
				// Bước 1: Lấy dữ liệu Contract với đầy đủ relations
				var contract = await _context.Contracts
					.Include(c => c.User)
						.ThenInclude(u => u.Position)
					.Include(c => c.SaleOrder)
						.ThenInclude(so => so!.Customer)
					.Include(c => c.SaleOrder)
						.ThenInclude(so => so!.SaleOrderServices)
							.ThenInclude(sos => sos.Service)
								.ThenInclude(s => s!.Tax)
					.Include(c => c.SaleOrder)
						.ThenInclude(so => so!.SaleOrderAddons)
							.ThenInclude(soa => soa.Addon)
								.ThenInclude(a => a!.Tax)
					.FirstOrDefaultAsync(c => c.Id == id);

				if (contract == null)
				{
					return NotFound(new { message = "Không tìm thấy hợp đồng" });
				}

				if (contract.SaleOrder == null)
				{
					return NotFound(new { message = "Không tìm thấy thông tin SaleOrder" });
				}

				var customer = contract.SaleOrder.Customer;
				if (customer == null)
				{
					return NotFound(new { message = "Không tìm thấy thông tin khách hàng" });
				}

				// ✅ Kiểm tra file PDF đã tồn tại chưa
				if (!string.IsNullOrEmpty(contract.ContractPdfPath))
				{
					var existingFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", contract.ContractPdfPath);
					
					if (System.IO.File.Exists(existingFilePath))
					{
						_logger.LogInformation("Sử dụng file PDF đã có sẵn: {FilePath}", contract.ContractPdfPath);
						
						var existingPdfBytes = await System.IO.File.ReadAllBytesAsync(existingFilePath);
						var existingFileName = Path.GetFileName(existingFilePath);
						
						return File(existingPdfBytes, "application/pdf", existingFileName);
					}
					else
					{
						_logger.LogWarning("File PDF đã lưu không tồn tại, sẽ tạo mới: {FilePath}", contract.ContractPdfPath);
					}
				}

				// Bước 2: Đọc HTML template dựa vào loại khách hàng
				var templateFileName = customer.CustomerType?.ToLower() == "individual"
					? "generate_contract_individual.html"
					: "generate_contract_business.html";

				var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Templates", templateFileName);

				if (!System.IO.File.Exists(templatePath))
				{
					return NotFound(new { message = $"Không tìm thấy template hợp đồng: {templateFileName}" });
				}

				var htmlTemplate = await System.IO.File.ReadAllTextAsync(templatePath);

				// Bước 3: Bind dữ liệu vào template
				var htmlContent = BindContractDataToTemplate(htmlTemplate, contract);

				// Bước 4: Sử dụng IronPDF để convert HTML sang PDF
				var renderer = new IronPdf.ChromePdfRenderer();

				// Cấu hình renderer - loại bỏ header/footer
				renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
				renderer.RenderingOptions.MarginTop = 0;
				renderer.RenderingOptions.MarginBottom = 0;
				renderer.RenderingOptions.MarginLeft = 0;
				renderer.RenderingOptions.MarginRight = 0;
				renderer.RenderingOptions.CssMediaType = IronPdf.Rendering.PdfCssMediaType.Print;
				renderer.RenderingOptions.PrintHtmlBackgrounds = true;
				renderer.RenderingOptions.CreatePdfFormsFromHtml = false;
				renderer.RenderingOptions.EnableJavaScript = false;

				// Render HTML thành PDF
				var pdf = await Task.Run(() => renderer.RenderHtmlAsPdf(htmlContent));

				// Bước 5: Lưu file PDF vào thư mục
				var fileName = $"HopDong_{contract.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
				var yearMonth = $"{DateTime.Now:yyyy}/{DateTime.Now:MM}";
				var relativeFolderPath = Path.Combine("Contracts", yearMonth);
				var absoluteFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativeFolderPath);

				// Tạo thư mục nếu chưa tồn tại
				if (!Directory.Exists(absoluteFolderPath))
				{
					Directory.CreateDirectory(absoluteFolderPath);
				}

				var relativeFilePath = Path.Combine(relativeFolderPath, fileName);
				var absoluteFilePath = Path.Combine(absoluteFolderPath, fileName);

				// Lấy PDF bytes
				var pdfBytes = pdf.BinaryData;

				// Lưu file vào disk
				await System.IO.File.WriteAllBytesAsync(absoluteFilePath, pdfBytes);

				// ✅ Cập nhật thông tin trong database
				contract.ContractPdfPath = relativeFilePath.Replace("\\", "/"); // Normalize path
				contract.PdfGeneratedAt = DateTime.UtcNow;
				contract.PdfFileSize = pdfBytes.Length;

				await _context.SaveChangesAsync();

				_logger.LogInformation("Đã lưu file PDF: {FilePath}, Size: {FileSize} bytes", relativeFilePath, pdfBytes.Length);

				// Bước 6: Trả về file PDF
				return File(pdfBytes, "application/pdf", fileName);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi xuất hợp đồng PDF cho Contract ID: {ContractId}", id);
				return StatusCode(500, new { message = "Lỗi server khi xuất hợp đồng", error = ex.Message });
			}
		}

		// ==================== REGENERATE CONTRACT PDF ====================

		// Tạo lại hợp đồng PDF (xóa file cũ và tạo file mới)
		[HttpPost("{id}/regenerate-contract")]

		public async Task<IActionResult> RegenerateContract(int id)
		{
			try
			{
				var contract = await _context.Contracts.FindAsync(id);
				
				if (contract == null)
				{
					return NotFound(new { message = "Không tìm thấy hợp đồng" });
				}

				// Xóa file PDF cũ nếu tồn tại
				if (!string.IsNullOrEmpty(contract.ContractPdfPath))
				{
					var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", contract.ContractPdfPath);
					
					if (System.IO.File.Exists(oldFilePath))
					{
						System.IO.File.Delete(oldFilePath);
						_logger.LogInformation("Đã xóa file PDF cũ: {FilePath}", contract.ContractPdfPath);
					}
				}

				// Reset thông tin PDF trong database
				contract.ContractPdfPath = null;
				contract.PdfGeneratedAt = null;
				contract.PdfFileSize = null;
				
				await _context.SaveChangesAsync();

				// Redirect đến ExportContract để tạo file mới
				return await ExportContract(id);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi tạo lại hợp đồng PDF cho Contract ID: {ContractId}", id);
				return StatusCode(500, new { message = "Lỗi server khi tạo lại hợp đồng", error = ex.Message });
			}
		}

		// ==================== QR CODE PAYMENT ====================

		/// <summary>
		/// Tạo URL QR code thanh toán cho hợp đồng
		/// GET /api/Contracts/5/qr-code?paymentType=full100
		/// </summary>
		/// <param name="id">Contract ID</param>
		/// <param name="paymentType">Loại thanh toán: full100 (mặc định), deposit50, final50</param>
		[HttpGet("{id}/qr-code")]
		[Authorize]
		public async Task<ActionResult<object>> GetContractQrCode(int id, [FromQuery] string paymentType = "full100")
		{
			try
			{
				// 1. Lấy thông tin Contract
				var contract = await _context.Contracts
					.Include(c => c.SaleOrder)
						.ThenInclude(so => so!.Customer)
					.FirstOrDefaultAsync(c => c.Id == id);

				if (contract == null)
				{
					return NotFound(new { message = "Không tìm thấy hợp đồng" });
				}

				// 2. Lấy thông tin ngân hàng từ config
				var bankCode = _configuration["Sepay:BankCode"] ?? "MB";
				var accountNumber = _configuration["Sepay:AccountNumber"] ?? "";
				var accountName = _configuration["Sepay:AccountName"] ?? "";

				if (string.IsNullOrEmpty(accountNumber))
				{
					return BadRequest(new { message = "Chưa cấu hình thông tin tài khoản ngân hàng" });
				}

				// 3. Xác định số tiền và nội dung thanh toán dựa trên paymentType
				decimal amount;
				string contentTemplate;
				string paymentTypeDisplay;

				switch (paymentType.ToLower())
				{
					case "deposit50":
						amount = contract.TotalAmount * 0.5m;
						contentTemplate = _configuration["Sepay:PaymentContentTemplates:Deposit50"] ?? "DatCoc50%HopDong{0}";
						paymentTypeDisplay = "Đặt cọc 50%";
						break;

					case "final50":
						amount = contract.TotalAmount * 0.5m;
						contentTemplate = _configuration["Sepay:PaymentContentTemplates:Final50"] ?? "ThanhToan50%HopDong{0}";
						paymentTypeDisplay = "Thanh toán nốt 50%";
						break;

					case "full100":
					default:
						amount = contract.TotalAmount;
						contentTemplate = _configuration["Sepay:PaymentContentTemplates:Full100"] ?? "ThanhToanHopDong{0}";
						paymentTypeDisplay = "Thanh toán 100%";
						break;
				}

				// 4. Tạo nội dung chuyển khoản với số hợp đồng
				var description = string.Format(contentTemplate, contract.NumberContract);
				var encodedDescription = HttpUtility.UrlEncode(description);

				// 5. Tạo URL QR code theo format Sepay
				var qrCodeUrl = $"https://qr.sepay.vn/img?acc={accountNumber}&bank={bankCode}&amount={amount}&des={encodedDescription}";

				_logger.LogInformation("Generated QR code for Contract {ContractId}, PaymentType: {PaymentType}, Amount: {Amount}", 
					id, paymentType, amount);

				// 6. Trả về thông tin đầy đủ
				return Ok(new
				{
					success = true,
					contractId = contract.Id,
					contractNumber = contract.NumberContract,
					paymentType = paymentType.ToLower(),
					paymentTypeDisplay = paymentTypeDisplay,
					qrCodeUrl = qrCodeUrl,
					paymentInfo = new
					{
						bankCode = bankCode,
						bankName = await GetBankNameAsync(bankCode),
						accountNumber = accountNumber,
						accountName = accountName,
						amount = amount,
						amountFormatted = amount.ToString("N0"),
						totalAmount = contract.TotalAmount,
						totalAmountFormatted = contract.TotalAmount.ToString("N0"),
						description = description,
						status = contract.Status
					}
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi tạo QR code cho Contract ID: {ContractId}", id);
				return StatusCode(500, new { message = "Lỗi server khi tạo QR code", error = ex.Message });
			}
		}

		/// <summary>
		/// Helper method: Lấy tên ngân hàng từ mã ngân hàng
		/// </summary>
		private async Task<string> GetBankNameAsync(string bankCode)
		{
			try
			{
				using var httpClient = new HttpClient();
				var response = await httpClient.GetAsync("https://qr.sepay.vn/banks.json");
				
				if (response.IsSuccessStatusCode)
				{
					var json = await response.Content.ReadAsStringAsync();
					var apiResponse = System.Text.Json.JsonSerializer.Deserialize<SepayBankResponse>(json);
					
					if (apiResponse?.Data != null)
					{
						var bank = apiResponse.Data.FirstOrDefault(b => 
							b.Code.Equals(bankCode, StringComparison.OrdinalIgnoreCase));
						
						if (bank != null)
						{
							return $"{bank.Name} ({bank.ShortName})";
						}
					}
				}
				
				// Fallback nếu API lỗi hoặc không tìm thấy
				_logger.LogWarning("Could not fetch bank name from Sepay API for code: {BankCode}", bankCode);
				return bankCode;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error calling Sepay API for bank code: {BankCode}", bankCode);
				return bankCode;
			}
		}

		// DTO classes for Sepay API
		private class SepayBankResponse
		{
			[System.Text.Json.Serialization.JsonPropertyName("no_banks")]
			public string? NoBanks { get; set; }
			
			[System.Text.Json.Serialization.JsonPropertyName("data")]
			public List<SepayBank>? Data { get; set; }
		}

		private class SepayBank
		{
			[System.Text.Json.Serialization.JsonPropertyName("name")]
			public string Name { get; set; } = string.Empty;
			
			[System.Text.Json.Serialization.JsonPropertyName("code")]
			public string Code { get; set; } = string.Empty;
			
			[System.Text.Json.Serialization.JsonPropertyName("bin")]
			public string Bin { get; set; } = string.Empty;
			
			[System.Text.Json.Serialization.JsonPropertyName("short_name")]
			public string ShortName { get; set; } = string.Empty;
			
			[System.Text.Json.Serialization.JsonPropertyName("supported")]
			public bool Supported { get; set; }
		}

        // Helper method: Bind dữ liệu Contract vào template
		private string BindContractDataToTemplate(string template, Contract contract)
		{
			var customer = contract.SaleOrder!.Customer!;
			var now = DateTime.Now;

			// Thông tin hợp đồng cơ bản
			template = template
				.Replace("{{ContractNumber}}", contract.NumberContract.ToString())
				.Replace("{{NumberContract}}", contract.NumberContract.ToString()) // ✅ THÊM MỚI
				.Replace("{{ContractYear}}", contract.CreatedAt.Year.ToString())
				.Replace("{{Day}}", now.Day.ToString())
				.Replace("{{Month}}", now.Month.ToString())
				.Replace("{{Year}}", now.Year.ToString())
				.Replace("{{ContractDate}}", contract.CreatedAt.ToString("dd/MM/yyyy"))
				.Replace("{{ExpirationDate}}", contract.Expiration.ToString("dd/MM/yyyy"))
				.Replace("{{Location}}", "Hà Nội");

			// Thông tin Bên A (Khách hàng)
			template = template
				.Replace("{{CompanyBName}}", customer.CompanyName ?? customer.Name ?? "")
				.Replace("{{CompanyBAddress}}", customer.CompanyAddress ?? customer.Address ?? "")
				.Replace("{{CompanyBTaxCode}}", customer.TaxCode ?? "")
				.Replace("{{CompanyBRepName}}", customer.RepresentativeName ?? customer.Name ?? "")
				.Replace("{{CompanyBRepPosition}}", customer.RepresentativePosition ?? "")
				.Replace("{{CompanyBRepID}}", customer.RepresentativeIdNumber ?? customer.IdNumber ?? "")
				.Replace("{{CompanyBPhone}}", customer.RepresentativePhone ?? customer.PhoneNumber ?? "")
				.Replace("{{CompanyBEmail}}", customer.RepresentativeEmail ?? customer.Email ?? "");

			// ✅ Ngày sinh (nếu là cá nhân) - Để trống nếu không có
			if (customer.BirthDate.HasValue)
			{
				template = template
					.Replace("{{CustomerBirthDay}}", customer.BirthDate.Value.Day.ToString())
					.Replace("{{CustomerBirthMonth}}", customer.BirthDate.Value.Month.ToString())
					.Replace("{{CustomerBirthYear}}", customer.BirthDate.Value.Year.ToString());
			}
			else
			{
				// ✅ Nếu không có ngày sinh, thay thế bằng chuỗi rỗng
				template = template
					.Replace("{{CustomerBirthDay}}", "")
					.Replace("{{CustomerBirthMonth}}", "")
					.Replace("{{CustomerBirthYear}}", "");
			}

			// ✅ Ngày thành lập (nếu là doanh nghiệp) - Để trống nếu không có
			if (customer.EstablishedDate.HasValue)
			{
				template = template
					.Replace("{{CompanyBEstablishedDay}}", customer.EstablishedDate.Value.Day.ToString())
					.Replace("{{CompanyBEstablishedMonth}}", customer.EstablishedDate.Value.Month.ToString())
					.Replace("{{CompanyBEstablishedYear}}", customer.EstablishedDate.Value.Year.ToString())
					.Replace("{{CompanyBEstablishedDate}}", customer.EstablishedDate.Value.ToString("dd/MM/yyyy"));
			}
			else
			{
				// ✅ Nếu không có ngày thành lập, thay thế bằng chuỗi rỗng
				template = template
					.Replace("{{CompanyBEstablishedDay}}", "")
					.Replace("{{CompanyBEstablishedMonth}}", "")
					.Replace("{{CompanyBEstablishedYear}}", "")
					.Replace("{{CompanyBEstablishedDate}}", "");
			}

			// Thông tin tài chính từ Contract
			template = template
				.Replace("{{SubTotal}}", contract.SubTotal.ToString("N0"))
				.Replace("{{Discount}}", "0")
				.Replace("{{TaxAmount}}", contract.TaxAmount.ToString("N0"))
				.Replace("{{TotalAmount}}", contract.TotalAmount.ToString("N0"))
				.Replace("{{NetAmount}}", contract.TotalAmount.ToString("N0"))
				.Replace("{{AmountInWords}}", ConvertNumberToWords(contract.TotalAmount))
				.Replace("{{PaymentMethod}}", contract.PaymentMethod ?? "Chuyển khoản")
				.Replace("{{Status}}", contract.Status)
				.Replace("{{Notes}}", contract.Notes ?? "");

			// Không còn thông tin VAT từ SaleOrder.Tax
			template = template.Replace("{{VATRate}}", "0");
			template = template.Replace("{{VATAmount}}", "0");

			// Thông tin nhân viên phụ trách
			if (contract.User != null)
			{
				template = template
					.Replace("{{UserName}}", contract.User.Name)
					.Replace("{{UserEmail}}", contract.User.Email)
					.Replace("{{UserPhone}}", contract.User.PhoneNumber ?? "")
					.Replace("{{UserPosition}}", contract.User.Position?.PositionName ?? "Nhân viên");
			}

			// Tạo bảng dịch vụ từ SaleOrder
			var itemsHtml = GenerateContractItemsTableFromContract(contract);
			template = template.Replace("{{Items}}", itemsHtml);

			return template;
		}

		// Helper method: Tạo bảng danh sách dịch vụ từ Contract
		private string GenerateContractItemsTableFromContract(Contract contract)
		{
			var items = new System.Text.StringBuilder();
			var index = 1;
			decimal subTotal = 0;
			decimal totalTax = 0;

			// Thêm Services từ SaleOrder
			if (contract.SaleOrder!.SaleOrderServices != null && contract.SaleOrder.SaleOrderServices.Any())
			{
				foreach (var sos in contract.SaleOrder.SaleOrderServices)
				{
					var service = sos.Service;
					var quantity = sos.Quantity ?? (service?.Quantity ?? 1);
					var lineTotal = sos.UnitPrice * quantity;
					subTotal += lineTotal;

					// Lấy thông tin thuế
					var taxRate = service?.Tax?.Rate ?? 0f;
					var taxDisplay = taxRate > 0 ? $"{taxRate}%" : "0%";
					
					// Tính thuế cho dòng này
					var lineTax = taxRate > 0 ? lineTotal * (decimal)taxRate / 100m : 0;
					totalTax += lineTax;

					items.AppendLine($@"
					<tr>
						<td style='text-align: center; border: 1px solid #000'>{index++}</td>
						<td style='border: 1px solid #000'>{service?.Name ?? ""}</td>
						<td style='border: 1px solid #000'>{sos.template ?? ""}</td>
						<td style='text-align: center; border: 1px solid #000'>{taxDisplay}</td>
						<td style='text-align: center; border: 1px solid #000'>{sos.duration} tháng</td>
						<td style='text-align: right; border: 1px solid #000'>{sos.UnitPrice:N0}</td>
						<td style='text-align: right; border: 1px solid #000'>{lineTotal:N0}</td>
					</tr>");
				}
			}

			// Thêm Addons từ SaleOrder
			if (contract.SaleOrder.SaleOrderAddons != null && contract.SaleOrder.SaleOrderAddons.Any())
			{
				foreach (var soa in contract.SaleOrder.SaleOrderAddons)
				{
					var addon = soa.Addon;
					var quantity = soa.Quantity ?? (addon?.Quantity ?? 1);
					var lineTotal = soa.UnitPrice * quantity;
					subTotal += lineTotal;

					// Lấy thông tin thuế
					var taxRate = addon?.Tax?.Rate ?? 0f;
					var taxDisplay = taxRate > 0 ? $"{taxRate}%" : "0%";
					
					// Tính thuế cho dòng này
					var lineTax = taxRate > 0 ? lineTotal * (decimal)taxRate / 100m : 0;
					totalTax += lineTax;

					items.AppendLine($@"
					<tr>
						<td style='text-align: center; border: 1px solid #000'>{index++}</td>
						<td style='border: 1px solid #000'>{addon?.Name ?? ""}</td>
						<td style='border: 1px solid #000'>{soa.template ?? ""}</td>
						<td style='text-align: center; border: 1px solid #000'>{taxDisplay}</td>
						<td style='text-align: center; border: 1px solid #000'>{soa.duration} tháng</td>
						<td style='text-align: right; border: 1px solid #000'>{soa.UnitPrice:N0}</td>
						<td style='text-align: right; border: 1px solid #000'>{lineTotal:N0}</td>
					</tr>");
				}	
			}

			// Thêm các dòng tổng hợp
			items.AppendLine($@"
			<tr style='background-color: #f9f9f9'>
				<td colspan='6' style='text-align: right; border: 1px solid #000;'>
					<b>Cộng (chưa thuế)</b>
				</td>
				<td style='text-align: right; border: 1px solid #000;'>
					<b>{subTotal:N0}</b>
				</td>
			</tr>
			<tr style='background-color: #f9f9f9'>
				<td colspan='6' style='text-align: right; border: 1px solid #000;'>
					<b>Thuế VAT</b>
				</td>
				<td style='text-align: right; border: 1px solid #000;'>
					<b>{totalTax:N0}</b>
				</td>
			</tr>
			<tr style='background-color: #f9f9f9'>
				<td colspan='6' style='text-align: right; border: 1px solid #000;'>
					<b>Giảm giá</b>
				</td>
				<td style='text-align: right; border: 1px solid #000;'>
					<b>0</b>
				</td>
			</tr>");


			// Tổng Cồng
			var finalTotal = subTotal + totalTax;
			
			items.AppendLine($@"
			<tr style='background-color: #e8f4fd'>
				<td colspan='6' style='text-align: right; border: 1px solid #000;'>
					<b>Thanh Toán</b>
				</td>
				<td style='text-align: right; border: 1px solid #000;'>
					<b>{finalTotal:N0}</b>
				</td>
			</tr>
			<tr style='border: none;'>
				<td colspan='7' style='text-align: right; border: none;'>
					<b>Bằng chữ: {ConvertNumberToWords(finalTotal)}</b>
				</td>
			</tr>");



			// Nếu không có services/addons, hiển thị title của SaleOrder
			if ((contract.SaleOrder.SaleOrderServices == null || !contract.SaleOrder.SaleOrderServices.Any()) &&
				(contract.SaleOrder.SaleOrderAddons == null || !contract.SaleOrder.SaleOrderAddons.Any()))
			{
				items.Clear();
				items.AppendLine($@"
				<tr>
					<td style='text-align: center; border: 1px solid #000'>1</td>
					<td style='border: 1px solid #000'>{contract.SaleOrder.Title}</td>
					<td style='border: 1px solid #000'>N/A</td>
					<td style='text-align: center; border: 1px solid #000'>0%</td>
					<td style='text-align: center; border: 1px solid #000'>N/A</td>
					<td style='text-align: right; border: 1px solid #000'>{contract.SubTotal:N0}</td>
					<td style='text-align: right; border: 1px solid #000'>{contract.SubTotal:N0}</td>
				</tr>
				<tr style='background-color: #f9f9f9'>
					<td colspan='6' style='text-align: right; border: 1px solid #000;'>
						<b>Cộng (chưa thuế)</b>
					</td>
					<td style='text-align: right; border: 1px solid #000;'>
						<b>{contract.SubTotal:N0}</b>
					</td>
				</tr>
				<tr style='background-color: #f9f9f9'>
					<td colspan='6' style='text-align: right; border: 1px solid #000;'>
						<b>Thuế VAT</b>
					</td>
					<td style='text-align: right; border: 1px solid #000;'>
						<b>{contract.TaxAmount:N0}</b>
					</td>
				</tr>
				<tr style='background-color: #f9f9f9'>
					<td colspan='6' style='text-align: right; border: 1px solid #000;'>
						<b>Giảm giá</b>
					</td>
					<td style='text-align: right; border: 1px solid #000;'>
						<b>0</b>
					</td>
				</tr>
				<tr style='background-color: #e8f4fd'>
					<td colspan='6' style='text-align: right; border: 1px solid #000;'>
						<b>Thanh Toán</b>
					</td>
					<td style='text-align: right; border: 1px solid #000;'>
						<b>{contract.TotalAmount:N0}</b>
					</td>
				</tr>
				<tr style='border: none;'>
					<td colspan='7' style='text-align: right; border: none;'>
						<b>Bằng chữ: {ConvertNumberToWords(contract.TotalAmount)}</b>
					</td>
				</tr>");
			}

			return items.ToString();
		}

		// Helper method: Convert số thành chữ (tiếng Việt)
		private string ConvertNumberToWords(decimal number)
		{
			if (number == 0) return "Không đồng";

			var units = new[] { "", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
			var scales = new[] { "", "nghìn", "triệu", "tỷ" };

			var numStr = ((long)number).ToString();
			var result = new System.Text.StringBuilder();
			var scaleIndex = 0;

			while (numStr.Length > 0)
			{
				var groupSize = Math.Min(3, numStr.Length);
				var group = numStr.Substring(numStr.Length - groupSize);
				numStr = numStr.Substring(0, numStr.Length - groupSize);

				if (int.Parse(group) > 0)
				{
					var groupWords = ConvertGroupToWords(int.Parse(group), units);
					if (result.Length > 0)
						result.Insert(0, " ");
					result.Insert(0, scales[scaleIndex]);
					result.Insert(0, " ");
					result.Insert(0, groupWords);
				}

				scaleIndex++;
			}

			return char.ToUpper(result[0]) + result.ToString().Substring(1).Trim() + " đồng chẵn";
		}

		private string ConvertGroupToWords(int number, string[] units)
		{
			var hundred = number / 100;
			var ten = (number % 100) / 10;
			var unit = number % 10;

			var result = new System.Text.StringBuilder();

			if (hundred > 0)
			{
				result.Append(units[hundred]);
				result.Append(" trăm");
			}

			if (ten > 1)
			{
				if (result.Length > 0) result.Append(" ");
				result.Append(units[ten]);
				result.Append(" mươi");
			}
			else if (ten == 1)
			{
				if (result.Length > 0) result.Append(" ");
				result.Append("mười");
			}

			if (unit > 0)
			{
				if (result.Length > 0) result.Append(" ");
				result.Append(units[unit]);
			}

			return result.ToString();
		}

	}
}
