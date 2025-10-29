using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{

    public class SaleOrder
    {
        public int Id { get; set; }

		[Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Customer ID là bắt buộc")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Giá trị là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị phải lớn hơn 0")]
        public decimal Value { get; set; }

        [Range(0, 100, ErrorMessage = "Xác suất phải từ 0-100%")]
        public int Probability { get; set; } = 0;

        [StringLength(2000)]
        public string? Notes { get; set; }

        // Giữ lại để backward compatibility (optional)
        public int? ServiceId { get; set; }
        public int? AddonId { get; set; }
        
		public int? TaxId { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

		// Navigation properties (old - single relationship)
		public Customer? Customer { get; set; }
		public Service? Service { get; set; }
		public Addon? Addon { get; set; }
		public Tax? Tax { get; set; }

        // Navigation properties (new - many-to-many relationship)
        public ICollection<SaleOrderService> SaleOrderServices { get; set; } = new List<SaleOrderService>();
        public ICollection<SaleOrderAddon> SaleOrderAddons { get; set; } = new List<SaleOrderAddon>();
	}
}