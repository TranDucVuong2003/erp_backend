namespace erp_backend.Services
{
	public interface IFileUploadService
	{
		Task<(string filePath, string fileName)> SaveFileAsync(IFormFile file, string folder);
		Task<bool> DeleteFileAsync(string filePath);
		string GetFileUrl(string filePath);
		bool IsValidFileExtension(string fileName, string[] allowedExtensions);
		bool IsValidFileSize(long fileSize, long maxSizeInMB);
	}

	public class FileUploadService : IFileUploadService
	{
		private readonly IWebHostEnvironment _environment;
		private readonly ILogger<FileUploadService> _logger;

		public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
		{
			_environment = environment;
			_logger = logger;
		}

		public async Task<(string filePath, string fileName)> SaveFileAsync(IFormFile file, string folder)
		{
			try
			{
				if (file == null || file.Length == 0)
					throw new ArgumentException("File không h?p l?");

				// T?o th? m?c n?u ch?a t?n t?i
				var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
				if (!Directory.Exists(uploadFolder))
					Directory.CreateDirectory(uploadFolder);

				// T?o tên file unique
				var fileName = file.FileName;
				var fileExtension = Path.GetExtension(fileName);
				var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
				var filePath = Path.Combine(uploadFolder, uniqueFileName);

				// L?u file
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await file.CopyToAsync(stream);
				}

				_logger.LogInformation("?ã l?u file: {FileName} t?i {FilePath}", fileName, filePath);

				// Tr? v? ???ng d?n t??ng ??i
				var relativePath = $"/uploads/{folder}/{uniqueFileName}";
				return (relativePath, fileName);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi l?u file: {FileName}", file?.FileName);
				throw;
			}
		}

		public async Task<bool> DeleteFileAsync(string filePath)
		{
			try
			{
				if (string.IsNullOrEmpty(filePath))
					return false;

				var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
				
				if (File.Exists(fullPath))
				{
					await Task.Run(() => File.Delete(fullPath));
					_logger.LogInformation("?ã xóa file: {FilePath}", filePath);
					return true;
				}
				
				return false;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "L?i khi xóa file: {FilePath}", filePath);
				return false;
			}
		}

		public string GetFileUrl(string filePath)
		{
			return filePath ?? string.Empty;
		}

		public bool IsValidFileExtension(string fileName, string[] allowedExtensions)
		{
			var extension = Path.GetExtension(fileName).ToLowerInvariant();
			return allowedExtensions.Contains(extension);
		}

		public bool IsValidFileSize(long fileSize, long maxSizeInMB)
		{
			var maxSizeInBytes = maxSizeInMB * 1024 * 1024;
			return fileSize <= maxSizeInBytes;
		}
	}
}
