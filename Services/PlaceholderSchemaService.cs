using erp_backend.Models;
using System.Reflection;

namespace erp_backend.Services
{
	/// <summary>
	/// Service qu?n lý schema và g?i ý placeholders cho template editor
	/// T? ??ng phát hi?n available fields t? các Models
	/// </summary>
	public interface IPlaceholderSchemaService
	{
		/// <summary>
		/// L?y t?t c? placeholders có s?n theo template type
		/// </summary>
		Dictionary<string, List<PlaceholderField>> GetAvailablePlaceholders(string templateType);

		/// <summary>
		/// L?y placeholders cho m?t entity c? th? (Contract, Customer, ...)
		/// </summary>
		List<PlaceholderField> GetPlaceholdersForEntity(string entityName);

		/// <summary>
		/// L?y t?t c? entity names có trong h? th?ng
		/// </summary>
		List<string> GetAvailableEntities();

		/// <summary>
		/// Validate xem placeholders trong template có t?n t?i trong schema không
		/// </summary>
		(bool isValid, List<string> invalidPlaceholders) ValidatePlaceholders(List<string> placeholders, string templateType);
	}

	public class PlaceholderSchemaService : IPlaceholderSchemaService
	{
		private readonly ILogger<PlaceholderSchemaService> _logger;
		private static readonly Dictionary<string, Type> _entityTypeMap = new()
		{
			{ "Contract", typeof(Contract) },
			{ "Customer", typeof(Customer) },
			{ "SaleOrder", typeof(SaleOrder) },
			{ "Service", typeof(Service) },
			{ "Addon", typeof(Addon) },
			{ "User", typeof(User) }
		};

		// Mapping template types to relevant entities
		private static readonly Dictionary<string, List<string>> _templateTypeEntities = new()
		{
			{ "contract", new List<string> { "Contract", "Customer", "SaleOrder", "Service", "User" } },
			{ "quote", new List<string> { "Customer", "SaleOrder", "Service", "Addon", "User" } },
			{ "invoice", new List<string> { "Contract", "Customer", "SaleOrder", "Service" } },
			{ "salary_notification", new List<string> { "User" } },
			{ "email", new List<string> { "Customer", "User" } },
			{ "notification", new List<string> { "User", "Customer" } }
		};

		public PlaceholderSchemaService(ILogger<PlaceholderSchemaService> logger)
		{
			_logger = logger;
		}

		/// <summary>
		/// L?y t?t c? placeholders có s?n theo template type
		/// Tr? v? dictionary v?i key là entity name (Contract, Customer...)
		/// và value là list các fields
		/// </summary>
		public Dictionary<string, List<PlaceholderField>> GetAvailablePlaceholders(string templateType)
		{
			var result = new Dictionary<string, List<PlaceholderField>>();

			// N?u không có mapping cho template type này, tr? v? t?t c? entities
			var entities = _templateTypeEntities.ContainsKey(templateType.ToLower())
				? _templateTypeEntities[templateType.ToLower()]
				: _entityTypeMap.Keys.ToList();

			foreach (var entityName in entities)
			{
				result[entityName] = GetPlaceholdersForEntity(entityName);
			}

			_logger.LogDebug("Generated placeholders for template type '{TemplateType}': {EntityCount} entities",
				templateType, result.Count);

			return result;
		}

		/// <summary>
		/// L?y danh sách fields c?a m?t entity d??i d?ng placeholders
		/// </summary>
		public List<PlaceholderField> GetPlaceholdersForEntity(string entityName)
		{
			if (!_entityTypeMap.ContainsKey(entityName))
			{
				_logger.LogWarning("Entity '{EntityName}' not found in schema", entityName);
				return new List<PlaceholderField>();
			}

			var entityType = _entityTypeMap[entityName];
			var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

			var fields = new List<PlaceholderField>();

			foreach (var prop in properties)
			{
				// B? qua navigation properties và collections
				if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string) && !prop.PropertyType.IsValueType)
				{
					// Ki?m tra xem có ph?i là collection không
					if (typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType))
					{
						continue;
					}

					// ?ây là navigation property ??n (nh? Customer, SaleOrder)
					// Ta s? x? lý nó ? level khác
					continue;
				}

