using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace erp_backend.Models
{
	public class Contract
	{
		public int Id { get; set; }

		[Required]
		public int SaleOrderId { get; set; }

		//[Required]
		//public int CustomerId { get; set; } 

		[Required]
		public int UserId { get; set; }

		// ✅ THÊM: Số hợp đồng - Tự động tăng
		public int NumberContract { get; set; }

		[StringLength(50)]
		public string Status { get; set; } = "Draft"; // Default status

		[StringLength(50)]
		public string? PaymentMethod { get; set; }

		[Range(0, double.MaxValue, ErrorMessage = "Tổng tiền chưa thuế phải lớn hơn hoặc bằng 0")]
		public decimal SubTotal { get; set; } // ✅ THÊM: Tổng tiền chưa thuế

		[Range(0, double.MaxValue, ErrorMessage = "Số tiền thuế phải lớn hơn hoặc bằng 0")]
		public decimal TaxAmount { get; set; } // ✅ THÊM: Số tiền thuế

		[Required]
		[Range(0, double.MaxValue, ErrorMessage = "Tổng tiền phải lớn hơn hoặc bằng 0")]
		public decimal TotalAmount { get; set; } // Tổng tiền sau thuế

		public DateTime Expiration { get; set; }

		[StringLength(2000)]
		public string? Notes { get; set; } // ✅ THÊM: Ghi chú

		// ✅ THÊM: Đánh dấu có xuất hóa đơn hay không
		public bool ExtractInvoices { get; set; } = false; // Mặc định là chưa xuất hóa đơn

		// ✅ PDF Storage Fields
		[StringLength(500)]
		public string? ContractPdfPath { get; set; } // Đường dẫn file PDF

		public DateTime? PdfGeneratedAt { get; set; } // Thời điểm tạo PDF

		public long? PdfFileSize { get; set; } // Kích thước file (bytes)

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public DateTime? UpdatedAt { get; set; }

		// Navigation properties (NO JsonIgnore - we handle it in DTOs)
		public User? User { get; set; }
		public SaleOrder? SaleOrder { get; set; }
		
		// ✅ THÊM: Navigation property đến MatchedTransactions
		[JsonIgnore] // Tránh circular reference khi serialize
		public ICollection<MatchedTransaction>? MatchedTransactions { get; set; }
	}
}
