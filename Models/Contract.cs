using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
	public class Contract
	{
		public int Id { get; set; }

		[Required]
		[StringLength(255)]
		public string Name { get; set; } = string.Empty;

		public int? ServiceId { get; set; } 

		public int? AddonsId { get; set; } 

		[Required]
		public int CustomerId { get; set; } 

		[Required]
		public int UserId { get; set; } 

		[StringLength(50)]
		public string Status { get; set; } = "Draft"; 

		[StringLength(50)]
		public string? PaymentMethod { get; set; }

		[Range(0, double.MaxValue, ErrorMessage = "Tổng tiền chưa thuế phải lớn hơn hoặc bằng 0")]
		public decimal SubTotal { get; set; } // ✅ THÊM: Tổng tiền chưa thuế

		public int? TaxId { get; set; } // Liên kết đến bảng Tax

		[Range(0, double.MaxValue, ErrorMessage = "Số tiền thuế phải lớn hơn hoặc bằng 0")]
		public decimal TaxAmount { get; set; } // ✅ THÊM: Số tiền thuế

		[Required]
		[Range(0, double.MaxValue, ErrorMessage = "Tổng tiền phải lớn hơn hoặc bằng 0")]
		public decimal TotalAmount { get; set; } // Tổng tiền sau thuế



		public DateTime Expiration { get; set; }

		[StringLength(2000)]
		public string? Notes { get; set; } // ✅ THÊM: Ghi chú

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public DateTime? UpdatedAt { get; set; }

		// Navigation properties
		public Customer? Customer { get; set; }
		public User? User { get; set; }
		public Service? Service { get; set; }
		public Addon? Addon { get; set; }
		public Tax? Tax { get; set; }
	}
}
