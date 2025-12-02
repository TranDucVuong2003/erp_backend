using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models.DTOs
{
    // ===== REQUEST DTOs =====
    
    public class CreateSaleKpiTargetRequest
    {
        [Required(ErrorMessage = "UserId là b?t bu?c")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Tháng là b?t bu?c")]
        [Range(1, 12, ErrorMessage = "Tháng ph?i t? 1 ??n 12")]
        public int Month { get; set; }

        [Required(ErrorMessage = "N?m là b?t bu?c")]
        [Range(2020, 2100, ErrorMessage = "N?m không h?p l?")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Target Amount là b?t bu?c")]
        [Range(1, double.MaxValue, ErrorMessage = "KPI Target ph?i l?n h?n 0")]
        public decimal TargetAmount { get; set; }

        [Required(ErrorMessage = "AssignedByUserId là b?t bu?c")]
        public int AssignedByUserId { get; set; }

        [StringLength(1000, ErrorMessage = "Notes không ???c v??t quá 1000 ký t?")]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateSaleKpiTargetRequest
    {
        [Required(ErrorMessage = "Id là b?t bu?c")]
        public int Id { get; set; }

        [Required(ErrorMessage = "UserId là b?t bu?c")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Tháng là b?t bu?c")]
        [Range(1, 12, ErrorMessage = "Tháng ph?i t? 1 ??n 12")]
        public int Month { get; set; }

        [Required(ErrorMessage = "N?m là b?t bu?c")]
        [Range(2020, 2100, ErrorMessage = "N?m không h?p l?")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Target Amount là b?t bu?c")]
        [Range(1, double.MaxValue, ErrorMessage = "KPI Target ph?i l?n h?n 0")]
        public decimal TargetAmount { get; set; }

        [Required(ErrorMessage = "AssignedByUserId là b?t bu?c")]
        public int AssignedByUserId { get; set; }

        [StringLength(1000, ErrorMessage = "Notes không ???c v??t quá 1000 ký t?")]
        public string? Notes { get; set; }

        public bool IsActive { get; set; }
    }

    // ===== RESPONSE DTOs =====
    
    /// <summary>
    /// DTO cho danh sách KPI Target (response g?n)
    /// </summary>
    public class SaleKpiTargetListDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string SaleUserName { get; set; } = string.Empty;
        public string SaleUserEmail { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TargetAmount { get; set; }
        public int AssignedByUserId { get; set; }
        public string AssignedByUserName { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO cho chi ti?t KPI Target (response ??y ?? h?n)
    /// </summary>
    public class SaleKpiTargetDetailDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public KpiUserDto? SaleUser { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TargetAmount { get; set; }
        public int AssignedByUserId { get; set; }
        public KpiUserDto? AssignedByUser { get; set; }
        public DateTime AssignedAt { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // ===== NESTED DTOs =====
    
    /// <summary>
    /// DTO g?n cho User trong KPI
    /// </summary>
    public class KpiUserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int? PositionId { get; set; }
        public string? PositionName { get; set; }
    }
}
