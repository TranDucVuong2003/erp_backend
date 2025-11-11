using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace erp_backend.Models
{
	public class Quote
	{
		public int Id { get; set; }

		public int? CustomerId { get; set; }

		// ⚠️ GIỮ LẠI cho backward compatibility (optional)
		// Hoặc có thể XÓA nếu không cần
		public int? ServiceId { get; set; }
		public int? AddonId { get; set; }

		[Column("CustomService")]
		[StringLength(4000)]
		public string? CustomServiceJson { get; set; }

		[NotMapped]
		public List<CustomServiceItem>? CustomService
		{
			get => string.IsNullOrEmpty(CustomServiceJson) 
				? null 
				: JsonSerializer.Deserialize<List<CustomServiceItem>>(CustomServiceJson);
			set => CustomServiceJson = value == null 
				? null 
				: JsonSerializer.Serialize(value);
		}

		[StringLength(1000)]
		public string? FilePath { get; set; }

		[Range(0, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn hoặc bằng 0")]
		public decimal Amount { get; set; }

		// ✅ THÊM: User ID của người tạo quote
		public int? CreatedByUserId { get; set; }

		// ✅ THÊM: Category Service Addon ID
		public int? CategoryServiceAddonId { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public DateTime? UpdatedAt { get; set; }

		// Navigation properties
		public Customer? Customer { get; set; }

		// ⚠️ GIỮ LẠI cho backward compatibility
		public Service? Service { get; set; }
		public Addon? Addon { get; set; }

		// ✅ THÊM Collection navigation properties
		public ICollection<QuoteService> QuoteServices { get; set; } = new List<QuoteService>();
		public ICollection<QuoteAddon> QuoteAddons { get; set; } = new List<QuoteAddon>();

		// ✅ THÊM: Navigation property cho User tạo quote
		public User? CreatedByUser { get; set; }

		// ✅ THÊM: Navigation property cho Category Service Addon
		public Category_service_addons? CategoryServiceAddon { get; set; }
	}
}
