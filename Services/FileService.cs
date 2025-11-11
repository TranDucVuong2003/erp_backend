namespace erp_backend.Services
{
	public class FileService : IFileService
	{
		private readonly IWebHostEnvironment _environment;
		private readonly ILogger<FileService> _logger;
		private readonly long _maxFileSize = 20 * 1024 * 1024; // 10MB
		
		private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
		private readonly string[] _allowedDocExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".zip", ".rar" };

		public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
		{
			_environment = environment;
			_logger = logger;
		}

		public async Task<(bool Success, string? FilePath, string? ErrorMessage)> SaveFileAsync(
			IFormFile file, 
			string subfolder = "ticket-logs")
		{
			try
			{
				if (file == null || file.Length == 0)
					return (false, null, "File không hợp lệ");

				if (file.Length > _maxFileSize)
					return (false, null, $"File vượt quá kích thước cho phép ({_maxFileSize / 1024 / 1024}MB)");

				var extension = Path.GetExtension(file.FileName).ToLower();
				if (!IsAllowedFileType(file.FileName))
					return (false, null, "Loại file không được hỗ trợ");

				// Tạo thư mục nếu chưa tồn tại
				var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", subfolder);
				if (!Directory.Exists(uploadFolder))
					Directory.CreateDirectory(uploadFolder);

				// Tạo tên file unique
				var uniqueFileName = $"{Guid.NewGuid()}{extension}";
				var filePath = Path.Combine(uploadFolder, uniqueFileName);

				// Lưu file
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await file.CopyToAsync(stream);
				}

				// Trả về đường dẫn tương đối
				var relativePath = Path.Combine("uploads", subfolder, uniqueFileName).Replace("\\", "/");
				return (true, relativePath, null);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error saving file {FileName}", file?.FileName);
				return (false, null, $"Lỗi lưu file: {ex.Message}");
			}
		}

		public async Task<bool> DeleteFileAsync(string filePath)
		{
			try
			{
				if (string.IsNullOrEmpty(filePath))
					return false;

				var fullPath = Path.Combine(_environment.WebRootPath, filePath);
				if (File.Exists(fullPath))
				{
					await Task.Run(() => File.Delete(fullPath));
					return true;
				}
				return false;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting file {FilePath}", filePath);
				return false;
			}
		}

		public bool IsImageFile(string fileName)
		{
			var extension = Path.GetExtension(fileName).ToLower();
			return _allowedImageExtensions.Contains(extension);
		}

		public bool IsAllowedFileType(string fileName)
		{
			var extension = Path.GetExtension(fileName).ToLower();
			return _allowedImageExtensions.Contains(extension) || 
				   _allowedDocExtensions.Contains(extension);
		}

		public long GetMaxFileSize() => _maxFileSize;
	}
}
