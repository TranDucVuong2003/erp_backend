using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
    public class TicketLog
    {
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; }
        public virtual Ticket? Ticket { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }
        public virtual User? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // ✅ THÊM navigation property
        public virtual ICollection<TicketLogAttachment> Attachments { get; set; } = new List<TicketLogAttachment>();
    }
}