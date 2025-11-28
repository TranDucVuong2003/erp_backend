using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
	public class Lead
	{
		public int Id { get; set; }

		// Ng??i t?o lead (Marketing user)
		[Required]
		public int CreatedByUserId { get; set; }
		public User? CreatedByUser { get; set; }

		[Required]
		[StringLength(200)]
		public string FullName { get; set; } = string.Empty;

		[StringLength(150)]
		public string? Email { get; set; }

		[Required]
		[StringLength(20)]
		public string PhoneNumber { get; set; } = string.Empty;

		[StringLength(200)]
		public string? CompanyName { get; set; }

		[StringLength(500)]
		public string? Address { get; set; }

		// Ngu?n lead
		[Required]
		[StringLength(50)]
		public string Source { get; set; } = string.Empty; // "Facebook", "Google", "Website", "Referral", "Event"

		// Kênh qu?ng cáo c? th?
		[StringLength(100)]
		public string? Campaign { get; set; }

		// Ch?t l??ng lead (1-5 sao)
		[Range(1, 5)]
		public int QualityScore { get; set; } = 3;

		// Tr?ng thái
		[StringLength(20)]
		public string Status { get; set; } = "New"; // "New", "Contacted", "Qualified", "Converted", "Lost"

		// ?ã chuy?n ??i thành customer?
		public bool IsConverted { get; set; } = false;

		// Link ??n Customer n?u ?ã convert
		public int? CustomerId { get; set; }
		public Customer? Customer { get; set; }

		// Ng??i ph? trách (Sales)
		public int? AssignedToUserId { get; set; }
		public User? AssignedToUser { get; set; }

		// Ngày chuy?n ??i
		public DateTime? ConvertedAt { get; set; }

		// Doanh thu mang l?i (n?u convert)
		public decimal? RevenueGenerated { get; set; }

		// Chi phí ?? có lead này
		public decimal? AcquisitionCost { get; set; }

		// ROI c?a lead
		public decimal? ROI { get; set; }

		[StringLength(2000)]
		public string? Notes { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }
	}
}
