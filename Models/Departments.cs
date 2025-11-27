using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
	public class Departments
	{
		public int Id { get; set; }
		
		[Required]
		[StringLength(200)]
		public string Name { get; set; } = string.Empty;
		
		public int ResionId { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public DateTime? UpdatedAt { get; set; }

		// Navigation property
		public Resion? Resion { get; set; }
	}
}
