namespace erp_backend.Models.DTOs
{
	public class CreateQuoteDto
	{
		public int? CustomerId { get; set; }
		public List<CustomServiceItem>? CustomService { get; set; }
		public string? FilePath { get; set; }
		public decimal Amount { get; set; }
		
		public int? CreatedByUserId { get; set; } // User ID của người tạo quote
		
		public int? CategoryServiceAddonId { get; set; } // Category Service Addon ID
		
		public List<QuoteServiceDto>? Services { get; set; }
		public List<QuoteAddonDto>? Addons { get; set; }
	}

	public class QuoteServiceDto
	{
		public int ServiceId { get; set; }
		
		// ✅ THÊM: Quantity, UnitPrice, Notes
		public int Quantity { get; set; } = 1;
		
		public decimal UnitPrice { get; set; } = 0; // Default = 0 means use price from DB
		
		public string? Notes { get; set; }
	}

	public class QuoteAddonDto
	{
		public int AddonId { get; set; }
		
		// ✅ THÊM: Quantity, UnitPrice, Notes
		public int Quantity { get; set; } = 1;
		
		public decimal UnitPrice { get; set; } = 0; // Default = 0 means use price from DB
		
		public string? Notes { get; set; }
	}
}
