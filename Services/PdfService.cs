using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace erp_backend.Services
{
    public interface IPdfService
    {
        Task<byte[]> ConvertHtmlToPdfAsync(string htmlContent);
        Task InitializeBrowserAsync();
    }

    public class PdfService : IPdfService
    {
        private readonly ILogger<PdfService> _logger;
        private IBrowser? _browser;
        private bool _isInitialized = false;
        private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);

        public PdfService(ILogger<PdfService> logger)
        {
            _logger = logger;
        }

        public async Task InitializeBrowserAsync()
        {
            if (_isInitialized) return;

            await _initLock.WaitAsync();
            try
            {
                if (_isInitialized) return;

                _logger.LogInformation("Downloading Chromium browser for PuppeteerSharp...");
                
                // Download Chromium nếu chưa có
                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();

                _logger.LogInformation("Chromium downloaded successfully. Launching browser...");

                // Launch browser
                _browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    Args = new[]
                    {
                        "--no-sandbox",
                        "--disable-setuid-sandbox",
                        "--disable-dev-shm-usage",
                        "--disable-gpu"
                    }
                });

                _isInitialized = true;
                _logger.LogInformation("PuppeteerSharp PdfService initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize PuppeteerSharp browser");
                throw;
            }
            finally
            {
                _initLock.Release();
            }
        }

        public async Task<byte[]> ConvertHtmlToPdfAsync(string htmlContent)
        {
            try
            {
                // Ensure browser is initialized
                await InitializeBrowserAsync();

                if (_browser == null)
                {
                    throw new InvalidOperationException("Browser not initialized");
                }

                _logger.LogInformation("Starting HTML to PDF conversion with PuppeteerSharp. HTML length: {Length}", htmlContent.Length);

                // Create new page
                await using var page = await _browser.NewPageAsync();

                // Set content
                await page.SetContentAsync(htmlContent, new NavigationOptions
                {
                    WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
                });

                // Generate PDF
                var pdfBytes = await page.PdfDataAsync(new PdfOptions
                {
                    Format = PaperFormat.A4,
                    PrintBackground = true,
                    MarginOptions = new MarginOptions
                    {
                        Top = "0",
                        Right = "0",
                        Bottom = "0",
                        Left = "0"
                    },
                    PreferCSSPageSize = false
                });

                _logger.LogInformation("PDF generated successfully with PuppeteerSharp. Size: {Size} bytes", pdfBytes.Length);

                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to convert HTML to PDF with PuppeteerSharp");
                throw;
            }
        }
    }
}

