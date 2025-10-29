using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
    public class SaleOrderAddon
    {
        public int Id { get; set; }

        [Required]
        public int SaleOrderId { get; set; }

        [Required]
        public int AddonId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "S? l??ng ph?i l?n h?n 0")]
        public int? Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Giá ph?i l?n h?n ho?c b?ng 0")]
        public decimal UnitPrice { get; set; }

        public decimal TotalPrice => (Quantity ?? 1) * UnitPrice;

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public SaleOrder SaleOrder { get; set; } = null!;
        public Addon Addon { get; set; } = null!;
    }
}
