using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models.DTOs
{
    // ===== REQUEST DTOs =====
    
    public class CreateContractRequest
    {
        [Required(ErrorMessage = "SaleOrderId là b?t bu?c")]
        public int SaleOrderId { get; set; }

        [Required(ErrorMessage = "UserId là b?t bu?c")]
        public int UserId { get; set; }

        [StringLength(50, ErrorMessage = "Status không ???c v??t quá 50 ký t?")]
        public string Status { get; set; } = "Draft";

        [StringLength(50, ErrorMessage = "PaymentMethod không ???c v??t quá 50 ký t?")]
        public string? PaymentMethod { get; set; }

        [Required(ErrorMessage = "Expiration là b?t bu?c")]
        public DateTime Expiration { get; set; }

        [StringLength(2000, ErrorMessage = "Notes không ???c v??t quá 2000 ký t?")]
        public string? Notes { get; set; }
    }

    // ===== RESPONSE DTOs =====
    
    public class ContractResponse
    {
        public int Id { get; set; }
        public int SaleOrderId { get; set; }
        public SaleOrderBasicDto? SaleOrder { get; set; }
        public int UserId { get; set; }
        public UserBasicDto? User { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PaymentMethod { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime Expiration { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ContractListItemDto
    {
        public int Id { get; set; }
        public int SaleOrderId { get; set; }
        public string SaleOrderTitle { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime Expiration { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ===== NESTED DTOs =====
    
    public class SaleOrderBasicDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public CustomerBasicDto? Customer { get; set; }
        public decimal Value { get; set; }
        public int Probability { get; set; }
        public int? TaxId { get; set; }
        public TaxBasicDto? Tax { get; set; }
        public List<ServiceItemDto> Services { get; set; } = new();
        public List<AddonItemDto> Addons { get; set; } = new();
    }

    public class CustomerBasicDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? CompanyName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class UserBasicDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class TaxBasicDto
    {
        public int Id { get; set; }
        public decimal Rate { get; set; }
    }

    public class ServiceItemDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int? Quantity { get; set; }
        public int? Duration { get; set; }
        public string? Template { get; set; }
    }

    public class AddonItemDto
    {
        public int AddonId { get; set; }
        public string AddonName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int? Quantity { get; set; }
        public int? Duration { get; set; }
        public string? Template { get; set; }
    }
}
