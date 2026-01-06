using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
    public class SalaryComponent
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "UserId là b?t bu?c")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Tháng là b?t bu?c")]
        [Range(1, 12, ErrorMessage = "Tháng ph?i t? 1-12")]
        public int Month { get; set; }

        [Required(ErrorMessage = "N?m là b?t bu?c")]
        [Range(2020, 2100, ErrorMessage = "N?m ph?i t? 2020-2100")]
        public int Year { get; set; }

        [Required(ErrorMessage = "S? ti?n là b?t bu?c")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Lo?i là b?t bu?c")]
        [StringLength(10)]
        public string Type { get; set; } = string.Empty; // "in" (c?ng) ho?c "out" (tr?)

        [Required(ErrorMessage = "Lý do là b?t bu?c")]
        [StringLength(500, ErrorMessage = "Lý do không ???c v??t quá 500 ký t?")]
        public string Reason { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public User? User { get; set; }
    }
}
