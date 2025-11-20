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
    [Authorize]
    public class MatchedTransactionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MatchedTransactionsController> _logger;

        public MatchedTransactionsController(ApplicationDbContext context, ILogger<MatchedTransactionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// L?y danh sách t?t c? giao d?ch ?ã match
        /// GET: api/matchedtransactions
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MatchedTransactionResponse>>> GetMatchedTransactions(
            [FromQuery] int? contractId = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var query = _context.MatchedTransactions
                .Include(mt => mt.Contract)
                .Include(mt => mt.MatchedByUser)
                .AsQueryable();

            // L?c theo contractId
            if (contractId.HasValue)
            {
                query = query.Where(mt => mt.ContractId == contractId.Value);
            }

            // L?c theo status
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(mt => mt.Status == status);
            }

            // L?c theo kho?ng th?i gian
            if (fromDate.HasValue)
            {
                query = query.Where(mt => mt.TransactionDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(mt => mt.TransactionDate <= toDate.Value);
            }

            var matchedTransactions = await query
                .OrderByDescending(mt => mt.MatchedAt)
                .ToListAsync();

            var response = matchedTransactions.Select(mt => new MatchedTransactionResponse
            {
                Id = mt.Id,
                TransactionId = mt.TransactionId,
                ContractId = mt.ContractId,
                Amount = mt.Amount,
                ReferenceNumber = mt.ReferenceNumber,
                Status = mt.Status,
                TransactionDate = mt.TransactionDate,
                MatchedAt = mt.MatchedAt,
                TransactionContent = mt.TransactionContent,
                BankBrandName = mt.BankBrandName,
                AccountNumber = mt.AccountNumber,
                MatchedByUserId = mt.MatchedByUserId,
                MatchedByUserName = mt.MatchedByUser?.Name,
                Notes = mt.Notes,
                Contract = mt.Contract != null ? new ContractBasicInfo
                {
                    Id = mt.Contract.Id,
                    NumberContract = mt.Contract.NumberContract,
                    Status = mt.Contract.Status,
                    TotalAmount = mt.Contract.TotalAmount,
                    Expiration = mt.Contract.Expiration
                } : null
            }).ToList();

            return Ok(response);
        }

        /// <summary>
        /// L?y chi ti?t giao d?ch ?ã match theo ID
        /// GET: api/matchedtransactions/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<MatchedTransactionResponse>> GetMatchedTransaction(int id)
        {
            var matchedTransaction = await _context.MatchedTransactions
                .Include(mt => mt.Contract)
                .Include(mt => mt.MatchedByUser)
                .FirstOrDefaultAsync(mt => mt.Id == id);

            if (matchedTransaction == null)
            {
                return NotFound(new { message = "Không tìm th?y giao d?ch ?ã match" });
            }

            var response = new MatchedTransactionResponse
            {
                Id = matchedTransaction.Id,
                TransactionId = matchedTransaction.TransactionId,
                ContractId = matchedTransaction.ContractId,
                Amount = matchedTransaction.Amount,
                ReferenceNumber = matchedTransaction.ReferenceNumber,
                Status = matchedTransaction.Status,
                TransactionDate = matchedTransaction.TransactionDate,
                MatchedAt = matchedTransaction.MatchedAt,
                TransactionContent = matchedTransaction.TransactionContent,
                BankBrandName = matchedTransaction.BankBrandName,
                AccountNumber = matchedTransaction.AccountNumber,
                MatchedByUserId = matchedTransaction.MatchedByUserId,
                MatchedByUserName = matchedTransaction.MatchedByUser?.Name,
                Notes = matchedTransaction.Notes,
                Contract = matchedTransaction.Contract != null ? new ContractBasicInfo
                {
                    Id = matchedTransaction.Contract.Id,
                    NumberContract = matchedTransaction.Contract.NumberContract,
                    Status = matchedTransaction.Contract.Status,
                    TotalAmount = matchedTransaction.Contract.TotalAmount,
                    Expiration = matchedTransaction.Contract.Expiration
                } : null
            };

            return Ok(response);
        }



        /// T?o match payment m?i
        /// POST: api/matchedtransactions
        [HttpPost]
        public async Task<ActionResult<MatchedTransactionResponse>> CreateMatchedTransaction(
            [FromBody] MatchPaymentRequest request)
        {
            try
            {
                // Validate
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Ki?m tra transaction ?ã ???c match ch?a
                var existingMatch = await _context.MatchedTransactions
                    .FirstOrDefaultAsync(mt => mt.TransactionId == request.TransactionId);

                if (existingMatch != null)
                {
                    return BadRequest(new { message = $"Giao d?ch {request.TransactionId} ?ã ???c match tr??c ?ó" });
                }

                // Ki?m tra contract có t?n t?i không
                var contractExists = await _context.Contracts.AnyAsync(c => c.Id == request.ContractId);
                if (!contractExists)
                {
                    return BadRequest(new { message = "Contract không t?n t?i" });
                }

                // L?y UserId t? JWT token
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userid");
                int? matchedByUserId = null;
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    matchedByUserId = userId;
                    _logger.LogInformation($"User {userId} is matching transaction {request.TransactionId} to contract {request.ContractId}");
                }

                // T?o matched transaction
                var matchedTransaction = new MatchedTransaction
                {
                    TransactionId = request.TransactionId,
                    ContractId = request.ContractId,
                    Amount = request.Amount,
                    ReferenceNumber = request.ReferenceNumber,
                    Status = request.Status,
                    TransactionDate = request.TransactionDate,
                    MatchedAt = DateTime.UtcNow,
                    TransactionContent = request.TransactionContent,
                    BankBrandName = request.BankBrandName,
                    AccountNumber = request.AccountNumber,
                    MatchedByUserId = matchedByUserId,
                    Notes = request.Notes
                };

                _context.MatchedTransactions.Add(matchedTransaction);
                await _context.SaveChangesAsync();

                // Load l?i ?? l?y navigation properties
                var savedTransaction = await _context.MatchedTransactions
                    .Include(mt => mt.Contract)
                    .Include(mt => mt.MatchedByUser)
                    .FirstOrDefaultAsync(mt => mt.Id == matchedTransaction.Id);

                var response = new MatchedTransactionResponse
                {
                    Id = savedTransaction!.Id,
                    TransactionId = savedTransaction.TransactionId,
                    ContractId = savedTransaction.ContractId,
                    Amount = savedTransaction.Amount,
                    ReferenceNumber = savedTransaction.ReferenceNumber,
                    Status = savedTransaction.Status,
                    TransactionDate = savedTransaction.TransactionDate,
                    MatchedAt = savedTransaction.MatchedAt,
                    TransactionContent = savedTransaction.TransactionContent,
                    BankBrandName = savedTransaction.BankBrandName,
                    AccountNumber = savedTransaction.AccountNumber,
                    MatchedByUserId = savedTransaction.MatchedByUserId,
                    MatchedByUserName = savedTransaction.MatchedByUser?.Name,
                    Notes = savedTransaction.Notes,
                    Contract = savedTransaction.Contract != null ? new ContractBasicInfo
                    {
                        Id = savedTransaction.Contract.Id,
                        NumberContract = savedTransaction.Contract.NumberContract,
                        Status = savedTransaction.Contract.Status,
                        TotalAmount = savedTransaction.Contract.TotalAmount,
                        Expiration = savedTransaction.Contract.Expiration
                    } : null
                };

                return CreatedAtAction(nameof(GetMatchedTransaction), new { id = savedTransaction.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi t?o matched transaction");
                return StatusCode(500, new { message = "L?i server khi t?o matched transaction", error = ex.Message });
            }
        }



        public class UpdateMatchedTransactionRequest
        {
            public string? Status { get; set; }
            public string? Notes { get; set; }
        }
    }
}
