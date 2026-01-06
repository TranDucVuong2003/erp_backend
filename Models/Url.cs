using System;
using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
	public class Url
	{
		public int Id { get; set; }

		[Required]
		[StringLength(500)]
		public string Links { get; set; } = string.Empty;   // URL

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}
