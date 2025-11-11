using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace erp_backend.Models
{
	public class Category_service_addons
	{
		public int Id { get; set; }

		[Required]
		[StringLength(200)]
		public string Name { get; set; } = string.Empty;

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public DateTime? UpdatedAt { get; set; }

		// Navigation properties
		[JsonIgnore] // Ignore khi serialize để tránh circular reference
		public ICollection<Service>? Services { get; set; }

		[JsonIgnore] // Ignore khi serialize để tránh circular reference
		public ICollection<Addon>? Addons { get; set; }
	}
}
