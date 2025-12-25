namespace erp_backend.Models
{
	// Bảng này lưu: BHXH, BHYT, BHTN
	public class InsurancePolicy
	{
		public int Id { get; set; }

		// Ví dụ: "BHXH", "BHYT"
		public string Code { get; set; }

		// Tên hiển thị: "Bảo hiểm Xã hội"
		public string Name { get; set; }

		// % Nhân viên phải đóng (VD: 8.0)
		public float EmployeeRate { get; set; }

		// % Công ty phải đóng (VD: 17.5 - Chi phí doanh nghiệp)
		public float EmployerRate { get; set; }

		// Loại trần bảo hiểm: "GOV_BASE" (Lương cơ sở) hoặc "REGION_MIN" (Lương tối thiểu vùng)
		// Để biết khi lương cao quá thì chặn trần ở đâu
		public string CapBaseType { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }
	}
}