using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models.DTOs
{
	/// <summary>
	/// DTO nh?n webhook payload t? Sepay
	/// </summary>
	public class SepayWebhookPayload
	{
		[Required]
		public string TransactionId { get; set; } = string.Empty;

		[Required]
		[Range(0, double.MaxValue)]
		public decimal Amount { get; set; }

		[Required]
		public string Content { get; set; } = string.Empty;

		public string? ReferenceNumber { get; set; }

		[Required]
		public DateTime TransactionDate { get; set; }

		public string? BankBrandName { get; set; }

		public string? AccountNumber { get; set; }

		/// <summary>
		/// Lo?i giao d?ch: "in" (ti?n vào) ho?c "out" (ti?n ra)
		/// </summary>
		public string? TransferType { get; set; }

		/// <summary>
		/// Mã giao d?ch tham chi?u (n?u có)
		/// </summary>
		public string? RefCode { get; set; }

		/// <summary>
		/// S? d? sau giao d?ch
		/// </summary>
		public decimal? Balance { get; set; }
	}
}
