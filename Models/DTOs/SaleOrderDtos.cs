using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models.DTOs
{
    public class CreateSaleOrderWithItemsRequest
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Customer ID là bắt buộc")]
        public int CustomerId { get; set; }

        [Range(0, 100, ErrorMessage = "Xác suất phải từ 0-100%")]
        public int Probability { get; set; } = 0;

        [StringLength(2000, ErrorMessage = "Ghi chú không được vượt quá 2000 ký tự")]
        public string? Notes { get; set; }

        public int? TaxId { get; set; }

        public List<SaleOrderServiceItemDto> Services { get; set; } = new();
        public List<SaleOrderAddonItemDto> Addons { get; set; } = new();
    }

    public class SaleOrderServiceItemDto
    {
        [Required(ErrorMessage = "Service ID là bắt buộc")]
        public int ServiceId { get; set; }

        // ============================================================
        // PHẦN NÀY ĐÃ ĐƯỢC COMMENT - Quantity sẽ tự động lấy từ Service
        // ============================================================
        //[Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        //public int? Quantity { get; set; }

        // Không bắt buộc - sẽ lấy từ Service.Price nếu không cung cấp
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal? UnitPrice { get; set; }

        // Không bắt buộc - sẽ lấy từ Service.Notes nếu không cung cấp
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }

        // Thời gian (duration) - không bắt buộc
        public int? Duration { get; set; }

        // Template - không bắt buộc
        [StringLength(200, ErrorMessage = "Template không được vượt quá 200 ký tự")]
        public string? Template { get; set; }
    }

    public class SaleOrderAddonItemDto
    {
        [Required(ErrorMessage = "Addon ID là bắt buộc")]
        public int AddonId { get; set; }

        // ============================================================
        // PHẦN NÀY ĐÃ ĐƯỢC COMMENT - Quantity sẽ tự động lấy từ Addon
        // ============================================================
        //[Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        //public int? Quantity { get; set; }

        // Không bắt buộc - sẽ lấy từ Addon.Price nếu không cung cấp
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal? UnitPrice { get; set; }

        // Không bắt buộc - sẽ lấy từ Addon.Notes nếu không cung cấp
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }

        // Thời gian (duration) - không bắt buộc
        public int? Duration { get; set; }

        // Template - không bắt buộc
        [StringLength(200, ErrorMessage = "Template không được vượt quá 200 ký tự")]
        public string? Template { get; set; }
    }

    public class SaleOrderWithItemsResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public decimal Value { get; set; }
        public int Probability { get; set; }
        public string? Notes { get; set; }
        public int? TaxId { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<SaleOrderServiceDetailDto> Services { get; set; } = new();
        public List<SaleOrderAddonDetailDto> Addons { get; set; } = new();

        public string Message { get; set; } = "Tạo sale order thành công";
    }

    public class SaleOrderServiceDetailDto
    {
        //public int Id { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        //public int? Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        //public decimal TotalPrice { get; set; }
        //public string? Notes { get; set; }
        public int Duration { get; set; }
        public string Template { get; set; } = string.Empty;
    }

    public class SaleOrderAddonDetailDto
    {
        //public int Id { get; set; }
        public int AddonId { get; set; }
        public string AddonName { get; set; } = string.Empty;
        //public int? Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        //public decimal TotalPrice { get; set; }
        //public string? Notes { get; set; }
        public int Duration { get; set; }
        public string Template { get; set; } = string.Empty;
    }
}
