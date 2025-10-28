using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models.DTOs
{
    public class UpdateSaleOrderStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}