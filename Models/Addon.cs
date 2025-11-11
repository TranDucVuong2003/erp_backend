using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
    public class Addon
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Giá ph?i l?n h?n ho?c b?ng 0")]
        public decimal Price { get; set; }

		[Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
		public int? Quantity { get; set; }

		[StringLength(50)]
        public string? Type { get; set; }

        public bool IsActive { get; set; } = true;

        public int? TaxId { get; set; }

		public int? CategoryId { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Tax? Tax { get; set; }

		public Category_service_addons? CategoryServiceAddons { get; set; }
    }
}