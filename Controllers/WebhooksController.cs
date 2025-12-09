using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using erp_backend.Data;
using erp_backend.Models;
using erp_backend.Models.DTOs;
using erp_backend.Services;
using System.Text.RegularExpressions;

namespace erp_backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class WebhooksController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<WebhooksController> _logger;
		private readonly IKpiCalculationService _kpiCalculationService;

		public WebhooksController(
			ApplicationDbContext context,
			ILogger<WebhooksController> logger,
			IKpiCalculationService kpiCalculationService)
		{
			_context = context;
			_logger = logger;
			_kpiCalculationService = kpiCalculationService;
		}

		/// <summary>
		/// Webhook endpoint nh?n thông báo t? Sepay khi có giao d?ch m?i
		/// POST /api/webhooks/sepay-payment
		/// </summary>
		[HttpPost("sepay-payment")]
		public async Task<IActionResult> SepayPaymentWebhook([FromBody] SepayWebhookPayload payload)
		{
			try
			{
				_logger.LogInformation("?? Nh?n webhook t? Sepay: TransactionId={TransactionId}, Amount={Amount}, Content={Content}",
					payload.TransactionId, payload.Amount, payload.Content);

				// 1. Ki?m tra xem transaction ?ã ???c x? lý ch?a
				var existingTransaction = await _context.MatchedTransactions
					.FirstOrDefaultAsync(mt => mt.TransactionId == payload.TransactionId);

				if (existingTransaction != null)
				{
					_logger.LogWarning("?? Transaction {TransactionId} ?ã ???c x? lý tr??c ?ó", payload.TransactionId);
					return Ok(new { message = "Transaction already processed", processed = false });
				}

				// 2. Parse s? h?p ??ng và lo?i thanh toán t? n?i dung chuy?n kho?n
				var (contractNumber, paymentType) = ExtractContractNumberAndPaymentType(payload.Content);

				if (!contractNumber.HasValue)
				{
					_logger.LogWarning("?? Không tìm th?y s? h?p ??ng trong n?i dung: {Content}", payload.Content);
					await SaveUnmatchedTransaction(payload);
					return Ok(new { message = "Cannot extract contract number", processed = false });
				}

				// 3. Tìm Contract theo NumberContract
				var contract = await _context.Contracts
					.Include(c => c.SaleOrder)
					.FirstOrDefaultAsync(c => c.NumberContract == contractNumber.Value);

				if (contract == null)
				{
					_logger.LogWarning("?? Không tìm th?y Contract v?i NumberContract={ContractNumber}", contractNumber.Value);
					await SaveUnmatchedTransaction(payload, contractNumber.Value);
					return Ok(new { message = "Contract not found", processed = false });
				}

				// 4. Xác ??nh s? ti?n k? v?ng d?a trên lo?i thanh toán
				decimal expectedAmount;
				string paymentTypeDescription;

				switch (paymentType)
				{
					case "deposit50":
						expectedAmount = contract.TotalAmount * 0.5m;
						paymentTypeDescription = "??t c?c 50%";
						break;
					case "final50":
						expectedAmount = contract.TotalAmount * 0.5m;
						paymentTypeDescription = "Thanh toán n?t 50%";
						break;
					case "full100":
					default:
						expectedAmount = contract.TotalAmount;
						paymentTypeDescription = "Thanh toán 100%";
						break;
				}

				// 5. Ki?m tra s? ti?n có kh?p không (cho phép sai l?ch 1%)
				var tolerance = expectedAmount * 0.01m; // 1% sai l?ch
				var amountDiff = Math.Abs(payload.Amount - expectedAmount);

				if (amountDiff > tolerance)
				{
					_logger.LogWarning("?? S? ti?n không kh?p: Expected={Expected} ({PaymentType}), Received={Received}, Diff={Diff}",
						expectedAmount, paymentTypeDescription, payload.Amount, amountDiff);
					await SaveUnmatchedTransaction(payload, contractNumber.Value);
					return Ok(new { message = "Amount mismatch", processed = false });
				}

				// 6. ? Match thành công ? T?o MatchedTransaction
				var matchedTransaction = new MatchedTransaction
				{
					TransactionId = payload.TransactionId,
					ContractId = contract.Id,
					Amount = payload.Amount,
					ReferenceNumber = payload.ReferenceNumber,
					Status = "Matched",
					TransactionDate = payload.TransactionDate,
					MatchedAt = DateTime.UtcNow,
					TransactionContent = payload.Content,
					BankBrandName = payload.BankBrandName,
					AccountNumber = payload.AccountNumber,
					Notes = $"Auto-matched by webhook at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {paymentTypeDescription}"
				};

				_context.MatchedTransactions.Add(matchedTransaction);

				// 7. C?p nh?t Contract Status
				var oldStatus = contract.Status;
				
				// Ch? c?p nh?t status sang "Paid" khi thanh toán 100% ho?c thanh toán n?t 50%
				if (paymentType == "full100" || paymentType == "final50")
				{
					contract.Status = "Paid";
					_logger.LogInformation("? Contract {ContractId} marked as Paid ({PaymentType})", contract.Id, paymentTypeDescription);
				}
				else if (paymentType == "deposit50")
				{
					// Có th? thêm status "PartiallyPaid" ho?c gi? nguyên status hi?n t?i
					_logger.LogInformation("? Contract {ContractId} received deposit 50%", contract.Id);
				}

				contract.UpdatedAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();

				_logger.LogInformation("? ?ã match payment thành công: Contract {ContractId}, Transaction {TransactionId}, Type: {PaymentType}",
					contract.Id, payload.TransactionId, paymentTypeDescription);

				// 8. ? T? ??ng tính KPI n?u contract chuy?n sang Paid
				if (oldStatus?.ToLower() != "paid" && contract.Status?.ToLower() == "paid")
				{
					var saleUserId = contract.SaleOrder?.CreatedByUserId;

					if (saleUserId.HasValue)
					{
						_logger.LogInformation("?? Triggering KPI calculation for User {UserId}...", saleUserId.Value);

						try
						{
							await _kpiCalculationService.CalculateKpiForUserAsync(
								saleUserId.Value,
								contract.CreatedAt.Month,
								contract.CreatedAt.Year);

							_logger.LogInformation("? KPI calculated successfully for User {UserId}", saleUserId.Value);
						}
						catch (Exception kpiEx)
						{
							_logger.LogError(kpiEx, "? Failed to calculate KPI for User {UserId}", saleUserId.Value);
						}
					}
				}

				return Ok(new
				{
					message = "Payment matched successfully",
					processed = true,
					contractId = contract.Id,
					contractNumber = contract.NumberContract,
					transactionId = payload.TransactionId,
					paymentType = paymentType,
					paymentTypeDescription = paymentTypeDescription,
					amount = payload.Amount,
					contractStatus = contract.Status
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "? L?i khi x? lý webhook t? Sepay");
				return StatusCode(500, new { message = "Internal server error", error = ex.Message });
			}
		}

		/// <summary>
		/// Parse s? h?p ??ng và lo?i thanh toán t? n?i dung chuy?n kho?n
		/// H? tr? các format:
		/// - DatCoc50%HopDong128 ? (128, "deposit50")
		/// - ThanhToan50%HopDong129 ? (129, "final50")
		/// - ThanhToanHopDong130 ? (130, "full100")
		/// </summary>
		private (int? contractNumber, string paymentType) ExtractContractNumberAndPaymentType(string content)
		{
			if (string.IsNullOrWhiteSpace(content))
				return (null, "full100");

			// Lo?i b? d?u ti?ng Vi?t và chuy?n v? lowercase
			var normalizedContent = RemoveVietnameseTones(content).ToLower();

			// Pattern cho ??t c?c 50%
			var depositPattern = @"datcoc50%?hopdong(\d+)";
			var depositMatch = Regex.Match(normalizedContent, depositPattern, RegexOptions.IgnoreCase);
			if (depositMatch.Success && int.TryParse(depositMatch.Groups[1].Value, out int depositContractNumber))
			{
				_logger.LogInformation("? Extracted contract number: {ContractNumber} with payment type: Deposit 50%", depositContractNumber);
				return (depositContractNumber, "deposit50");
			}

			// Pattern cho thanh toán n?t 50%
			var finalPattern = @"thanhtoan50%?hopdong(\d+)";
			var finalMatch = Regex.Match(normalizedContent, finalPattern, RegexOptions.IgnoreCase);
			if (finalMatch.Success && int.TryParse(finalMatch.Groups[1].Value, out int finalContractNumber))
			{
				_logger.LogInformation("? Extracted contract number: {ContractNumber} with payment type: Final 50%", finalContractNumber);
				return (finalContractNumber, "final50");
			}

			// Pattern cho thanh toán 100%
			var fullPattern = @"thanhtoanhopdong(\d+)";
			var fullMatch = Regex.Match(normalizedContent, fullPattern, RegexOptions.IgnoreCase);
			if (fullMatch.Success && int.TryParse(fullMatch.Groups[1].Value, out int fullContractNumber))
			{
				_logger.LogInformation("? Extracted contract number: {ContractNumber} with payment type: Full 100%", fullContractNumber);
				return (fullContractNumber, "full100");
			}

			// Fallback: Th? các pattern c? (backward compatibility)
			var contractNumber = ExtractContractNumber(content);
			if (contractNumber.HasValue)
			{
				_logger.LogInformation("? Extracted contract number: {ContractNumber} using fallback pattern (assumed Full 100%)", contractNumber.Value);
				return (contractNumber.Value, "full100");
			}

			return (null, "full100");
		}

		/// <summary>
		/// Parse s? h?p ??ng t? n?i dung chuy?n kho?n (fallback method)
		/// H? tr? nhi?u format: "hop dong 128", "HD128", "128"
		/// </summary>
		private int? ExtractContractNumber(string content)
		{
			if (string.IsNullOrWhiteSpace(content))
				return null;

			// Lo?i b? d?u ti?ng Vi?t và chuy?n v? lowercase
			var normalizedContent = RemoveVietnameseTones(content).ToLower();

			var patterns = new[]
			{
				@"hop\s*dong\s*(\d+)",        // hop dong 128
				@"hopdong\s*(\d+)",           // hopdong128
				@"hd\s*(\d+)",                // hd 128
				@"contract\s*(\d+)",          // contract 128
				@"\b(\d{3,})\b"               // b?t k? s? nào >= 3 ch? s?
			};

			foreach (var pattern in patterns)
			{
				var match = Regex.Match(normalizedContent, pattern, RegexOptions.IgnoreCase);
				if (match.Success && int.TryParse(match.Groups[1].Value, out int contractNumber))
				{
					return contractNumber;
				}
			}

			return null;
		}

		/// <summary>
		/// Lo?i b? d?u ti?ng Vi?t
		/// </summary>
		private string RemoveVietnameseTones(string text)
		{
			string[] vietnameseSigns = new string[]
			{
				"aAeEoOuUiIdDyY",
				"áà??ãâ???????????",
				"ÁÀ??ÃÂ???????????",
				"éè???ê?????",
				"ÉÈ???Ê?????",
				"óò??õô???????????",
				"ÓÒ??ÕÔ???????????",
				"úù?????????",
				"ÚÙ?????????",
				"íì???",
				"ÍÌ???",
				"?",
				"?",
				"ý????",
				"Ý????"
			};

			for (int i = 1; i < vietnameseSigns.Length; i++)
			{
				for (int j = 0; j < vietnameseSigns[i].Length; j++)
					text = text.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
			}

			return text;
		}

		/// <summary>
		/// L?u transaction không match ???c ?? admin review
		/// </summary>
		private async Task SaveUnmatchedTransaction(SepayWebhookPayload payload, int? suspectedContractNumber = null)
		{
			var unmatchedTransaction = new MatchedTransaction
			{
				TransactionId = payload.TransactionId,
				ContractId = null, // Ch?a match ???c
				Amount = payload.Amount,
				ReferenceNumber = payload.ReferenceNumber,
				Status = "Unmatched",
				TransactionDate = payload.TransactionDate,
				MatchedAt = DateTime.UtcNow,
				TransactionContent = payload.Content,
				BankBrandName = payload.BankBrandName,
				AccountNumber = payload.AccountNumber,
				Notes = suspectedContractNumber.HasValue
					? $"Suspected contract: {suspectedContractNumber.Value}, but not found or amount mismatch"
					: "Cannot extract contract number from transaction content"
			};

			_context.MatchedTransactions.Add(unmatchedTransaction);
			await _context.SaveChangesAsync();

			_logger.LogInformation("?? Saved unmatched transaction: {TransactionId}", payload.TransactionId);
		}
	}
}
