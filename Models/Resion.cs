using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
	public class Resion
	{
		public int Id { get; set; }
		
		[Required]
		[StringLength(100)]
		public string City { get; set; } = string.Empty;

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public DateTime? UpdatedAt { get; set; }
	}
}
