namespace erp_backend.Services
{
    public interface IFileService
    {
        Task<(bool Success, string? FilePath, string? ErrorMessage)> SaveFileAsync(
            IFormFile file, 
            string subfolder = "ticket-logs");
            
        Task<bool> DeleteFileAsync(string filePath);
        
        bool IsImageFile(string fileName);
        
        bool IsAllowedFileType(string fileName);
        
        long GetMaxFileSize();
    }
}
