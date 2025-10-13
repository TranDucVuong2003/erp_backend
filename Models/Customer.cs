using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
	public class Customer
	{
		public int Id { get; set; }


		// Individual fields
		[StringLength(100)]
		public string? Name { get; set; }

		[StringLength(500)]
		public string? Address { get; set; }

		public DateTime? BirthDate { get; set; }

		[StringLength(50)]
		public string? IdNumber { get; set; }

		[StringLength(20)]
		public string? PhoneNumber { get; set; }

		[EmailAddress]
		[StringLength(150)]
		public string? Email { get; set; }

		[StringLength(100)]
		public string? Domain { get; set; }


		// Company fields
		[StringLength(200)]
		public string? CompanyName { get; set; }

		[StringLength(500)]
		public string? CompanyAddress { get; set; }

		public DateTime? EstablishedDate { get; set; }

		[StringLength(50)]
		public string? TaxCode { get; set; }

		[StringLength(100)]
		public string? CompanyDomain { get; set; }


		// Representative info
		[StringLength(100)]
		public string? RepresentativeName { get; set; }

		[StringLength(100)]
		public string? RepresentativePosition { get; set; }

		[StringLength(50)]
		public string? RepresentativeIdNumber { get; set; }

		[StringLength(20)]
		public string? RepresentativePhone { get; set; }

		[EmailAddress]
		[StringLength(150)]
		public string? RepresentativeEmail { get; set; }


		// Technical contact
		[StringLength(100)]
		public string? TechContactName { get; set; }

		[StringLength(20)]
		public string? TechContactPhone { get; set; }

		[EmailAddress]
		[StringLength(150)]
		public string? TechContactEmail { get; set; }


		// Display fields

		public bool IsActive { get; set; } = true;

		// Common fields

		[StringLength(20)]
		public string CustomerType { get; set; } = string.Empty; 

		[StringLength(20)]
		public string? Status { get; set; }

		[StringLength(2000)]
		public string? Notes { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }
	}
}
