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
		

	}

	public class LoginResponse
	{
		public string Token { get; set; } = string.Empty;
		public DateTime Expiration { get; set; }
		public UserInfo User { get; set; } = null!;
		public string Message { get; set; } = "Đăng nhập thành công";
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
}
