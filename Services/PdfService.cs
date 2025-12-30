using IronPdf;

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
        private readonly ChromePdfRenderer _renderer;

        public PdfService(ILogger<PdfService> logger)
        {
            _logger = logger;
            
            // Cấu hình ChromePdfRenderer
            _renderer = new ChromePdfRenderer();
            
            // Cấu hình rendering options
            _renderer.RenderingOptions.PaperSize = IronPdf.Rendering.PdfPaperSize.A4;
            _renderer.RenderingOptions.PaperOrientation = IronPdf.Rendering.PdfPaperOrientation.Portrait;
            _renderer.RenderingOptions.MarginTop = 10;
            _renderer.RenderingOptions.MarginBottom = 10;
            _renderer.RenderingOptions.MarginLeft = 10;
            _renderer.RenderingOptions.MarginRight = 10;
            _renderer.RenderingOptions.CssMediaType = IronPdf.Rendering.PdfCssMediaType.Print;
            _renderer.RenderingOptions.PrintHtmlBackgrounds = true;
            _renderer.RenderingOptions.EnableJavaScript = false;
            _renderer.RenderingOptions.Timeout = 60; // 60 seconds timeout
            
            _logger.LogInformation("IronPDF PdfService initialized successfully");
        }

        public async Task<byte[]> ConvertHtmlToPdfAsync(string htmlContent)
        {
            try
            {
                _logger.LogInformation("Starting HTML to PDF conversion with IronPDF. HTML length: {Length}", htmlContent.Length);

                // Convert HTML to PDF using IronPDF
                var pdfDocument = await Task.Run(() => _renderer.RenderHtmlAsPdf(htmlContent));
                
                // Convert to byte array
                var pdfBytes = pdfDocument.BinaryData;
                
                _logger.LogInformation("PDF generated successfully with IronPDF. Size: {Size} bytes", pdfBytes.Length);
                
                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting HTML to PDF with IronPDF");
                throw new Exception($"Failed to generate PDF: {ex.Message}", ex);
            }
        }

        public async Task<byte[]> GeneratePdfFromUrlAsync(string url)
        {
            try
            {
                _logger.LogInformation("Starting URL to PDF conversion with IronPDF. URL: {Url}", url);

                // Convert URL to PDF using IronPDF
                var pdfDocument = await Task.Run(() => _renderer.RenderUrlAsPdf(url));
                
                // Convert to byte array
                var pdfBytes = pdfDocument.BinaryData;
                
                _logger.LogInformation("PDF generated successfully from URL with IronPDF. Size: {Size} bytes", pdfBytes.Length);
                
                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF from URL with IronPDF");
                throw new Exception($"Failed to generate PDF from URL: {ex.Message}", ex);
            }
        }
    }
}

