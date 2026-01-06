using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace erp_backend.Models.DTOs
{
	/// <summary>
	/// DTO nh?n webhook payload t? Sepay
	/// Format theo docs: https://docs.sepay.vn
	/// </summary>
	public class SepayWebhookPayload
	{
		/// <summary>
		/// ID giao d?ch trên SePay
		/// </summary>
		[Required]
		[JsonPropertyName("id")]
		public int Id { get; set; }

		/// <summary>
		/// Brand name c?a ngân hàng (VD: "Vietcombank", "MBBank")
		/// </summary>
		[JsonPropertyName("gateway")]
		public string? Gateway { get; set; }

		/// <summary>
		/// Th?i gian x?y ra giao d?ch phía ngân hàng
		/// Format: "2023-03-25 14:02:37"
		/// </summary>
		[Required]
		[JsonPropertyName("transactionDate")]
		public string TransactionDate { get; set; } = string.Empty;

		/// <summary>
		/// S? tài kho?n ngân hàng
		/// </summary>
		[JsonPropertyName("accountNumber")]
		public string? AccountNumber { get; set; }

		/// <summary>
		/// Mã code thanh toán (sepay t? nh?n di?n d?a vào c?u hình)
		/// </summary>
		[JsonPropertyName("code")]
		public string? Code { get; set; }

		/// <summary>
		/// N?i dung chuy?n kho?n
		/// </summary>
		[Required]
		[JsonPropertyName("content")]
		public string Content { get; set; } = string.Empty;

		/// <summary>
		/// Lo?i giao d?ch: "in" (ti?n vào) ho?c "out" (ti?n ra)
		/// </summary>
		[JsonPropertyName("transferType")]
		public string? TransferType { get; set; }

		/// <summary>
		/// S? ti?n giao d?ch
		/// </summary>
		[Required]
		[Range(0, double.MaxValue)]
		[JsonPropertyName("transferAmount")]
		public decimal TransferAmount { get; set; }

		/// <summary>
		/// S? d? tài kho?n (l?y k?)
		/// </summary>
		[JsonPropertyName("accumulated")]
		public decimal? Accumulated { get; set; }

		/// <summary>
		/// Tài kho?n ngân hàng ph? (tài kho?n ??nh danh)
		/// </summary>
		[JsonPropertyName("subAccount")]
		public string? SubAccount { get; set; }

		/// <summary>
		/// Mã tham chi?u c?a tin nh?n SMS (VD: "MBVCB.3278907687")
		/// </summary>
		[JsonPropertyName("referenceCode")]
		public string? ReferenceCode { get; set; }

		/// <summary>
		/// Toàn b? n?i dung tin nh?n SMS
		/// </summary>
		[JsonPropertyName("description")]
		public string? Description { get; set; }

		// ===== Computed properties ?? t??ng thích v?i code hi?n t?i =====

		/// <summary>
		/// TransactionId ?? l?u vào database (convert t? Id)
		/// </summary>
		[JsonIgnore]
		public string TransactionId => Id.ToString();

		/// <summary>
		/// Amount ?? t??ng thích v?i code c?
		/// </summary>
		[JsonIgnore]
		public decimal Amount => TransferAmount;

		/// <summary>
		/// BankBrandName ?? t??ng thích v?i code c?
		/// </summary>
		[JsonIgnore]
		public string? BankBrandName => Gateway;

		/// <summary>
		/// ReferenceNumber ?? t??ng thích v?i code c?
		/// </summary>
		[JsonIgnore]
		public string? ReferenceNumber => ReferenceCode;

		/// <summary>
		/// Parse TransactionDate string sang DateTime v?i Kind = UTC
		/// </summary>
		[JsonIgnore]
		public DateTime TransactionDateTime
		{
			get
			{
				// Parse "2023-03-25 14:02:37" sang DateTime
				if (DateTime.TryParseExact(TransactionDate,
					"yyyy-MM-dd HH:mm:ss",
					System.Globalization.CultureInfo.InvariantCulture,
					System.Globalization.DateTimeStyles.None,
					out DateTime result))
				{
					// ? Chuy?n sang UTC ?? t??ng thích v?i PostgreSQL
					return DateTime.SpecifyKind(result, DateTimeKind.Utc);
				}
				return DateTime.UtcNow; // Fallback to UTC
			}
		}
	}
}
