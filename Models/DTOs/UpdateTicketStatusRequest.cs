using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models.DTOs
{
    public class UpdateTicketStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}