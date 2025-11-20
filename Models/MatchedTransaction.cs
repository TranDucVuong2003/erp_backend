using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace erp_backend.Models
{
	public class MatchedTransaction
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[StringLength(50)]
		public string TransactionId { get; set; } = string.Empty;

		public int? ContractId { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal Amount { get; set; }

		[StringLength(100)]
		public string? ReferenceNumber { get; set; }

		[StringLength(50)]
		public string Status { get; set; } 

		public DateTime TransactionDate { get; set; }

		public DateTime MatchedAt { get; set; } = DateTime.UtcNow;

		[StringLength(500)]
		public string? TransactionContent { get; set; }

		[StringLength(50)]
		public string? BankBrandName { get; set; }

		[StringLength(50)]
		public string? AccountNumber { get; set; }

		public int? MatchedByUserId { get; set; }

		[StringLength(1000)]
		public string? Notes { get; set; }

		[ForeignKey("ContractId")]
		public Contract? Contract { get; set; }

		[ForeignKey("MatchedByUserId")]
		public User? MatchedByUser { get; set; }
	}
}
