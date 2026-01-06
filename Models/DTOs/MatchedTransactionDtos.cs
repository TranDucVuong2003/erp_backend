using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models.DTOs
{
    /// <summary>
    /// Request DTO ?? match payment t? API banking
    /// </summary>
    public class MatchPaymentRequest
    {
        [Required(ErrorMessage = "TransactionId là b?t bu?c")]
        public string TransactionId { get; set; } = string.Empty;

        [Required(ErrorMessage = "ContractId là b?t bu?c")]
        public int ContractId { get; set; }

        [Required(ErrorMessage = "Amount là b?t bu?c")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount ph?i l?n h?n 0")]
        public decimal Amount { get; set; }

        public string? ReferenceNumber { get; set; }

        public string Status { get; set; } = "?ã thanh toán"; // ? S?a: b? d?u ; th?a và thêm default value

        [Required]
        public DateTime TransactionDate { get; set; }

        public string? TransactionContent { get; set; }

        public string? BankBrandName { get; set; }

        public string? AccountNumber { get; set; }

        public string? Notes { get; set; }
    }

    /// <summary>
    /// Response DTO cho matched transaction
    /// </summary>
    public class MatchedTransactionResponse
    {
        public int Id { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public int? ContractId { get; set; }
        public decimal Amount { get; set; }
        public string? ReferenceNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public DateTime MatchedAt { get; set; }
        public string? TransactionContent { get; set; }
        public string? BankBrandName { get; set; }
        public string? AccountNumber { get; set; }
        public int? MatchedByUserId { get; set; }
        public string? MatchedByUserName { get; set; }
        public string? Notes { get; set; }

        // Thông tin contract
        public ContractBasicInfo? Contract { get; set; }
    }

    /// <summary>
    /// Thông tin c? b?n c?a Contract
    /// </summary>
    public class ContractBasicInfo
    {
        public int Id { get; set; }
        public int NumberContract { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime Expiration { get; set; }
    }

    /// <summary>
    /// Response cho danh sách giao d?ch ch?a match (t? API banking)
    /// </summary>
    public class UnmatchedTransactionResponse
    {
        public string Id { get; set; } = string.Empty;
        public string BankBrandName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public decimal AmountOut { get; set; }
        public decimal AmountIn { get; set; }
        public decimal Accumulated { get; set; }
        public string TransactionContent { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? SubAccount { get; set; }
        public string BankAccountId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response cho th?ng kê matched transactions
    /// </summary>
    public class MatchedTransactionStatisticsResponse
    {
        public int TotalMatched { get; set; }
        public decimal TotalAmount { get; set; }
        public int MatchedToday { get; set; }
        public int MatchedThisMonth { get; set; }
        public Dictionary<string, int> ByStatus { get; set; } = new();
        public Dictionary<string, decimal> AmountByStatus { get; set; } = new();
    }
}
