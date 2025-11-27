using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
    public class AccountActivationToken
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Token { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime ExpiresAt { get; set; }
        
        public bool IsUsed { get; set; } = false;
        
        public DateTime? UsedAt { get; set; }
        
        // Navigation property
        public User? User { get; set; }
    }
}
