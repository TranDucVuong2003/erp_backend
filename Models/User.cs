using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace erp_backend.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty;

        public int PositionId { get; set; } 

        public int DepartmentId { get; set; }

		[StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        public int RoleId { get; set; } 

        // Foreign key tự tham chiếu - Quản lý trực tiếp
        public int? ManagerId { get; set; }

        [EmailAddress]
        [StringLength(150)]
        public string? SecondaryEmail { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }

        //public bool firstLogin { get; set; }

        // Navigation properties
        public Roles? Role { get; set; }
        public Positions? Position { get; set; }
        public Departments? Department { get; set; }
        
        // Navigation cho quản lý
        [JsonIgnore]
        public User? Manager { get; set; }
        
        // Danh sách nhân viên cấp dưới
        [JsonIgnore]
        public ICollection<User>? Subordinates { get; set; }
	}
}