using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
    public class MonthlyAttendance
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

        [Required(ErrorMessage = "S? ngày công th?c t? là b?t bu?c")]
        [Range(0, 31, ErrorMessage = "S? ngày công ph?i t? 0-31")]
        public float ActualWorkDays { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public User? User { get; set; }
    }
}