				var field = new PlaceholderField
				{
					Name = prop.Name,
					Placeholder = $"{{{{{entityName}.{prop.Name}}}}}",
					Type = GetSimpleTypeName(prop.PropertyType),
					Description = GetPropertyDescription(prop),
					IsRequired = IsPropertyRequired(prop),
					Example = GetExampleValue(prop)
				};

				fields.Add(field);
			}

			return fields.OrderBy(f => f.Name).ToList();
		}

		/// <summary>
		/// L?y danh sách t?t c? entities có s?n
		/// </summary>
		public List<string> GetAvailableEntities()
		{
			return _entityTypeMap.Keys.ToList();
		}

		/// <summary>
		/// Validate xem các placeholders có h?p l? v?i template type không
		/// </summary>
		public (bool isValid, List<string> invalidPlaceholders) ValidatePlaceholders(
			List<string> placeholders, 
			string templateType)
		{
			var availableFields = GetAvailablePlaceholders(templateType);
			var validPlaceholderSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			// T?o set c?a t?t c? placeholders h?p l?
			foreach (var entity in availableFields)
			{
				foreach (var field in entity.Value)
				{ 
					validPlaceholderSet.Add(field.Placeholder);
					// C?ng ch?p nh?n format không có entity prefix: {{FieldName}}
					validPlaceholderSet.Add($"{{{{{field.Name}}}}}");
				}
			}

			var invalidPlaceholders = new List<string>();

			foreach (var placeholder in placeholders)
			{
				// Format: {{Entity.Field}} ho?c {{Field}}
				var cleanPlaceholder = placeholder.Trim();
				
				if (!validPlaceholderSet.Contains(cleanPlaceholder))
				{
					invalidPlaceholders.Add(cleanPlaceholder);
				}
			}

			bool isValid = invalidPlaceholders.Count == 0;

			if (!isValid)
			{
				_logger.LogWarning(
					"Found {Count} invalid placeholders for template type '{TemplateType}': {Invalid}",
					invalidPlaceholders.Count,
					templateType,
					string.Join(", ", invalidPlaceholders)
				);
			}

			return (isValid, invalidPlaceholders);
		}

		// ============ Helper Methods ============

		private static string GetSimpleTypeName(Type type)
		{
			// X? lý nullable types
			var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

			return underlyingType.Name switch
			{
				"String" => "string",
				"Int32" => "number",
				"Int64" => "number",
				"Decimal" => "number",
				"Double" => "number",
				"Boolean" => "boolean",
				"DateTime" => "date",
				_ => "string"
			};
		}

		private static string GetPropertyDescription(PropertyInfo prop)
		{
			// Có th? m? r?ng ?? ??c t? XML documentation ho?c attributes
			return prop.PropertyType.Name switch
			{
				"DateTime" => $"{prop.Name} (??nh d?ng: dd/MM/yyyy)",
				"Decimal" => $"{prop.Name} (s? ti?n)",
				"Boolean" => $"{prop.Name} (true/false)",
				_ => prop.Name
			};
		}

		private static bool IsPropertyRequired(PropertyInfo prop)
		{
			// Ki?m tra Required attribute
			return prop.GetCustomAttribute<System.ComponentModel.DataAnnotations.RequiredAttribute>() != null;
		}

		private static string GetExampleValue(PropertyInfo prop)
		{
			var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

			return type.Name switch
			{
				"String" => prop.Name.Contains("Email") ? "example@company.com" :
							prop.Name.Contains("Phone") ? "0912345678" :
							prop.Name.Contains("Name") ? "Nguy?n V?n A" :
							"Sample text",
				"Int32" => "123",
				"Decimal" => "15000000",
				"DateTime" => "01/01/2025",
				"Boolean" => "true",
				_ => ""
			};
		}
	}

	/// <summary>
	/// DTO cho thông tin placeholder field
	/// </summary>
	public class PlaceholderField
	{
		public string Name { get; set; } = string.Empty;
		public string Placeholder { get; set; } = string.Empty;
		public string Type { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public bool IsRequired { get; set; }
		public string Example { get; set; } = string.Empty;
	}
}
