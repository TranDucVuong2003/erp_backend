namespace erp_backend.DTO
{
	public class CreateUserRequest
	{
		public string Name { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;

		public int PositionId { get; set; }
		public int DepartmentId { get; set; }
		public int RoleId { get; set; }

		public string? PhoneNumber { get; set; }
		public string? Address { get; set; }

		public string? SecondaryEmail { get; set; }

		// Thêm trường firstLogin vào DTO
		public bool FirstLogin { get; set; }
		public string Status { get; set; } = "inactive";
	}
}
