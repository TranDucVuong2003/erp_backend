namespace erp_backend.Models.DTOs
{
	/// <summary>
	/// DTO cho thông tin user c? b?n trong department
	/// </summary>
	public class DepartmentUserInfo
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string? SecondaryEmail { get; set; }
		public string? PhoneNumber { get; set; }
		public string? Address { get; set; }
		public string Status { get; set; } = string.Empty;
		
		// Position info
		public int? PositionId { get; set; }
		public string? PositionName { get; set; }
		public int? PositionLevel { get; set; }
		
		// Role info
		public int RoleId { get; set; }
		public string RoleName { get; set; } = string.Empty;
		
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}

	/// <summary>
	/// DTO cho department v?i danh sách users
	/// </summary>
	public class DepartmentWithUsersResponse
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public int ResionId { get; set; }
		public string? ResionName { get; set; }
		public int TotalUsers { get; set; }
		public List<DepartmentUserInfo> Users { get; set; } = new List<DepartmentUserInfo>();
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}

	/// <summary>
	/// DTO cho response danh sách departments có users
	/// </summary>
	public class DepartmentsWithUsersListResponse
	{
		public int TotalDepartments { get; set; }
		public int TotalUsers { get; set; }
		public List<DepartmentWithUsersResponse> Departments { get; set; } = new List<DepartmentWithUsersResponse>();
	}
}
