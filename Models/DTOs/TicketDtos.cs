using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models.DTOs
{
    public class CreateTicketDto
    {
        [Required]
        [StringLength(500)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public string Priority { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [Range(1, 5)]
        public int UrgencyLevel { get; set; } = 1;

        public int? CreatedById { get; set; }

        public DateTime? Dateline { get; set; }
    }

    public class UpdateTicketDto
    {
        [Required]
        [StringLength(500)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Priority { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [Range(1, 5)]
        public int UrgencyLevel { get; set; } = 1;

        public int? AssignedToId { get; set; }

        public DateTime? Dateline { get; set; }
    }

    public class AssignTicketDto
    {
        public int? AssignedToId { get; set; }
    }

    public class UpdateTicketStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }

    public class CreateTicketLogDto
    {
        [Required]
        public int TicketId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }
    }

    public class UpdateTicketLogDto
    {
        [Required]
        public string Content { get; set; } = string.Empty;
    }

    public class CreateTicketCategoryDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateTicketCategoryDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }

    public class PatchTicketCategoryDto
    {
        [StringLength(100)]
        public string? Name { get; set; }
    }

    public class TicketResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int UrgencyLevel { get; set; }
        public int? AssignedToId { get; set; }
        public string? AssignedToName { get; set; }
        public int? CreatedById { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? Dateline { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class TicketLogResponseDto
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}