using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{

    public class SaleOrder
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị phải lơn hơn 0")]
        public decimal Value { get; set; }

        [Range(0, 100, ErrorMessage = "Xác suất phải từ 0-100%")]
        public int Probability { get; set; } = 0;

        [StringLength(2000)]
        public string? Notes { get; set; }

        public int? ServiceId { get; set; }

        public int? AddonId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

    }
}