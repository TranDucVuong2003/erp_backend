using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
	public class TicketLogAttachment
	{
		public int Id { get; set; }

		[Required]
		public int TicketLogId { get; set; }
		public virtual TicketLog? TicketLog { get; set; }

		[Required]
		[StringLength(255)]
		public string FileName { get; set; } = string.Empty;

		[Required]
		[StringLength(500)]
		public string FilePath { get; set; } = string.Empty;

		[StringLength(100)]
		public string? FileType { get; set; } // image/png, application/pdf, etc.

		public long FileSize { get; set; } // bytes

		[StringLength(50)]
		public string? Category { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }
	}
}
