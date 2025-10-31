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
    public class ContractsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContractsController> _logger;

        public ContractsController(ApplicationDbContext context, ILogger<ContractsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Contracts
        [HttpGet]
        //[Authorize]
        public async Task<ActionResult<IEnumerable<ContractResponse>>> GetContracts()
        {
            var contracts = await _context.Contracts
                .Include(c => c.User)
                .Include(c => c.SaleOrder)
                    .ThenInclude(so => so!.Customer)
                .Include(c => c.SaleOrder)
                    .ThenInclude(so => so!.Tax)
                .Include(c => c.SaleOrder)
                    .ThenInclude(so => so!.SaleOrderServices)
                        .ThenInclude(sos => sos.Service)
                .Include(c => c.SaleOrder)
                    .ThenInclude(so => so!.SaleOrderAddons)
                        .ThenInclude(soa => soa.Addon)
                .ToListAsync();

            var response = contracts.Select(c => MapToContractResponse(c));

            return Ok(response);
        }

        // GET: api/Contracts/5
        [HttpGet("{id}")]
        //[Authorize]
        public async Task<ActionResult<ContractResponse>> GetContract(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.User)
                .Include(c => c.SaleOrder)
                    .ThenInclude(so => so!.Customer)
                .Include(c => c.SaleOrder)
                    .ThenInclude(so => so!.Tax)
                .Include(c => c.SaleOrder)
                    .ThenInclude(so => so!.SaleOrderServices)
                        .ThenInclude(sos => sos.Service)
                .Include(c => c.SaleOrder)
                    .ThenInclude(so => so!.SaleOrderAddons)
                        .ThenInclude(soa => soa.Addon)
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
        //[Authorize]
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
                    .Include(so => so.Tax)
                    .Include(so => so.SaleOrderServices)
                        .ThenInclude(sos => sos.Service)
                    .Include(so => so.SaleOrderAddons)
                        .ThenInclude(soa => soa.Addon)
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

                // Tính toán tự động SubTotal, TaxAmount, TotalAmount từ SaleOrder
                decimal subTotal = saleOrder.Value;
                decimal taxAmount = 0;

                // Tính thuế từ SaleOrder
                if (saleOrder.TaxId.HasValue && saleOrder.Tax != null)
                {
                    taxAmount = subTotal * saleOrder.Tax.Rate / 100;
                }

                decimal totalAmount = subTotal + taxAmount;

                // Tạo Contract
                var contract = new Contract
                {
                    SaleOrderId = request.SaleOrderId,
                    UserId = request.UserId,
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
                        .ThenInclude(so => so!.Tax)
                    .Include(c => c.SaleOrder)
                        .ThenInclude(so => so!.SaleOrderServices)
                            .ThenInclude(sos => sos.Service)
                    .Include(c => c.SaleOrder)
                        .ThenInclude(so => so!.SaleOrderAddons)
                            .ThenInclude(soa => soa.Addon)
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
        //[Authorize]
        public async Task<ActionResult<ContractResponse>> UpdateContract(int id, [FromBody] CreateContractRequest request)
        {
            try
            {
                // Kiểm tra contract có tồn tại không
                var existingContract = await _context.Contracts.FindAsync(id);
                if (existingContract == null)
                {
                    return NotFound(new { message = "Không tìm thấy hợp đồng" });
                }

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

                // Cập nhật các trường
                existingContract.SaleOrderId = request.SaleOrderId;
                existingContract.UserId = request.UserId;
                existingContract.Status = request.Status;
                existingContract.PaymentMethod = request.PaymentMethod;
                existingContract.Expiration = request.Expiration;
                existingContract.Notes = request.Notes;
                existingContract.UpdatedAt = DateTime.UtcNow;

                // Tính lại SubTotal, TaxAmount, TotalAmount nếu SaleOrder thay đổi
                if (request.SaleOrderId != existingContract.SaleOrderId)
                {
                    var saleOrder = await _context.SaleOrders
                        .Include(so => so.Tax)
                        .FirstAsync(so => so.Id == request.SaleOrderId);

                    existingContract.SubTotal = saleOrder.Value;
                    existingContract.TaxAmount = saleOrder.TaxId.HasValue && saleOrder.Tax != null
                        ? saleOrder.Value * saleOrder.Tax.Rate / 100
                        : 0;
                    existingContract.TotalAmount = existingContract.SubTotal + existingContract.TaxAmount;
                }

                await _context.SaveChangesAsync();

                // Load lại contract với đầy đủ navigation properties
                var contract = await _context.Contracts
                    .Include(c => c.User)
                    .Include(c => c.SaleOrder)
                        .ThenInclude(so => so!.Customer)
                    .Include(c => c.SaleOrder)
                        .ThenInclude(so => so!.Tax)
                    .Include(c => c.SaleOrder)
                        .ThenInclude(so => so!.SaleOrderServices)
                            .ThenInclude(sos => sos.Service)
                    .Include(c => c.SaleOrder)
                        .ThenInclude(so => so!.SaleOrderAddons)
                            .ThenInclude(soa => soa.Addon)
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
        //[Authorize]
        public async Task<IActionResult> DeleteContract(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
            {
                return NotFound(new { message = "Không tìm thấy hợp đồng" });
            }

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Contracts/saleorder/5
        [HttpGet("saleorder/{saleOrderId}")]
        //[Authorize]
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
                    TaxId = contract.SaleOrder.TaxId,
                    Tax = contract.SaleOrder.Tax != null ? new TaxBasicDto
                    {
                        Id = contract.SaleOrder.Tax.Id,
                        Rate = contract.SaleOrder.Tax.Rate
                    } : null,
                    Services = contract.SaleOrder.SaleOrderServices?.Select(sos => new ServiceItemDto
                    {
                        ServiceId = sos.ServiceId,
                        ServiceName = sos.Service?.Name ?? "",
                        UnitPrice = sos.UnitPrice,
                        Quantity = sos.Quantity
                    }).ToList() ?? new(),
                    Addons = contract.SaleOrder.SaleOrderAddons?.Select(soa => new AddonItemDto
                    {
                        AddonId = soa.AddonId,
                        AddonName = soa.Addon?.Name ?? "",
                        UnitPrice = soa.UnitPrice,
                        Quantity = soa.Quantity
                    }).ToList() ?? new()
                } : null,
                UserId = contract.UserId,
                User = contract.User != null ? new UserBasicDto
                {
                    Id = contract.User.Id,
                    Name = contract.User.Name,
                    Email = contract.User.Email
                } : null,
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
    }
}
