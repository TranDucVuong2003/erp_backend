using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using erp_backend.Data;
using erp_backend.Models;
using erp_backend.Models.DTOs;
using erp_backend.Services;
using erp_backend.Hubs;
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
		private readonly IHubContext<PaymentHub> _hubContext;

		public WebhooksController(
			ApplicationDbContext context,
			ILogger<WebhooksController> logger,
			IKpiCalculationService kpiCalculationService,
			IHubContext<PaymentHub> hubContext)
		{
			_context = context;
			_logger = logger;
			_kpiCalculationService = kpiCalculationService;
			_hubContext = hubContext;
		}

		/// <summary>
		/// Webhook endpoint nhận thông báo từ Sepay khi có giao dịch mới
		/// POST /api/webhooks/sepay-payment
		/// </summary>
		[HttpPost("sepay-payment")]
		public async Task<IActionResult> SepayPaymentWebhook([FromBody] SepayWebhookPayload payload)
		{
			try
			{
				_logger.LogInformation("✅ Nhận webhook từ Sepay: ID={Id}, Gateway={Gateway}, Amount={Amount}, Content={Content}",
					payload.Id, payload.Gateway, payload.TransferAmount, payload.Content);

				// 1. Chỉ xử lý giao dịch tiền VÀO
				if (payload.TransferType?.ToLower() != "in")
				{
					_logger.LogWarning("⚠️ Bỏ qua giao dịch tiền RA: ID={Id}", payload.Id);
					return Ok(new { success = true, message = "Transfer type is not 'in', ignored" });
				}

				// 2. Kiểm tra xem transaction đã được xử lý chưa
				var existingTransaction = await _context.MatchedTransactions
					.FirstOrDefaultAsync(mt => mt.TransactionId == payload.TransactionId);

				if (existingTransaction != null)
				{
					_logger.LogWarning("⚠️ Transaction {TransactionId} đã được xử lý trước đó", payload.TransactionId);
					return Ok(new { success = true, processed = false, message = "Transaction already processed" });
				}

				// 3. Parse số hợp đồng và loại thanh toán từ nội dung chuyển khoản
				var (contractNumber, paymentType) = ExtractContractNumberAndPaymentType(payload.Content);

				if (!contractNumber.HasValue)
				{
					_logger.LogWarning("⚠️ Không tìm thấy số hợp đồng trong nội dung: {Content}", payload.Content);
					await SaveUnmatchedTransaction(payload);
					return Ok(new { success = true, processed = false, message = "Cannot extract contract number" });
				}

				// 4. Tìm Contract theo NumberContract
				var contract = await _context.Contracts
					.Include(c => c.SaleOrder)
					.FirstOrDefaultAsync(c => c.NumberContract == contractNumber.Value);

				if (contract == null)
				{
					_logger.LogWarning("⚠️ Không tìm thấy Contract với NumberContract={ContractNumber}", contractNumber.Value);
					await SaveUnmatchedTransaction(payload, contractNumber.Value);
					return Ok(new { success = true, processed = false, message = "Contract not found" });
				}

				// 5. Xác định số tiền kỳ vọng dựa trên loại thanh toán
				decimal expectedAmount;
				string paymentTypeDescription;

				switch (paymentType)
				{
					case "deposit50":
						expectedAmount = contract.TotalAmount * 0.5m;
						paymentTypeDescription = "Đặt cọc 50%";
						break;
					case "final50":
						expectedAmount = contract.TotalAmount * 0.5m;
						paymentTypeDescription = "Thanh toán nốt 50%";
						break;
					case "full100":
					default:
						expectedAmount = contract.TotalAmount;
						paymentTypeDescription = "Thanh toán 100%";
						break;
				}

				// 6. Kiểm tra số tiền có khớp không (cho phép sai lệch 1%)
				var tolerance = expectedAmount * 0.01m; // 1% sai lệch
				var amountDiff = Math.Abs(payload.Amount - expectedAmount);

				if (amountDiff > tolerance)
				{
					_logger.LogWarning("⚠️ Số tiền không khớp: Expected={Expected} ({PaymentType}), Received={Received}, Diff={Diff}",
						expectedAmount, paymentTypeDescription, payload.Amount, amountDiff);
					await SaveUnmatchedTransaction(payload, contractNumber.Value);
					return Ok(new { success = true, processed = false, message = "Amount mismatch" });
				}

				// 7. ✅ Match thành công -> Tạo MatchedTransaction
				var matchedTransaction = new MatchedTransaction
				{
					TransactionId = payload.TransactionId,
					ContractId = contract.Id,
					Amount = payload.Amount,
					ReferenceNumber = payload.ReferenceNumber,
					Status = "Matched",
					TransactionDate = payload.TransactionDateTime,
					MatchedAt = DateTime.UtcNow,
					TransactionContent = payload.Content,
					BankBrandName = payload.BankBrandName,
					AccountNumber = payload.AccountNumber,
					Notes = $"Auto-matched by webhook at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {paymentTypeDescription}"
				};

				_context.MatchedTransactions.Add(matchedTransaction);

				// 8. Cập nhật Contract Status
				var oldStatus = contract.Status;

				// Cập nhật status dựa trên loại thanh toán
				switch (paymentType)
				{
					case "deposit50":
						contract.Status = "Deposit 50%";
						_logger.LogInformation("✅ Contract {ContractId} status changed to 'Deposit 50%'", contract.Id);
						break;
					
					case "final50":
						contract.Status = "Paid";
						_logger.LogInformation("✅ Contract {ContractId} status changed to 'Paid' (Final 50%)", contract.Id);
						break;
					
					case "full100":
						contract.Status = "Paid";
						_logger.LogInformation("✅ Contract {ContractId} status changed to 'Paid' (Full 100%)", contract.Id);
						break;
				}

				contract.UpdatedAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();

				_logger.LogInformation("🎉 Đã match payment thành công: Contract {ContractId}, Transaction {TransactionId}, Type: {PaymentType}",
					contract.Id, payload.TransactionId, paymentTypeDescription);

				// 🔔 GỬI THÔNG BÁO REAL-TIME ĐẾN CLIENT
				var groupName = $"Contract_{contract.Id}";
				await _hubContext.Clients.Group(groupName).SendAsync("PaymentSuccess", new
				{
					contractId = contract.Id,
					contractNumber = contract.NumberContract,
					amount = payload.Amount,
					paymentType = paymentType,
					paymentTypeDescription = paymentTypeDescription,
					status = contract.Status,
					transactionDate = payload.TransactionDateTime,
					transactionId = payload.TransactionId,
					message = $"✅ Thanh toán {paymentTypeDescription} thành công!"
				});

				_logger.LogInformation("📢 Sent SignalR notification to group {GroupName}", groupName);

				// 9. 🎯 Tự động tính KPI nếu contract chuyển sang Paid
				if (oldStatus?.ToLower() != "paid" && contract.Status?.ToLower() == "paid")
				{
					var saleUserId = contract.SaleOrder?.CreatedByUserId;

					if (saleUserId.HasValue)
					{
						_logger.LogInformation("⚙️ Triggering KPI calculation for User {UserId}...", saleUserId.Value);

						try
						{
							await _kpiCalculationService.CalculateKpiForUserAsync(
								saleUserId.Value,
								contract.CreatedAt.Month,
								contract.CreatedAt.Year);

							_logger.LogInformation("✅ KPI calculated successfully for User {UserId}", saleUserId.Value);
						}
						catch (Exception kpiEx)
						{
							_logger.LogError(kpiEx, "❌ Lỗi tính toán KPI cho User {UserId}", saleUserId.Value);
						}
					}
				}

				// ✅ Response theo format Sepay yêu cầu: {"success": true, ...}
				return Ok(new
				{
					success = true,
					processed = true,
					message = "Payment matched successfully",
					data = new
					{
						contractId = contract.Id,
						contractNumber = contract.NumberContract,
						transactionId = payload.TransactionId,
						paymentType = paymentType,
						paymentTypeDescription = paymentTypeDescription,
						amount = payload.Amount,
						contractStatus = contract.Status
					}
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "❌ Lỗi khi xử lý webhook từ Sepay. InnerException: {InnerException}", 
					ex.InnerException?.Message ?? "N/A");
				return StatusCode(500, new { 
					success = false, 
					message = "Internal server error", 
					error = ex.Message,
					innerError = ex.InnerException?.Message 
				});
			}
		}

		/// <summary>
		/// Parse số hợp đồng và loại thanh toán từ nội dung chuyển khoản
		/// Hỗ trợ các format:
		/// - ttw deposit 128 -> (128, "deposit50")
		/// - ttw final 129 -> (129, "final50")
		/// - ttw paid 130 -> (130, "full100")
		/// - DatCoc50%HopDong128 -> (128, "deposit50") [backward compatibility]
		/// - ThanhToan50%HopDong129 -> (129, "final50") [backward compatibility]
		/// - ThanhToanHopDong130 -> (130, "full100") [backward compatibility]
		/// </summary>
		private (int? contractNumber, string paymentType) ExtractContractNumberAndPaymentType(string content)
		{
			if (string.IsNullOrWhiteSpace(content))
				return (null, "full100");

			// Loại bỏ dấu tiếng Việt và chuyển về lowercase
			var normalizedContent = RemoveVietnameseTones(content).ToLower();

			// ✅ NEW PATTERNS (from appsettings.json templates)
			
			// Pattern cho "ttw deposit 128" -> deposit50
			var ttwDepositPattern = @"ttw\s+deposit\s+(\d+)";
			var ttwDepositMatch = Regex.Match(normalizedContent, ttwDepositPattern, RegexOptions.IgnoreCase);
			if (ttwDepositMatch.Success && int.TryParse(ttwDepositMatch.Groups[1].Value, out int depositNum))
			{
				_logger.LogInformation("🔍 Extracted contract number: {ContractNumber} with payment type: Deposit 50%", depositNum);
				return (depositNum, "deposit50");
			}

			// Pattern cho "ttw final 129" -> final50
			var ttwFinalPattern = @"ttw\s+final\s+(\d+)";
			var ttwFinalMatch = Regex.Match(normalizedContent, ttwFinalPattern, RegexOptions.IgnoreCase);
			if (ttwFinalMatch.Success && int.TryParse(ttwFinalMatch.Groups[1].Value, out int finalNum))
			{
				_logger.LogInformation("🔍 Extracted contract number: {ContractNumber} with payment type: Final 50%", finalNum);
				return (finalNum, "final50");
			}

			// Pattern cho "ttw paid 130" -> full100
			var ttwPaidPattern = @"ttw\s+paid\s+(\d+)";
			var ttwPaidMatch = Regex.Match(normalizedContent, ttwPaidPattern, RegexOptions.IgnoreCase);
			if (ttwPaidMatch.Success && int.TryParse(ttwPaidMatch.Groups[1].Value, out int paidNum))
			{
				_logger.LogInformation("🔍 Extracted contract number: {ContractNumber} with payment type: Full 100%", paidNum);
				return (paidNum, "full100");
			}

			// ✅ BACKWARD COMPATIBILITY - OLD PATTERNS
			
			// Pattern cho đặt cọc 50% (old format)
			var depositPattern = @"datcoc50%?hopdong(\d+)";
			var depositMatch = Regex.Match(normalizedContent, depositPattern, RegexOptions.IgnoreCase);
			if (depositMatch.Success && int.TryParse(depositMatch.Groups[1].Value, out int depositContractNumber))
			{
				_logger.LogInformation("🔍 Extracted contract number: {ContractNumber} with payment type: Deposit 50% (old format)", depositContractNumber);
				return (depositContractNumber, "deposit50");
			}

			// Pattern cho thanh toán nốt 50% (old format)
			var finalPattern = @"thanhtoan50%?hopdong(\d+)";
			var finalMatch = Regex.Match(normalizedContent, finalPattern, RegexOptions.IgnoreCase);
			if (finalMatch.Success && int.TryParse(finalMatch.Groups[1].Value, out int finalContractNumber))
			{
				_logger.LogInformation("🔍 Extracted contract number: {ContractNumber} with payment type: Final 50% (old format)", finalContractNumber);
				return (finalContractNumber, "final50");
			}

			// Pattern cho thanh toán 100% (old format)
			var fullPattern = @"thanhtoanhopdong(\d+)";
			var fullMatch = Regex.Match(normalizedContent, fullPattern, RegexOptions.IgnoreCase);
			if (fullMatch.Success && int.TryParse(fullMatch.Groups[1].Value, out int fullContractNumber))
			{
				_logger.LogInformation("🔍 Extracted contract number: {ContractNumber} with payment type: Full 100% (old format)", fullContractNumber);
				return (fullContractNumber, "full100");
			}

			// Fallback: Thử các pattern cũ (backward compatibility)
			var contractNumber = ExtractContractNumber(content);
			if (contractNumber.HasValue)
			{
				_logger.LogInformation("🔍 Extracted contract number: {ContractNumber} using fallback pattern (assumed Full 100%)", contractNumber.Value);
				return (contractNumber.Value, "full100");
			}

			return (null, "full100");
		}

		/// <summary>
		/// Parse số hợp đồng từ nội dung chuyển khoản (fallback method)
		/// Hỗ trợ nhiều format: "hop dong 128", "HD128", "128"
		/// </summary>
		private int? ExtractContractNumber(string content)
		{
			if (string.IsNullOrWhiteSpace(content))
				return null;

			// Loại bỏ dấu tiếng Việt và chuyển về lowercase
			var normalizedContent = RemoveVietnameseTones(content).ToLower();

			var patterns = new[]
			{
				@"hop\s*dong\s*(\d+)",        // hop dong 128
				@"hopdong\s*(\d+)",           // hopdong128
				@"hd\s*(\d+)",                // hd 128
				@"contract\s*(\d+)",          // contract 128
				@"\b(\d{3,})\b"               // bất kỳ số nào >= 3 chữ số
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
		/// Loại bỏ dấu tiếng Việt
		/// </summary>
		private string RemoveVietnameseTones(string text)
		{
			string[] vietnameseSigns = new string[]
			{
				"aAeEoOuUiIdDyY",
				"áàảãạâấồẩẫậăắằẳẵặ",
				"ÁÀẢÃẠÂẤỒẨẪẬĂẮẰẲẴẶ",
				"éèẻẽẹêếềểễệ",
				"ÉÈẺẼẸÊẾỀỂỄỆ",
				"óòỏõọôốồổỗộơớờởỡợ",
				"ÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỬỢ",
				"úùủũụưứừửữự",
				"ÚÙỦŨỤƯỨỪỬỮỰ",
				"íìỉĩị",
				"ÍÌỈĨỊ",
				"đ",
				"Đ",
				"ýỳỷỹỵ",
				"ÝỲỶỸỴ"
			};

			// Xóa các ký tự có dấu
			for (int i = 1; i < vietnameseSigns.Length; i++)
			{
				for (int j = 0; j < vietnameseSigns[i].Length; j++)
					text = text.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
			}

			return text;
		}

		/// <summary>
		/// Lưu transaction không match được để admin review
		/// </summary>
		private async Task SaveUnmatchedTransaction(SepayWebhookPayload payload, int? suspectedContractNumber = null)
		{
			try
			{
				_logger.LogInformation("📝 Attempting to save unmatched transaction: ID={Id}, Content={Content}", 
					payload.Id, payload.Content);

				// Truncate các field nếu quá dài
				var transactionContent = payload.Content?.Length > 500 
					? payload.Content.Substring(0, 497) + "..." 
					: payload.Content;

				var referenceNumber = payload.ReferenceNumber?.Length > 100
					? payload.ReferenceNumber.Substring(0, 97) + "..."
					: payload.ReferenceNumber;

				var bankBrandName = payload.BankBrandName?.Length > 50
					? payload.BankBrandName.Substring(0, 47) + "..."
					: payload.BankBrandName;

				var accountNumber = payload.AccountNumber?.Length > 50
					? payload.AccountNumber.Substring(0, 47) + "..."
					: payload.AccountNumber;

				var notes = suspectedContractNumber.HasValue
					? $"Suspected contract: {suspectedContractNumber.Value}, but not found or amount mismatch"
					: "Cannot extract contract number from transaction content";

				if (notes.Length > 1000)
				{
					notes = notes.Substring(0, 997) + "...";
				}

				var unmatchedTransaction = new MatchedTransaction
				{
					TransactionId = payload.TransactionId, // Đã là string từ computed property
					ContractId = null, // Chưa match được
					Amount = payload.Amount,
					ReferenceNumber = referenceNumber,
					Status = "Unmatched", // ✅ Phải có giá trị
					TransactionDate = payload.TransactionDateTime,
					MatchedAt = DateTime.UtcNow,
					TransactionContent = transactionContent,
					BankBrandName = bankBrandName,
					AccountNumber = accountNumber,
					Notes = notes
				};

				_context.MatchedTransactions.Add(unmatchedTransaction);
				await _context.SaveChangesAsync();

				_logger.LogInformation("✅ Saved unmatched transaction: {TransactionId}", payload.TransactionId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "❌ Lỗi khi lưu unmatched transaction: {TransactionId}. InnerException: {InnerException}", 
					payload.TransactionId, ex.InnerException?.Message ?? "N/A");
				
				// Không throw exception để không làm webhook fail
				// Chỉ log lỗi và tiếp tục
			}
		}
	}
}