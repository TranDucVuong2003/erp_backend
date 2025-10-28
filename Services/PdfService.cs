using System.Diagnostics;
using System.Text;

namespace erp_backend.Services
{
    public interface IPdfService
    {
        Task<byte[]> ConvertHtmlToPdfAsync(string htmlContent);
        Task<byte[]> GeneratePdfFromUrlAsync(string url);
    }

    public class PdfService : IPdfService
    {
        private readonly ILogger<PdfService> _logger;
        private readonly string _wkhtmltopdfPath;

        public PdfService(ILogger<PdfService> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            
            // Đường dẫn đến wkhtmltopdf.exe
            _wkhtmltopdfPath = Path.Combine(env.ContentRootPath, "wkhtmltox", "bin", "wkhtmltopdf.exe");
            
            if (!File.Exists(_wkhtmltopdfPath))
            {
                _logger.LogWarning("wkhtmltopdf.exe not found at: {Path}", _wkhtmltopdfPath);
            }
        }

        public async Task<byte[]> ConvertHtmlToPdfAsync(string htmlContent)
        {
            try
            {
                if (!File.Exists(_wkhtmltopdfPath))
                {
                    throw new FileNotFoundException($"wkhtmltopdf.exe not found at: {_wkhtmltopdfPath}");
                }

                // Tạo file HTML tạm thời
                var tempHtmlFile = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.html");
                var tempPdfFile = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.pdf");

                try
                {
                    // Ghi nội dung HTML vào file tạm
                    await File.WriteAllTextAsync(tempHtmlFile, htmlContent, Encoding.UTF8);

                    // Cấu hình tham số cho wkhtmltopdf
                    // Xuất ra khổ A4 chuẩn (210mm x 297mm)
                    var arguments = new StringBuilder();
                    arguments.Append("--page-size A4 ");
                    arguments.Append("--orientation portrait ");
                    arguments.Append("--margin-top 10mm ");
                    arguments.Append("--margin-right 10mm ");
                    arguments.Append("--margin-bottom 10mm ");
                    arguments.Append("--margin-left 10mm ");
                    arguments.Append("--encoding UTF-8 ");
                    arguments.Append("--enable-local-file-access ");
                    arguments.Append("--disable-smart-shrinking ");
                    arguments.Append("--dpi 96 ");
                    arguments.Append("--no-stop-slow-scripts ");
                    arguments.Append("--javascript-delay 1000 ");
                    arguments.Append("--load-error-handling ignore ");
                    arguments.Append("--load-media-error-handling ignore ");
                    arguments.Append($"\"{tempHtmlFile}\" ");
                    arguments.Append($"\"{tempPdfFile}\"");

                    // Chạy wkhtmltopdf
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = _wkhtmltopdfPath,
                        Arguments = arguments.ToString(),
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    };

                    using var process = new Process { StartInfo = processStartInfo };
                    
                    process.Start();

                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();

                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                    {
                        _logger.LogError("wkhtmltopdf error: {Error}", error);
                        throw new Exception($"PDF generation failed: {error}");
                    }

                    // Đọc file PDF đã tạo
                    if (!File.Exists(tempPdfFile))
                    {
                        throw new Exception("PDF file was not created");
                    }

                    var pdfBytes = await File.ReadAllBytesAsync(tempPdfFile);
                    
                    _logger.LogInformation("PDF generated successfully. Size: {Size} bytes", pdfBytes.Length);
                    
                    return pdfBytes;
                }
                finally
                {
                    // Xóa các file tạm
                    try
                    {
                        if (File.Exists(tempHtmlFile))
                            File.Delete(tempHtmlFile);
                        if (File.Exists(tempPdfFile))
                            File.Delete(tempPdfFile);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete temporary files");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting HTML to PDF");
                throw;
            }
        }

        public async Task<byte[]> GeneratePdfFromUrlAsync(string url)
        {
            try
            {
                if (!File.Exists(_wkhtmltopdfPath))
                {
                    throw new FileNotFoundException($"wkhtmltopdf.exe not found at: {_wkhtmltopdfPath}");
                }

                var tempPdfFile = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.pdf");

                try
                {
                    // Cấu hình tham số cho wkhtmltopdf
                    var arguments = new StringBuilder();
                    arguments.Append("--page-size A4 ");
                    arguments.Append("--margin-top 10mm ");
                    arguments.Append("--margin-right 10mm ");
                    arguments.Append("--margin-bottom 10mm ");
                    arguments.Append("--margin-left 10mm ");
                    arguments.Append("--encoding UTF-8 ");
                    arguments.Append($"\"{url}\" ");
                    arguments.Append($"\"{tempPdfFile}\"");

                    // Chạy wkhtmltopdf
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = _wkhtmltopdfPath,
                        Arguments = arguments.ToString(),
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    using var process = new Process { StartInfo = processStartInfo };
                    
                    process.Start();

                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();

                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                    {
                        _logger.LogError("wkhtmltopdf error: {Error}", error);
                        throw new Exception($"PDF generation failed: {error}");
                    }

                    // Đọc file PDF đã tạo
                    if (!File.Exists(tempPdfFile))
                    {
                        throw new Exception("PDF file was not created");
                    }

                    var pdfBytes = await File.ReadAllBytesAsync(tempPdfFile);
                    
                    return pdfBytes;
                }
                finally
                {
                    // Xóa file tạm
                    try
                    {
                        if (File.Exists(tempPdfFile))
                            File.Delete(tempPdfFile);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete temporary PDF file");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF from URL");
                throw;
            }
        }
    }
}

