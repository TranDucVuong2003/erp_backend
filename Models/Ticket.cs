using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
	public class Ticket
	{
		public int Id { get; set; }

		[Required]
		[StringLength(500)]
		public string Title { get; set; } = string.Empty;

		[Required]
		public string Description { get; set; } = string.Empty; // Rich text content

	// Customer Information
	[Required]
	public int CustomerId { get; set; }
	public virtual Customer? Customer { get; set; }

	// Ticket Classification
	[Required]
	public string Priority { get; set; } = string.Empty;
	public string Status { get; set; }

		[Required]
	public int CategoryId { get; set; }
	public virtual TicketCategory? Category { get; set; }

		// Urgency level (1-5 stars như trong UI React)
		[Range(1, 5)]
		public int UrgencyLevel { get; set; } = 1;

		// Assignment
		public int? UserId { get; set; }
		public virtual User? AssignedTo { get; set; }

		public int? CreatedById { get; set; }
		public virtual User? CreatedBy { get; set; }

		// Timing
		public DateTime? Deadline { get; set; }
		public DateTime? ClosedAt { get; set; }


		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }
	}
}