using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
	public class QuoteAddon
	{
		public int Id { get; set; }

		[Required]
		public int QuoteId { get; set; }

		[Required]
		public int AddonId { get; set; }

		// ✅ THÊM: Quantity, UnitPrice, Notes
		public int Quantity { get; set; } = 1;

		public decimal UnitPrice { get; set; }

		public string? Notes { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public DateTime? UpdatedAt { get; set; }

		// Navigation properties
		public Quote? Quote { get; set; }

		public Addon? Addon { get; set; }
	}
}
