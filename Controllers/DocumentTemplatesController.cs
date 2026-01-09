using erp_backend.Data;
using erp_backend.Models;
using erp_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class DocumentTemplatesController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly ITemplateRenderService _templateRenderService;
		private readonly IPlaceholderSchemaService _placeholderSchemaService;
		private readonly ILogger<DocumentTemplatesController> _logger;

		public DocumentTemplatesController(
			ApplicationDbContext context,
			ITemplateRenderService templateRenderService,
			IPlaceholderSchemaService placeholderSchemaService,
			ILogger<DocumentTemplatesController> logger)
		{
			_context = context;
			_templateRenderService = templateRenderService;
			_placeholderSchemaService = placeholderSchemaService;
			_logger = logger;
		}

		/// <summary>
		/// Lấy danh sách tất cả templates (có thể filter theo type)
		/// GET: api/DocumentTemplates?type=contract
		/// </summary>
		[HttpGet]
		public async Task<ActionResult<IEnumerable<DocumentTemplate>>> GetTemplates([FromQuery] string? type = null)
		{
			try
			{
				var query = _context.DocumentTemplates
					.Include(t => t.CreatedByUser)
					.AsQueryable();

				if (!string.IsNullOrWhiteSpace(type))
				{
					query = query.Where(t => t.TemplateType.ToLower() == type.ToLower());
				}

				var templates = await query
					.Where(t => t.IsActive)
					.OrderByDescending(t => t.IsDefault)
					.ThenBy(t => t.Name)
					.ToListAsync();

				return Ok(new
				{
					success = true,
					totalCount = templates.Count,
					data = templates
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting document templates");
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi server khi lấy danh sách templates",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// Lấy template theo ID
		/// GET: api/DocumentTemplates/5
		/// </summary>
		[HttpGet("{id}")]
		public async Task<ActionResult<DocumentTemplate>> GetTemplate(int id)
		{
			try
			{
				var template = await _context.DocumentTemplates
					.Include(t => t.CreatedByUser)
					.FirstOrDefaultAsync(t => t.Id == id);

				if (template == null)
				{
					return NotFound(new { message = "Không tìm thấy template" });
				}

				return Ok(new
				{
					success = true,
					data = template
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting template with ID: {TemplateId}", id);
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi server khi lấy template",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// Lấy template với auto-detected placeholders
		/// GET: api/DocumentTemplates/with-placeholders/5
		/// </summary>
		[HttpGet("with-placeholders/{id}")]
		public async Task<ActionResult> GetTemplateWithPlaceholders(int id)
		{
			try
			{
				var template = await _context.DocumentTemplates
					.Include(t => t.CreatedByUser)
					.FirstOrDefaultAsync(t => t.Id == id);

				if (template == null)
				{
					return NotFound(new { message = "Không tìm thấy template" });
				}

				// Auto-detect placeholders từ HTML content
				var detectedPlaceholders = _templateRenderService.ExtractPlaceholders(template.HtmlContent);

				return Ok(new
				{
					success = true,
					data = new
					{
						template = template,
						detectedPlaceholders = detectedPlaceholders,
						placeholderCount = detectedPlaceholders.Count
					}
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting template with placeholders, ID: {TemplateId}", id);
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi server khi lấy template với placeholders",
					error = ex.Message
				});
			}
		}

		
		/// <summary>
		/// Lấy RAW HTML content của template theo code (không bao gồm trong JSON)
		/// GET: api/DocumentTemplates/by-code/{code}/raw-html
		/// </summary>
		[HttpGet("by-code/raw-html/{code}")]
		public async Task<IActionResult> GetTemplateRawHtml(string code)
		{
			try
			{
				var template = await _context.DocumentTemplates
					.FirstOrDefaultAsync(t => t.Code == code && t.IsActive);

				if (template == null)
				{
					return NotFound(new { message = $"Template với code '{code}' không tồn tại" });
				}

				// Trả về HTML trực tiếp, không JSON serialize
				return Content(template.HtmlContent, "text/html", System.Text.Encoding.UTF8);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting raw HTML template by code: {Code}", code);
				return StatusCode(500, "Lỗi server khi lấy template");
			}
		}

		/// <summary>
		/// Tạo template mới (chỉ admin)
		/// POST: api/DocumentTemplates
		/// </summary>
		[HttpPost]
		public async Task<ActionResult<DocumentTemplate>> CreateTemplate(DocumentTemplate template)
		{
			try
			{
				// Validate model
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// Kiểm tra code đã tồn tại chưa
				if (await _context.DocumentTemplates.AnyAsync(t => t.Code == template.Code))
				{
					return BadRequest(new { message = $"Code '{template.Code}' đã tồn tại" });
				}

				// Lấy UserId từ JWT token
				var userIdClaim = User.FindFirst("userid")?.Value;
				if (userIdClaim != null)
				{
					template.CreatedByUserId = int.Parse(userIdClaim);
				}

				// Nếu đặt làm default, bỏ default của các template cùng loại khác
				if (template.IsDefault)
				{
					var existingDefaults = await _context.DocumentTemplates
						.Where(t => t.TemplateType == template.TemplateType && t.IsDefault)
						.ToListAsync();

					foreach (var existingDefault in existingDefaults)
					{
						existingDefault.IsDefault = false;
						existingDefault.UpdatedAt = DateTime.UtcNow;
					}
				}

				template.CreatedAt = DateTime.UtcNow;
				template.Version = 1;

				_context.DocumentTemplates.Add(template);
				await _context.SaveChangesAsync();

				// Reload với navigation property
				var savedTemplate = await _context.DocumentTemplates
					.Include(t => t.CreatedByUser)
					.FirstOrDefaultAsync(t => t.Id == template.Id);

				_logger.LogInformation("Created new template: {TemplateName} (Code: {Code}) by user {UserId}",
					template.Name, template.Code, template.CreatedByUserId);

				return CreatedAtAction(nameof(GetTemplate), new { id = savedTemplate!.Id }, new
				{
					success = true,
					message = "Tạo template thành công",
					data = savedTemplate
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating template");
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi server khi tạo template",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// Cập nhật template (chỉ admin)
		/// PUT: api/DocumentTemplates/5
		/// </summary>
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateTemplate(int id, DocumentTemplate template)
		{
			try
			{
				if (id != template.Id)
				{
					return BadRequest(new { message = "ID không khớp" });
				}

				var existing = await _context.DocumentTemplates.FindAsync(id);
				if (existing == null)
				{
					return NotFound(new { message = "Không tìm thấy template" });
				}

				// Kiểm tra code trùng (ngoại trừ chính nó)
				if (await _context.DocumentTemplates.AnyAsync(t => t.Code == template.Code && t.Id != id))
				{
					return BadRequest(new { message = $"Code '{template.Code}' đã được sử dụng bởi template khác" });
				}

				// Nếu đặt làm default, bỏ default của các template cùng loại khác
				if (template.IsDefault && !existing.IsDefault)
				{
					var existingDefaults = await _context.DocumentTemplates
						.Where(t => t.TemplateType == template.TemplateType && t.IsDefault && t.Id != id)
						.ToListAsync();

					foreach (var existingDefault in existingDefaults)
					{
						existingDefault.IsDefault = false;
						existingDefault.UpdatedAt = DateTime.UtcNow;
					}
				}

				// Update fields
				existing.Name = template.Name;
				existing.TemplateType = template.TemplateType;
				existing.Code = template.Code;
				existing.HtmlContent = template.HtmlContent;
				existing.Description = template.Description;
				existing.IsActive = template.IsActive;
				existing.IsDefault = template.IsDefault;
				existing.Version++; // Tăng version
				existing.UpdatedAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();

				_logger.LogInformation("Updated template ID: {TemplateId} to version {Version}", id, existing.Version);

				return Ok(new
				{
					success = true,
					message = "Cập nhật template thành công",
					data = existing
				});
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!await _context.DocumentTemplates.AnyAsync(e => e.Id == id))
				{
					return NotFound(new { message = "Không tìm thấy template" });
				}
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating template with ID: {TemplateId}", id);
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi server khi cập nhật template",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// Xóa mềm template (chỉ admin)
		/// DELETE: api/DocumentTemplates/5
		/// </summary>
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteTemplate(int id)
		{
			try
			{
				var template = await _context.DocumentTemplates.FindAsync(id);
				if (template == null)
				{
					return NotFound(new { message = "Không tìm thấy template" });
				}

				// Soft delete
				template.IsActive = false;
				template.UpdatedAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();

				_logger.LogInformation("Soft deleted template ID: {TemplateId}", id);

				return Ok(new
				{
					success = true,
					message = "Xóa template thành công"
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting template with ID: {TemplateId}", id);
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi server khi xóa template",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// Đặt template làm mặc định (chỉ admin)
		/// PATCH: api/DocumentTemplates/5/set-default
		/// </summary>
		[HttpPatch("{id}/set-default")]
		public async Task<IActionResult> SetAsDefault(int id)
		{
			try
			{
				var template = await _context.DocumentTemplates.FindAsync(id);
				if (template == null)
				{
					return NotFound(new { message = "Không tìm thấy template" });
				}

				// Bỏ default của các template cùng loại khác
				var existingDefaults = await _context.DocumentTemplates
					.Where(t => t.TemplateType == template.TemplateType && t.IsDefault && t.Id != id)
					.ToListAsync();

				foreach (var existingDefault in existingDefaults)
				{
					existingDefault.IsDefault = false;
					existingDefault.UpdatedAt = DateTime.UtcNow;
				}

				template.IsDefault = true;
				template.UpdatedAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();

				_logger.LogInformation("Set template ID: {TemplateId} as default for type: {TemplateType}",
					id, template.TemplateType);

				return Ok(new
				{
					success = true,
					message = $"Đã đặt '{template.Name}' làm template mặc định cho loại {template.TemplateType}"
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error setting template as default with ID: {TemplateId}", id);
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi server khi đặt template mặc định",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// Lấy danh sách các loại template có sẵn
		/// GET: api/DocumentTemplates/types
		/// </summary>
		[HttpGet("types")]
		public async Task<ActionResult> GetTemplateTypes()
		{
			try
			{
				var types = await _context.DocumentTemplates
					.Where(t => t.IsActive)
					.GroupBy(t => t.TemplateType)
					.Select(g => new
					{
						Type = g.Key,
						Count = g.Count(),
						HasDefault = g.Any(t => t.IsDefault)
					})
					.ToListAsync();

				return Ok(new
				{
					success = true,
					data = types
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting template types");
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi server khi lấy danh sách loại template",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// Tự động phát hiện placeholders từ HTML content
		/// POST: api/DocumentTemplates/extract-placeholders
		/// Body: { "htmlContent": "<html>{{Name}} {{Email}}</html>" }
		/// </summary>
		[HttpPost("extract-placeholders")]
		public ActionResult<List<string>> ExtractPlaceholders([FromBody] ExtractPlaceholdersRequest request)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(request.HtmlContent))
				{
					return BadRequest(new { message = "htmlContent không được để trống" });
				}

				var placeholders = _templateRenderService.ExtractPlaceholders(request.HtmlContent);

				return Ok(new
				{
					success = true,
					placeholders = placeholders,
					count = placeholders.Count
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error extracting placeholders");
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi server khi phát hiện placeholders",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// Validate dữ liệu trước khi render template
		/// POST: api/DocumentTemplates/validate/5
		/// Body: { "EmployeeName": "John", "BaseSalary": "5000" }
		/// </summary>
		[HttpPost("validate/{id}")]
		public async Task<ActionResult> ValidateTemplateData(int id, [FromBody] Dictionary<string, string> data)
		{
			try
			{
				var template = await _context.DocumentTemplates.FindAsync(id);
				if (template == null)
				{
					return NotFound(new { message = "Không tìm thấy template" });
				}

				var (isValid, missingPlaceholders) = _templateRenderService.ValidateTemplateData(
					template.HtmlContent,
					data
				);

				if (!isValid)
				{
					return BadRequest(new
					{
						success = false,
						message = "Dữ liệu không hợp lệ",
						isValid = false,
						missingPlaceholders = missingPlaceholders
					});
				}

				return Ok(new
				{
					success = true,
					message = "Dữ liệu hợp lệ",
					isValid = true,
					providedFields = data.Keys.ToList()
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error validating template data for ID: {TemplateId}", id);
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi server khi validate dữ liệu",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// Render template theo ID với data động
		/// POST: api/DocumentTemplates/render/5
		/// Body: { "EmployeeName": "Nguyễn Văn A", "BaseSalary": "15,000,000" }
		/// </summary>
		[HttpPost("render/{id}")]
		public async Task<IActionResult> RenderTemplate(int id, [FromBody] Dictionary<string, string> data)
		{
			try
			{
				var template = await _context.DocumentTemplates.FindAsync(id);
				if (template == null)
				{
					return NotFound(new { message = "Không tìm thấy template" });
				}

				// Validate trước khi render
				var (isValid, missingPlaceholders) = _templateRenderService.ValidateTemplateData(
					template.HtmlContent,
					data
				);

				if (!isValid)
				{
					_logger.LogWarning(
						"Rendering template ID {TemplateId} with {MissingCount} missing placeholders: {Missing}",
						id,
						missingPlaceholders.Count,
						string.Join(", ", missingPlaceholders)
					);
				}

				var renderedHtml = await _templateRenderService.RenderTemplateByIdAsync(id, data);

				_logger.LogInformation(
					"Successfully rendered template ID: {TemplateId} (Code: {Code})",
					id,
					template.Code
				);

				// Trả về HTML trực tiếp
				return Content(renderedHtml, "text/html", System.Text.Encoding.UTF8);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error rendering template with ID: {TemplateId}", id);
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi server khi render template",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// Render template theo code với data động
		/// POST: api/DocumentTemplates/render-by-code/SALARY_NOTIFY_V2
		/// Body: { "EmployeeName": "Nguyễn Văn A", "BaseSalary": "15,000,000" }
		/// </summary>
		[HttpPost("render-by-code/{code}")]
		public async Task<IActionResult> RenderTemplateByCode(string code, [FromBody] Dictionary<string, string> data)
		{
			try
			{
				var template = await _context.DocumentTemplates
					.FirstOrDefaultAsync(t => t.Code == code && t.IsActive);

				if (template == null)
				{
					return NotFound(new { message = $"Template với code '{code}' không tồn tại" });
				}

				// Validate trước khi render
				var (isValid, missingPlaceholders) = _templateRenderService.ValidateTemplateData(
					template.HtmlContent,
					data
				);

				if (!isValid)
				{
					_logger.LogWarning(
						"Rendering template Code '{Code}' with {MissingCount} missing placeholders: {Missing}",
						code,
						missingPlaceholders.Count,
						string.Join(", ", missingPlaceholders)
					);
				}

				var renderedHtml = await _templateRenderService.RenderTemplateAsync(code, data);

				_logger.LogInformation(
					"Successfully rendered template by code: {Code} (ID: {TemplateId})",
					code,
					template.Id
				);

				// Trả về HTML trực tiếp
				return Content(renderedHtml, "text/html", System.Text.Encoding.UTF8);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error rendering template by code: {Code}", code);
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi server khi render template",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// Lấy danh sách available placeholders theo template type
		/// GET: api/DocumentTemplates/schema/placeholders?templateType=contract
		/// </summary>
		[HttpGet("schema/placeholders")]
		public ActionResult GetAvailablePlaceholders([FromQuery] string? templateType = "contract")
		{
			try
			{
				var placeholders = _placeholderSchemaService.GetAvailablePlaceholders(templateType ?? "contract");

				return Ok(new
				{
					success = true,
					templateType = templateType,
					data = placeholders,
					entityCount = placeholders.Count,
					totalFields = placeholders.Sum(e => e.Value.Count)
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting available placeholders for type: {TemplateType}", templateType);
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi khi lấy danh sách placeholders",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// Lấy placeholders của một entity cụ thể
		/// GET: api/DocumentTemplates/schema/placeholders/Contract
		/// </summary>
		[HttpGet("schema/placeholders/{entityName}")]
		public ActionResult GetPlaceholdersForEntity(string entityName)
		{
			try
			{
				var placeholders = _placeholderSchemaService.GetPlaceholdersForEntity(entityName);

				if (placeholders.Count == 0)
				{
					return NotFound(new
					{
						success = false,
						message = $"Entity '{entityName}' không tồn tại trong schema"
					});
				}

				return Ok(new
				{
					success = true,
					entity = entityName,
					placeholders = placeholders,
					count = placeholders.Count
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting placeholders for entity: {EntityName}", entityName);
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi khi lấy placeholders",
					error = ex.Message
				});
			}
		}

		
		/// <summary>
		/// Validate placeholders có hợp lệ với template type không
		/// POST: api/DocumentTemplates/schema/validate-placeholders
		/// Body: { "placeholders": ["{{Contract.Id}}", "{{InvalidField}}"], "templateType": "contract" }
		/// </summary>
		[HttpPost("schema/validate-placeholders")]
		public ActionResult ValidatePlaceholderSchema([FromBody] ValidatePlaceholdersRequest request)
		{
			try
			{
				if (request.Placeholders == null || request.Placeholders.Count == 0)
				{
					return BadRequest(new { message = "Placeholders không được để trống" });
				}

				var (isValid, invalidPlaceholders) = _placeholderSchemaService.ValidatePlaceholders(
					request.Placeholders,
					request.TemplateType ?? "contract"
				);

				if (!isValid)
				{
					return BadRequest(new
					{
						success = false,
						message = "Có placeholders không hợp lệ",
						isValid = false,
						invalidPlaceholders = invalidPlaceholders
					});
				}

				return Ok(new
				{
					success = true,
					message = "Tất cả placeholders đều hợp lệ",
					isValid = true,
					validatedCount = request.Placeholders.Count
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error validating placeholder schema");
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi khi validate placeholders",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// Render template with structured object data (hỗ trợ nested properties)
		/// POST: api/DocumentTemplates/render-with-object/{id}
		/// Body: { "Contract": { "Id": 1, "NumberContract": 123 }, "Customer": { "Name": "..." } }
		/// </summary>
		[HttpPost("render-with-object/{id}")]
		public async Task<IActionResult> RenderTemplateWithObject(int id, [FromBody] object data)
		{
			try
			{
				var template = await _context.DocumentTemplates.FindAsync(id);
				if (template == null)
				{
					return NotFound(new { message = "Không tìm thấy template" });
				}

				var renderedHtml = await _templateRenderService.RenderTemplateWithObjectAsync(id, data);

				_logger.LogInformation(
					"Successfully rendered template ID: {TemplateId} with object data",
					id
				);

				return Content(renderedHtml, "text/html", System.Text.Encoding.UTF8);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error rendering template with object data, ID: {TemplateId}", id);
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi server khi render template",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// Render template by code with structured object data
		/// POST: api/DocumentTemplates/render-with-object-by-code/{code}
		/// </summary>
		[HttpPost("render-with-object-by-code/{code}")]
		public async Task<IActionResult> RenderTemplateWithObjectByCode(string code, [FromBody] object data)
		{
			try
			{
				var template = await _context.DocumentTemplates
					.FirstOrDefaultAsync(t => t.Code == code && t.IsActive);

				if (template == null)
				{
					return NotFound(new { message = $"Template với code '{code}' không tồn tại" });
				}

				var renderedHtml = await _templateRenderService.RenderTemplateWithObjectByCodeAsync(code, data);

				_logger.LogInformation(
					"Successfully rendered template Code: {Code} with object data",
					code
				);

				return Content(renderedHtml, "text/html", System.Text.Encoding.UTF8);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error rendering template with object data, Code: {Code}", code);
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi server khi render template",
					error = ex.Message
				});
			}
		}
	}

	// DTO classes for request bodies
	public class ExtractPlaceholdersRequest
	{
		public string HtmlContent { get; set; } = string.Empty;
	}

	public class ValidatePlaceholdersRequest
	{
		public List<string> Placeholders { get; set; } = new List<string>();
		public string? TemplateType { get; set; }
	}
}
