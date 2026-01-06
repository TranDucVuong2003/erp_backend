using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace erp_backend.Models
{
	public class Payslip
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "UserId là bắt buộc")]
		public int UserId { get; set; }

		[Required(ErrorMessage = "Tháng là bắt buộc")]
		[Range(1, 12, ErrorMessage = "Tháng phải từ 1–12")]
		public int Month { get; set; }

		[Required(ErrorMessage = "Năm là bắt buộc")]
		[Range(2020, 2100, ErrorMessage = "Năm phải từ 2020–2100")]
		public int Year { get; set; }

		[Required(ErrorMessage = "Số công chuẩn là bắt buộc")]
		[Range(1, 31, ErrorMessage = "Số công chuẩn phải từ 1–31")]
		public int StandardWorkDays { get; set; } = 26; // Số công chuẩn (C)

		[Required(ErrorMessage = "Lương gộp là bắt buộc")]
		[Column(TypeName = "decimal(18, 2)")]
		public decimal GrossSalary { get; set; } // Lương theo ngày công + Thưởng - Phạt

		// Bảo hiểm (BHXH + BHYT + BHTN)
		[Column(TypeName = "decimal(18, 2)")]
		public decimal InsuranceDeduction { get; set; } = 0; // Tổng bảo hiểm nhân viên phải đóng

		// Giảm trừ gia cảnh (Bản thân + Người phụ thuộc)
		[Column(TypeName = "decimal(18, 2)")]
		public decimal FamilyDeduction { get; set; } = 0; // Tổng giảm trừ gia cảnh

		// Thu nhập tính thuế (Sau khi trừ bảo hiểm và giảm trừ gia cảnh)
		[Column(TypeName = "decimal(18, 2)")]
		public decimal AssessableIncome { get; set; } = 0; // Thu nhập tính thuế

		[Required(ErrorMessage = "Tiền thuế là bắt buộc")]
		[Column(TypeName = "decimal(18, 2)")]
		public decimal TaxAmount { get; set; } // Tiền thuế đã trừ

		[Required(ErrorMessage = "Lương thực lĩnh là bắt buộc")]
		[Column(TypeName = "decimal(18, 2)")]
		public decimal NetSalary { get; set; } // Thực lĩnh cuối cùng

		[Required(ErrorMessage = "Trạng thái là bắt buộc")]
		[StringLength(20)]
		public string Status { get; set; } = "DRAFT"; // DRAFT / PAID

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public DateTime? UpdatedAt { get; set; }

		public DateTime? PaidAt { get; set; } // Ngày thanh toán

		// Navigation properties
		public User? User { get; set; }
	}
}
