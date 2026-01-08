using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models.DTOs
{
	public class CreateSalaryContractDto
	{
		[Required(ErrorMessage = "UserId là b?t bu?c")]
		public int UserId { get; set; }

		[Required]
		[Range(0, double.MaxValue, ErrorMessage = "L??ng c? b?n ph?i l?n h?n ho?c b?ng 0")]
		public decimal BaseSalary { get; set; }

		[Required]
		[Range(0, double.MaxValue, ErrorMessage = "L??ng b?o hi?m ph?i l?n h?n ho?c b?ng 0")]
		public decimal InsuranceSalary { get; set; }

		[Required]
		public string ContractType { get; set; } = "OFFICIAL";

		[Range(0, 20, ErrorMessage = "S? ng??i ph? thu?c ph?i t? 0 ??n 20")]
		public int DependentsCount { get; set; } = 0;

		public bool HasCommitment08 { get; set; } = false;

		// File ?ính kèm (không b?t bu?c)
		public IFormFile? Attachment { get; set; }
	}

	public class UpdateSalaryContractDto
	{
		[Range(0, double.MaxValue, ErrorMessage = "L??ng c? b?n ph?i l?n h?n ho?c b?ng 0")]
		public decimal? BaseSalary { get; set; }

		[Range(0, double.MaxValue, ErrorMessage = "L??ng b?o hi?m ph?i l?n h?n ho?c b?ng 0")]
		public decimal? InsuranceSalary { get; set; }

		public string? ContractType { get; set; }

		[Range(0, 20, ErrorMessage = "S? ng??i ph? thu?c ph?i t? 0 ??n 20")]
		public int? DependentsCount { get; set; }

		public bool? HasCommitment08 { get; set; }

		// File ?ính kèm m?i (n?u c?n c?p nh?t)
		public IFormFile? Attachment { get; set; }
	}

	public class SalaryContractResponseDto
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public decimal BaseSalary { get; set; }
		public decimal InsuranceSalary { get; set; }
		public string ContractType { get; set; } = "OFFICIAL";
		public int DependentsCount { get; set; }
		public bool HasCommitment08 { get; set; }
		public string? AttachmentPath { get; set; }
		public string? AttachmentFileName { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public string? UserName { get; set; }
		public string? UserEmail { get; set; }
	}

	/// <summary>
	/// DTO for uploading commitment08 file
	/// </summary>
	public class UploadCommitment08Dto
	{
		[Required(ErrorMessage = "File is required")]
		public IFormFile File { get; set; } = null!;
	}
}
