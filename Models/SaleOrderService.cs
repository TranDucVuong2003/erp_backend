using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace erp_backend.Models
{
    public class SaleOrderService
    {
        public int Id { get; set; }

        [Required]
        public int SaleOrderId { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "S? l??ng ph?i l?n h?n 0")]
        public int? Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Giá ph?i l?n h?n ho?c b?ng 0")]
        public decimal UnitPrice { get; set; }

        public decimal TotalPrice => (Quantity ?? 1) * UnitPrice;

        [StringLength(500)]
        public string? Notes { get; set; }
		public int duration { get; set; }
		[StringLength(200)]
		public string template { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties (NO JsonIgnore - we handle it in DTOs)
        public SaleOrder? SaleOrder { get; set; }
        public Service? Service { get; set; }
    }
}
