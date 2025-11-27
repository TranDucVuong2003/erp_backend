using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
	public class Positions
	{
		public int Id { get; set; }
		
		[Required]
		[StringLength(100)]
		public string PositionName { get; set; } = string.Empty;
		
		public int Level { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		
		public DateTime? UpdatedAt { get; set; }
	}
}
