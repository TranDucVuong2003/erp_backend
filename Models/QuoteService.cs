using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
	public class QuoteService
	{
		public int Id { get; set; }

		[Required]
		public int QuoteId { get; set; }

		[Required]
		public int ServiceId { get; set; }

		public int Quantity { get; set; } = 1;

		public decimal UnitPrice { get; set; }

		public string? Notes { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public DateTime? UpdatedAt { get; set; }

		// Navigation properties
		public Quote? Quote { get; set; }

		public Service? Service { get; set; }
	}
}
