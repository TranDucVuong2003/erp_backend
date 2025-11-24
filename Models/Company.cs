using System;
using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
	public class Company
	{
		public int Id { get; set; }

		[StringLength(20)]
		public string Mst { get; set; } = string.Empty;           // Mã số thuế

		[Required]
		[StringLength(255)]
		public string TenDoanhNghiep { get; set; } = string.Empty; // Tên doanh nghiệp

		[StringLength(255)]
		public string? TenGiaoDich { get; set; }                   // Tên giao dịch

		[StringLength(20)]
		public string? SoDienThoai { get; set; }                   // SĐT

		[StringLength(500)]
		public string? DiaChi { get; set; }                        // Địa chỉ

		[StringLength(255)]
		public string? DaiDienPhapLuat { get; set; }               // Người đại diện

		public DateTime? NgayCapGiayPhep { get; set; }             // Ngày cấp GPKD

		public DateTime? NgayHoatDong { get; set; }               // Ngày hoạt động

		[StringLength(50)]
		public string? TrangThai { get; set; }                     // Trạng thái

		[StringLength(255)]
		public string? Url { get; set; }                           // Website

		public int UserId { get; set; }                           // Chủ sở hữu

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }

		public User? User { get; set; }
	}
}
