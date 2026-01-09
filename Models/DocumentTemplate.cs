using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace erp_backend.Models
{
	/// <summary>
	/// B?ng l?u tr? các HTML template cho h?p ??ng, báo giá, email, l??ng,...
	/// </summary>
	[Table("document_templates")]
	public class DocumentTemplate
	{
		public int Id { get; set; }

		[Required]
		[StringLength(100)]
		public string Name { get; set; } = string.Empty; // "Contract Template", "Quote Template"

		[StringLength(50)]
		public string? TemplateType { get; set; } // "contract", "quote", "email", "salary_report"

		[Required]
		[StringLength(50)]
		public string Code { get; set; } = string.Empty; // "CONTRACT_DEFAULT", "QUOTE_STANDARD" (unique)

		[Required]
		public string HtmlContent { get; set; } = string.Empty; // N?i dung HTML template

		[StringLength(500)]
		public string? Description { get; set; }

		// Versioning
		public int Version { get; set; } = 1;

		public bool IsActive { get; set; } = true;

		public bool IsDefault { get; set; } = false; // Template m?c ??nh cho lo?i này

		// Audit fields
		public int? CreatedByUserId { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public DateTime? UpdatedAt { get; set; }

		// Navigation properties
		public User? CreatedByUser { get; set; }
	}
}
