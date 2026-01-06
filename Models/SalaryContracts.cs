using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace erp_backend.Models
{
	public class SalaryContracts
	{
		[Key]
		public int Id { get; set; }

		[Required(ErrorMessage = "UserId là bắt buộc")]
		public int UserId { get; set; }

		// --- NHÓM 1: LƯƠNG THỰC TẾ ---
		[Required]
		[Range(0, double.MaxValue)]
		[Column(TypeName = "decimal(18, 0)")] // Dùng decimal cho tiền tệ
		public decimal BaseSalary { get; set; } // Lương thực nhận (VD: 20.000.000) - Dùng tính công, tính thuế

		// --- NHÓM 2: CẤU HÌNH BẢO HIỂM ---
		[Required]
		[Column(TypeName = "decimal(18, 0)")]
		public decimal InsuranceSalary { get; set; } // Lương đóng bảo hiểm (VD: 5.682.000)
		// Quy ước: Để = 0 sẽ tự động tính theo mức sàn, > 0 sẽ đóng theo số tiền cụ thể

		// --- NHÓM 3: CẤU HÌNH THUẾ (Luật 2026) ---
		[Required]
		public string ContractType { get; set; } = "OFFICIAL"; // Các giá trị: "OFFICIAL" (Chính thức), "FREELANCE" (Vãng lai)

		[Range(0, 20, ErrorMessage = "Số người phụ thuộc không hợp lệ")]
		public int DependentsCount { get; set; } = 0; // Số người phụ thuộc (để nhân với 6.2 triệu)

		public bool HasCommitment08 { get; set; } = false; // Có làm cam kết 08 không? (Cho CTV lương thấp)

		// --- NHÓM 4: TÀI LIỆU ĐÍNH KÈM ---
		[StringLength(500)]
		public string? AttachmentPath { get; set; } // Đường dẫn file đính kèm (VD: /uploads/contracts/user123_contract.pdf)

		[StringLength(255)]
		public string? AttachmentFileName { get; set; } // Tên file gốc (VD: hop_dong_lao_dong.pdf)

		// --- NHÓM 5: QUẢN LÝ ---
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }

		// Navigation property
		[ForeignKey("UserId")]
		public User? User { get; set; }
	}
}