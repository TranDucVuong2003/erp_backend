using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models.DTOs
{
	public class LoginRequest
	{
		[Required(ErrorMessage = "Email là bắt buộc")]
		[EmailAddress(ErrorMessage = "Email không hợp lệ")]
		public string Email { get; set; } = string.Empty;
		
		[Required(ErrorMessage = "Mật khẩu là bắt buộc")]
		public string Password { get; set; } = string.Empty;
		
		public string? DeviceInfo { get; set; }
	}

	public class LoginResponse
	{
		public string AccessToken { get; set; } = string.Empty;
		public DateTime ExpiresAt { get; set; }
		public UserInfo User { get; set; } = new UserInfo();
		public string Message { get; set; } = "Đăng nhập thành công";
	}

	public class RefreshTokenResponse
	{
		public string AccessToken { get; set; } = string.Empty;
		public DateTime ExpiresAt { get; set; }
	}

	public class SessionInfo
	{
		public int Id { get; set; }
		public string DeviceInfo { get; set; } = string.Empty;
		public string IpAddress { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public DateTime ExpiresAt { get; set; }
		public bool IsCurrentSession { get; set; }
	}
	
	public class UserInfo
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Position { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty;
	}

	public class RegisterRequest
	{
		[Required(ErrorMessage = "Tên là bắt buộc")]
		[StringLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự")]
		public string Name { get; set; } = string.Empty;
		
		[Required(ErrorMessage = "Email là bắt buộc")]
		[EmailAddress(ErrorMessage = "Email không hợp lệ")]
		public string Email { get; set; } = string.Empty;
		
		[Required(ErrorMessage = "Mật khẩu là bắt buộc")]
		[MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
		public string Password { get; set; } = string.Empty;
		

		
		[StringLength(100, ErrorMessage = "Chức vụ không được vượt quá 100 ký tự")]
		public string Position { get; set; } = string.Empty;
		
		public string Role { get; set; } = "User";
	}

	public class UpdateUserResponse
	{
		public string Message { get; set; } = string.Empty;
		public UserInfo User { get; set; } = null!;
		public DateTime UpdatedAt { get; set; }
	}

	public class DeleteUserResponse
	{
		public string Message { get; set; } = string.Empty;
		public UserInfo DeletedUser { get; set; } = null!;
		public DateTime DeletedAt { get; set; }
	}

	public class CustomerInfo
	{
		public int Id { get; set; }
		public string? Name { get; set; }
		public string? Email { get; set; }
		public string? PhoneNumber { get; set; }
		public string? CompanyName { get; set; }
		public string? RepresentativeName { get; set; }
		public string? RepresentativeEmail { get; set; }
		public string CustomerType { get; set; } = string.Empty;
		public bool IsActive { get; set; }
		public string? Status { get; set; }
	}

	public class DeleteCustomerResponse
	{
		public string Message { get; set; } = string.Empty;
		public CustomerInfo DeletedCustomer { get; set; } = null!;
		public DateTime DeletedAt { get; set; }
	}

	public class UpdateCustomerResponse
	{
		public string Message { get; set; } = string.Empty;
		public CustomerInfo Customer { get; set; } = null!;
		public DateTime UpdatedAt { get; set; }
	}

	public class ServiceInfo
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
		public decimal Price { get; set; }
		public int? Quantity { get; set; }
		public string? Category { get; set; }
		public bool IsActive { get; set; }
		public string? Notes { get; set; }
	}

	public class UpdateServiceResponse
	{
		public string Message { get; set; } = string.Empty;
		public ServiceInfo Service { get; set; } = null!;
		public DateTime UpdatedAt { get; set; }
	}

	public class DeleteServiceResponse
	{
		public string Message { get; set; } = string.Empty;
		public ServiceInfo DeletedService { get; set; } = null!;
		public DateTime DeletedAt { get; set; }
	}

	public class AddonInfo
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string? Description { get; set; }
		public decimal Price { get; set; }
		public int? Quantity { get; set; }
		public string? Type { get; set; }
		public bool IsActive { get; set; }
		public string? Notes { get; set; }
	}

	public class UpdateAddonResponse
	{
		public string Message { get; set; } = string.Empty;
		public AddonInfo Addon { get; set; } = null!;
		public DateTime UpdatedAt { get; set; }
	}

	public class DeleteAddonResponse
	{
		public string Message { get; set; } = string.Empty;
		public AddonInfo DeletedAddon { get; set; } = null!;
		public DateTime DeletedAt { get; set; }
	}
}
