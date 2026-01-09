using System.Text.RegularExpressions;
using erp_backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace erp_backend.Services
{
	/// <summary>
	/// Service ?? render HTML template v?i dynamic placeholders
	/// H? tr? syntax: {{VariableName}} ho?c {{Entity.Property}}
	/// </summary>
	public interface ITemplateRenderService
	{
		/// <summary>
		/// Render template theo code v?i data ??ng
		/// </summary>
		Task<string> RenderTemplateAsync(string templateCode, Dictionary<string, string> data);

		/// <summary>
		/// Render template theo ID v?i data ??ng
		/// </summary>
		Task<string> RenderTemplateByIdAsync(int templateId, Dictionary<string, string> data);

		/// <summary>
		/// Render template v?i object data (h? tr? nested properties)
		/// </summary>
		Task<string> RenderTemplateWithObjectAsync(int templateId, object data);

		/// <summary>
		/// Render template theo code v?i object data
		/// </summary>
		Task<string> RenderTemplateWithObjectByCodeAsync(string templateCode, object data);

		/// <summary>
		/// T? ??ng phát hi?n t?t c? placeholders trong HTML
		/// </summary>
		List<string> ExtractPlaceholders(string htmlContent);

		/// <summary>
		/// Validate xem t?t c? placeholders ?ã có data ch?a
		/// </summary>
		(bool isValid, List<string> missingPlaceholders) ValidateTemplateData(
			string htmlContent, 
			Dictionary<string, string> data);
	}

	public class TemplateRenderService : ITemplateRenderService
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<TemplateRenderService> _logger;

		public TemplateRenderService(
			ApplicationDbContext context,
			ILogger<TemplateRenderService> logger)
		{
			_context = context;
			_logger = logger;
		}

		/// <summary>
		/// Render template theo code v?i data ??ng
		/// </summary>
		public async Task<string> RenderTemplateAsync(string templateCode, Dictionary<string, string> data)
		{
			var template = await _context.DocumentTemplates
				.FirstOrDefaultAsync(t => t.Code == templateCode && t.IsActive);

			if (template == null)
			{
				throw new InvalidOperationException($"Template '{templateCode}' không t?n t?i ho?c ?ã b? vô hi?u hóa");
			}

			_logger.LogInformation(
				"Rendering template '{Code}' (ID: {Id}) with {DataCount} data fields",
				template.Code, template.Id, data.Count
			);

			return ReplaceAllPlaceholders(template.HtmlContent, data);
		}

		/// <summary>
		/// Render template theo ID v?i data ??ng
		/// </summary>
		public async Task<string> RenderTemplateByIdAsync(int templateId, Dictionary<string, string> data)
		{
			var template = await _context.DocumentTemplates.FindAsync(templateId);

			if (template == null)
			{
				throw new InvalidOperationException($"Template ID {templateId} không t?n t?i");
			}

			if (!template.IsActive)
			{
				_logger.LogWarning("Attempting to render inactive template ID: {TemplateId}", templateId);
			}

			_logger.LogInformation(
				"Rendering template ID {Id} ('{Name}') with {DataCount} data fields",
				template.Id, template.Name, data.Count
			);

			return ReplaceAllPlaceholders(template.HtmlContent, data);
		}

		/// <summary>
		/// Render template v?i object data (h? tr? nested properties)
		/// VD: data có th? là { Contract = {...}, Customer = {...} }
		/// </summary>
		public async Task<string> RenderTemplateWithObjectAsync(int templateId, object data)
		{
			var template = await _context.DocumentTemplates.FindAsync(templateId);

			if (template == null)
			{
				throw new InvalidOperationException($"Template ID {templateId} không t?n t?i");
			}

			_logger.LogInformation(
				"Rendering template ID {Id} with object data",
				template.Id
			);

			// Convert object to flat dictionary
			var flatData = FlattenObject(data);
			return ReplaceAllPlaceholders(template.HtmlContent, flatData);
		}

		/// <summary>
		/// Render template theo code v?i object data
		/// </summary>
		public async Task<string> RenderTemplateWithObjectByCodeAsync(string templateCode, object data)
		{
			var template = await _context.DocumentTemplates
				.FirstOrDefaultAsync(t => t.Code == templateCode && t.IsActive);

			if (template == null)
			{
				throw new InvalidOperationException($"Template '{templateCode}' không t?n t?i");
			}

			_logger.LogInformation(
				"Rendering template '{Code}' with object data",
				templateCode
			);

			var flatData = FlattenObject(data);
			return ReplaceAllPlaceholders(template.HtmlContent, flatData);
		}

		/// <summary>
		/// T? ??ng phát hi?n t?t c? placeholders trong HTML (dùng cho UI)
		/// Pattern: {{VariableName}} ho?c {{ VariableName }} ho?c {{Entity.Property}}
		/// </summary>
		public List<string> ExtractPlaceholders(string htmlContent)
		{
			if (string.IsNullOrWhiteSpace(htmlContent))
			{
				return new List<string>();
			}

			// Regex ?? tìm t?t c? {{...}}
			var matches = Regex.Matches(htmlContent, @"\{\{([^}]+)\}\}");

			var placeholders = matches
				.Select(m => m.Groups[1].Value.Trim()) // L?y tên bi?n và trim kho?ng tr?ng
				.Where(p => !string.IsNullOrWhiteSpace(p)) // Lo?i b? placeholders r?ng
				.Distinct(StringComparer.OrdinalIgnoreCase) // Lo?i b? trùng l?p (case-insensitive)
				.OrderBy(x => x)
				.ToList();

			_logger.LogDebug(
				"Extracted {Count} unique placeholders from HTML content ({TotalLength} chars)",
				placeholders.Count, htmlContent.Length
			);

			return placeholders;
		}

		/// <summary>
		/// Validate xem t?t c? placeholders ?ã có data ch?a
		/// </summary>
		public (bool isValid, List<string> missingPlaceholders) ValidateTemplateData(
			string htmlContent,
			Dictionary<string, string> data)
		{
			var placeholders = ExtractPlaceholders(htmlContent);

			// Tìm các placeholder ch?a có trong data (case-insensitive)
			var missingPlaceholders = placeholders
				.Where(p => !data.Keys.Any(k => k.Equals(p, StringComparison.OrdinalIgnoreCase)))
				.ToList();

			bool isValid = missingPlaceholders.Count == 0;

			if (!isValid)
			{
				_logger.LogWarning(
					"Template validation failed. Missing {Count} placeholders: {Missing}",
					missingPlaceholders.Count,
					string.Join(", ", missingPlaceholders)
				);
			}

			return (isValid, missingPlaceholders);
		}

		// ============ Private Helper Methods ============

		/// <summary>
		/// Thay th? t?t c? placeholders d?ng {{Key}} v?i giá tr? t? dictionary
		/// H? tr? c? {{Entity.Property}} và {{Property}}
		/// Support case-insensitive replacement
		/// </summary>
		private string ReplaceAllPlaceholders(string htmlContent, Dictionary<string, string> data)
		{
			var result = htmlContent;
			int replacementCount = 0;

			foreach (var kvp in data)
			{
				// Replace {{Key}} và {{ Key }} (v?i ho?c không có kho?ng tr?ng)
				// Case-insensitive
				var pattern = $@"\{{\{{\s*{Regex.Escape(kvp.Key)}\s*\}}\}}";
				var matches = Regex.Matches(result, pattern, RegexOptions.IgnoreCase);

				if (matches.Count > 0)
				{
					result = Regex.Replace(
						result,
						pattern,
						kvp.Value ?? string.Empty, // X? lý null value
						RegexOptions.IgnoreCase
					);

					replacementCount += matches.Count;
				}
			}

			_logger.LogDebug(
				"Replaced {Count} placeholder occurrences in template",
				replacementCount
			);

			// Tùy ch?n: Xóa các placeholders ch?a ???c replace
			// B? comment dòng d??i n?u mu?n hi?n th? "[Missing Data]" cho các bi?n ch?a có giá tr?
			// result = Regex.Replace(result, @"\{\{[^}]+\}\}", "<span style='color:red;'>[Missing Data]</span>");

			return result;
		}

		/// <summary>
		/// Flatten nested object thành dictionary v?i keys d?ng "Entity.Property"
		/// VD: { Contract: { Id: 1, NumberContract: 123 } } 
		///  => { "Contract.Id": "1", "Contract.NumberContract": "123" }
		/// </summary>
		private Dictionary<string, string> FlattenObject(object obj, string prefix = "")
		{
			var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			if (obj == null)
			{
				return result;
			}

			var type = obj.GetType();
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (var prop in properties)
			{
				var value = prop.GetValue(obj);

				if (value == null)
				{
					continue;
				}

				var key = string.IsNullOrEmpty(prefix) 
					? prop.Name 
					: $"{prefix}.{prop.Name}";

				var propType = prop.PropertyType;

				// Ki?m tra n?u là simple type (string, number, date, bool)
				if (IsSimpleType(propType))
				{
					result[key] = FormatValue(value, propType);

					// C?ng thêm version không có prefix cho backward compatibility
					// VD: c? {{Contract.NumberContract}} và {{NumberContract}} ??u work
					if (!string.IsNullOrEmpty(prefix))
					{
						result[prop.Name] = FormatValue(value, propType);
					}
				}
				// N?u là nested object, ?? quy
				else if (propType.IsClass && propType != typeof(string))
				{
					// Tránh ?? quy vô h?n v?i collections
					if (!typeof(System.Collections.IEnumerable).IsAssignableFrom(propType))
					{
						var nestedData = FlattenObject(value, key);
						foreach (var kvp in nestedData)
						{
							result[kvp.Key] = kvp.Value;
						}
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Ki?m tra xem type có ph?i là simple type không
		/// </summary>
		private static bool IsSimpleType(Type type)
		{
			var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

			return underlyingType.IsPrimitive
				|| underlyingType.IsEnum
				|| underlyingType == typeof(string)
				|| underlyingType == typeof(decimal)
				|| underlyingType == typeof(DateTime)
				|| underlyingType == typeof(DateTimeOffset)
				|| underlyingType == typeof(TimeSpan)
				|| underlyingType == typeof(Guid);
		}

		/// <summary>
		/// Format giá tr? theo type
		/// </summary>
		private static string FormatValue(object value, Type type)
		{
			var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

			return underlyingType.Name switch
			{
				"DateTime" => ((DateTime)value).ToString("dd/MM/yyyy"),
				"Decimal" => ((decimal)value).ToString("N0"), // Format v?i d?u ph?y ng?n cách hàng nghìn
				"Boolean" => ((bool)value) ? "Có" : "Không",
				_ => value.ToString() ?? string.Empty
			};
		}
	}
}
