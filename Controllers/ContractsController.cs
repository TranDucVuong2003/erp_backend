using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using erp_backend.Data;
using erp_backend.Models;

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
        public async Task<ActionResult<IEnumerable<Contract>>> GetContracts()
        {
            return await _context.Contracts
                .Include(c => c.Customer)
                .Include(c => c.User)
                .Include(c => c.Service)
                .Include(c => c.Addon)
                .Include(c => c.Tax)
                .ToListAsync();
        }

        // GET: api/Contracts/5
        [HttpGet("{id}")]
        //[Authorize]
        public async Task<ActionResult<Contract>> GetContract(int id)
        {
            var contract = await _context.Contracts
                .Include(c => c.Customer)
                .Include(c => c.User)
                .Include(c => c.Service)
                .Include(c => c.Addon)
                .Include(c => c.Tax)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
            {
                return NotFound(new { message = "Không tìm thấy hợp đồng" });
            }

            return contract;
        }

        // POST: api/Contracts
        [HttpPost]
        //[Authorize]
        public async Task<ActionResult<Contract>> CreateContract(Contract contract)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Kiểm tra customer tồn tại
                var customerExists = await _context.Customers.AnyAsync(c => c.Id == contract.CustomerId);
                if (!customerExists)
                {
                    return BadRequest(new { message = "Customer không tồn tại" });
                }

                // Kiểm tra user tồn tại
                var userExists = await _context.Users.AnyAsync(u => u.Id == contract.UserId);
                if (!userExists)
                {
                    return BadRequest(new { message = "User không tồn tại" });
                }

                // Kiểm tra service tồn tại (nếu có)
                if (contract.ServiceId.HasValue)
                {
                    var serviceExists = await _context.Services.AnyAsync(s => s.Id == contract.ServiceId.Value);
                    if (!serviceExists)
                    {
                        return BadRequest(new { message = "Service không tồn tại" });
                    }
                }

                // Kiểm tra addon tồn tại (nếu có)
                if (contract.AddonsId.HasValue)
                {
                    var addonExists = await _context.Addons.AnyAsync(a => a.Id == contract.AddonsId.Value);
                    if (!addonExists)
                    {
                        return BadRequest(new { message = "Addon không tồn tại" });
                    }
                }

                // Kiểm tra tax tồn tại (nếu có)
                if (contract.TaxId.HasValue)
                {
                    var taxExists = await _context.Taxes.AnyAsync(t => t.Id == contract.TaxId.Value);
                    if (!taxExists)
                    {
                        return BadRequest(new { message = "Tax không tồn tại" });
                    }
                }

                // Tính toán tự động SubTotal, TaxAmount, TotalAmount
                decimal subTotal = 0;

                if (contract.ServiceId.HasValue)
                {
                    var service = await _context.Services.FindAsync(contract.ServiceId.Value);
                    if (service != null)
                    {
                        subTotal += service.Price * (service.Quantity ?? 1);
                    }
                }

                if (contract.AddonsId.HasValue)
                {
                    var addon = await _context.Addons.FindAsync(contract.AddonsId.Value);
                    if (addon != null)
                    {
                        subTotal += addon.Price * (addon.Quantity ?? 1);
                    }
                }

                contract.SubTotal = subTotal;

                // Tính thuế
                if (contract.TaxId.HasValue)
                {
                    var tax = await _context.Taxes.FindAsync(contract.TaxId.Value);
                    if (tax != null)
                    {
                        contract.TaxAmount = subTotal * tax.Rate / 100;
                    }
                }

                contract.TotalAmount = contract.SubTotal + contract.TaxAmount;
                contract.CreatedAt = DateTime.UtcNow;

                _context.Contracts.Add(contract);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetContract), new { id = contract.Id }, contract);
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
        public async Task<ActionResult<Contract>> UpdateContract(int id, Contract contract)
        {
            try
            {
                if (id != contract.Id)
                {
                    return BadRequest(new { message = "ID không khớp" });
                }

                // Kiểm tra contract có tồn tại không
                var existingContract = await _context.Contracts.FindAsync(id);
                if (existingContract == null)
                {
                    return NotFound(new { message = "Không tìm thấy hợp đồng" });
                }

                // Kiểm tra customer tồn tại
                if (contract.CustomerId != existingContract.CustomerId)
                {
                    var customerExists = await _context.Customers.AnyAsync(c => c.Id == contract.CustomerId);
                    if (!customerExists)
                    {
                        return BadRequest(new { message = "Customer không tồn tại" });
                    }
                }

                // Kiểm tra user tồn tại
                if (contract.UserId != existingContract.UserId)
                {
                    var userExists = await _context.Users.AnyAsync(u => u.Id == contract.UserId);
                    if (!userExists)
                    {
                        return BadRequest(new { message = "User không tồn tại" });
                    }
                }

                // Kiểm tra service tồn tại (nếu có)
                if (contract.ServiceId.HasValue)
                {
                    var serviceExists = await _context.Services.AnyAsync(s => s.Id == contract.ServiceId.Value);
                    if (!serviceExists)
                    {
                        return BadRequest(new { message = "Service không tồn tại" });
                    }
                }

                // Kiểm tra addon tồn tại (nếu có)
                if (contract.AddonsId.HasValue)
                {
                    var addonExists = await _context.Addons.AnyAsync(a => a.Id == contract.AddonsId.Value);
                    if (!addonExists)
                    {
                        return BadRequest(new { message = "Addon không tồn tại" });
                    }
                }

                // Kiểm tra tax tồn tại (nếu có)
                if (contract.TaxId.HasValue)
                {
                    var taxExists = await _context.Taxes.AnyAsync(t => t.Id == contract.TaxId.Value);
                    if (!taxExists)
                    {
                        return BadRequest(new { message = "Tax không tồn tại" });
                    }
                }

                // Cập nhật các trường
                existingContract.Name = contract.Name;
                existingContract.CustomerId = contract.CustomerId;
                existingContract.UserId = contract.UserId;
                existingContract.ServiceId = contract.ServiceId;
                existingContract.AddonsId = contract.AddonsId;
                existingContract.TaxId = contract.TaxId;
                existingContract.Status = contract.Status;
                existingContract.PaymentMethod = contract.PaymentMethod;
                existingContract.SubTotal = contract.SubTotal;
                existingContract.TaxAmount = contract.TaxAmount;
                existingContract.TotalAmount = contract.TotalAmount;
                existingContract.Expiration = contract.Expiration;
                existingContract.Notes = contract.Notes;
                existingContract.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Load navigation properties để trả về
                await _context.Entry(existingContract)
                    .Reference(c => c.Customer)
                    .LoadAsync();
                await _context.Entry(existingContract)
                    .Reference(c => c.User)
                    .LoadAsync();
                await _context.Entry(existingContract)
                    .Reference(c => c.Service)
                    .LoadAsync();
                await _context.Entry(existingContract)
                    .Reference(c => c.Addon)
                    .LoadAsync();
                await _context.Entry(existingContract)
                    .Reference(c => c.Tax)
                    .LoadAsync();

                // Trả về contract sau khi đã cập nhật
                return Ok(existingContract);
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

        // GET: api/Contracts/customer/5
        [HttpGet("customer/{customerId}")]
        //[Authorize]
        public async Task<ActionResult<IEnumerable<Contract>>> GetContractsByCustomer(int customerId)
        {
            return await _context.Contracts
                .Where(c => c.CustomerId == customerId)
                .Include(c => c.Service)
                .Include(c => c.Addon)
                .Include(c => c.Tax)
                .ToListAsync();
        }

        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.Id == id);
        }
    }
}
