using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models.DTOs
{
    // DTO for creating a customer
    public class CreateCustomerDto
    {
        [Required]
        [StringLength(20)]
        public string CustomerType { get; set; } = string.Empty;

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

        [StringLength(20)]
        public string? Status { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // DTO for customer response with creator info
    public class CustomerResponseDto
    {
        public int Id { get; set; }
        public string CustomerType { get; set; } = string.Empty;
        
        // Individual fields
        public string? Name { get; set; }
        public string? Address { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? IdNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Domain { get; set; }

        // Company fields
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public DateTime? EstablishedDate { get; set; }
        public string? TaxCode { get; set; }
        public string? CompanyDomain { get; set; }

        // Representative info
        public string? RepresentativeName { get; set; }
        public string? RepresentativePosition { get; set; }
        public string? RepresentativeIdNumber { get; set; }
        public string? RepresentativePhone { get; set; }
        public string? RepresentativeEmail { get; set; }

        // Technical contact
        public string? TechContactName { get; set; }
        public string? TechContactPhone { get; set; }
        public string? TechContactEmail { get; set; }

        public bool IsActive { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Creator info
        public int? CreatedByUserId { get; set; }
        public UserBasicInfoDto? CreatedByUser { get; set; }
    }

    // Basic user info DTO (to avoid circular references)
    public class UserBasicInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    // Các class CustomerInfo, UpdateCustomerResponse, DeleteCustomerResponse
    // ?ã ???c ??nh ngh?a trong AuthDtos.cs, không c?n ??nh ngh?a l?i ? ?ây
}
