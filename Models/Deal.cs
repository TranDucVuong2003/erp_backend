using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
    public enum DealPriority
    {
        Low,
        Medium,
        High
    }

    public enum DealStage
    {
        Lead,
        Qualified,
        Proposal,
        Negotiation,
        ClosedWon,
        ClosedLost
    }

    public class Deal
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Giá tr? ph?i l?n h?n 0")]
        public decimal Value { get; set; }

        public DateOnly? ExpectedCloseDate { get; set; }

        public DateOnly? ActualCloseDate { get; set; }

        public DealPriority Priority { get; set; } = DealPriority.Medium;

        [Range(0, 100, ErrorMessage = "Xác su?t ph?i t? 0-100%")]
        public int Probability { get; set; } = 0;

        public DealStage Stage { get; set; } = DealStage.Lead;

        [StringLength(2000)]
        public string? Notes { get; set; }

        public int? AssignedTo { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Customer? Customer { get; set; }
        public User? AssignedUser { get; set; }
        public User? CreatedByUser { get; set; }
    }
}