using erp_backend.Data;
using erp_backend.Models;
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
		private readonly ILogger<DocumentTemplatesController> _logger;

		public DocumentTemplatesController(ApplicationDbContext context, ILogger<DocumentTemplatesController> logger)
		{
			_context = context;
			_logger = logger;
		}

		/// <summary>
		/// L?y danh sách t?t c? templates (có th? filter theo type)
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
					message = "L?i server khi l?y danh sách templates",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// L?y template theo ID
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
					return NotFound(new { message = "Không tìm th?y template" });
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
					message = "L?i server khi l?y template",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// L?y template theo code (unique)
		/// GET: api/DocumentTemplates/by-code/CONTRACT_DEFAULT
		/// </summary>
		[HttpGet("by-code/{code}")]
		public async Task<ActionResult<DocumentTemplate>> GetTemplateByCode(string code)
		{
			try
			{
				var template = await _context.DocumentTemplates
					.Include(t => t.CreatedByUser)
					.FirstOrDefaultAsync(t => t.Code == code && t.IsActive);

				if (template == null)
				{
					return NotFound(new { message = $"Template v?i code '{code}' không t?n t?i" });
				}

				return Ok(new
				{
					success = true,
					data = template
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting template by code: {Code}", code);
				return StatusCode(500, new
				{
					success = false,
					message = "L?i server khi l?y template",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// L?y RAW HTML content c?a template theo code (không bao g?i trong JSON)
		/// GET: api/DocumentTemplates/by-code/{code}/raw-html
		/// </summary>
		[HttpGet("by-code/{code}/raw-html")]
		public async Task<IActionResult> GetTemplateRawHtml(string code)
		{
			try
			{
				var template = await _context.DocumentTemplates
					.FirstOrDefaultAsync(t => t.Code == code && t.IsActive);

				if (template == null)
				{
					return NotFound(new { message = $"Template v?i code '{code}' kh?ng t?n t?i" });
				}

				// Tr? v? HTML tr?c ti?p, không JSON serialize
				return Content(template.HtmlContent, "text/html", System.Text.Encoding.UTF8);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting raw HTML template by code: {Code}", code);
				return StatusCode(500, "L?i server khi l?y template");
			}
		}

		/// <summary>
		/// L?y template m?c ??nh theo lo?i
		/// GET: api/DocumentTemplates/default/contract
		/// </summary>
		[HttpGet("default/{templateType}")]
		public async Task<ActionResult<DocumentTemplate>> GetDefaultTemplate(string templateType)
		{
			try
			{
				var template = await _context.DocumentTemplates
					.Include(t => t.CreatedByUser)
					.FirstOrDefaultAsync(t => t.TemplateType.ToLower() == templateType.ToLower() 
						&& t.IsDefault 
						&& t.IsActive);

				if (template == null)
				{
					return NotFound(new { message = $"Không tìm th?y template m?c ??nh cho lo?i '{templateType}'" });
				}

				return Ok(new
				{
					success = true,
					data = template
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting default template for type: {TemplateType}", templateType);
				return StatusCode(500, new
				{
					success = false,
					message = "L?i server khi l?y template m?c ??nh",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// T?o template m?i (ch? admin)
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

				// Ki?m tra code ?ã t?n t?i ch?a
				if (await _context.DocumentTemplates.AnyAsync(t => t.Code == template.Code))
				{
					return BadRequest(new { message = $"Code '{template.Code}' ?ã t?n t?i" });
				}

				// L?y UserId t? JWT token
				var userIdClaim = User.FindFirst("userid")?.Value;
				if (userIdClaim != null)
				{
					template.CreatedByUserId = int.Parse(userIdClaim);
				}

				// N?u @@t làm default, b? default c?a các template cùng lo?i khác
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

				// Reload v?i navigation property
				var savedTemplate = await _context.DocumentTemplates
					.Include(t => t.CreatedByUser)
					.FirstOrDefaultAsync(t => t.Id == template.Id);

				_logger.LogInformation("Created new template: {TemplateName} (Code: {Code}) by user {UserId}", 
					template.Name, template.Code, template.CreatedByUserId);

				return CreatedAtAction(nameof(GetTemplate), new { id = savedTemplate.Id }, new
				{
					success = true,
					message = "T?o template thành công",
					data = savedTemplate
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating template");
				return StatusCode(500, new
				{
					success = false,
					message = "L?i server khi t?o template",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// C?p nh?t template (ch? admin)
		/// PUT: api/DocumentTemplates/5
		/// </summary>
		[HttpPut("{id}")]

		public async Task<IActionResult> UpdateTemplate(int id, DocumentTemplate template)
		{
			try
			{
				if (id != template.Id)
				{
					return BadRequest(new { message = "ID không kh?p" });
				}

				var existing = await _context.DocumentTemplates.FindAsync(id);
				if (existing == null)
				{
					return NotFound(new { message = "Không tìm th?y template" });
				}

				// Ki?m tra code trùng (ngo?i tr? chính nó)
				if (await _context.DocumentTemplates.AnyAsync(t => t.Code == template.Code && t.Id != id))
				{
					return BadRequest(new { message = $"Code '{template.Code}' ?ã ???c s? d?ng b?i template khác" });
				}

				// N?u @@t làm default, b? default c?a các template cùng lo?i khác
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
				existing.AvailablePlaceholders = template.AvailablePlaceholders;
				existing.IsActive = template.IsActive;
				existing.IsDefault = template.IsDefault;
				existing.Version++; // T?ng version
				existing.UpdatedAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();

				_logger.LogInformation("Updated template ID: {TemplateId} to version {Version}", id, existing.Version);

				return Ok(new
				{
					success = true,
					message = "C?p nh?t template thành công",
					data = existing
				});
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!await _context.DocumentTemplates.AnyAsync(e => e.Id == id))
				{
					return NotFound(new { message = "Không tìm th?y template" });
				}
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating template with ID: {TemplateId}", id);
				return StatusCode(500, new
				{
					success = false,
					message = "L?i server khi c?p nh?t template",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// Xóa m?m template (ch? admin)
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
					return NotFound(new { message = "Không tìm th?y template" });
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
					message = "L?i server khi xóa template",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// ??t template làm m?c ??nh (ch? admin)
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
					return NotFound(new { message = "Không tìm th?y template" });
				}

				// B? default c?a các template cùng lo?i khác
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
					message = $"?ã ??t '{template.Name}' làm template m?c ??nh cho lo?i {template.TemplateType}"
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error setting template as default with ID: {TemplateId}", id);
				return StatusCode(500, new
				{
					success = false,
					message = "L?i server khi ??t template m?c ??nh",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// L?y danh sách các lo?i template có s?n
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
					message = "L?i server khi l?y danh sách lo?i template",
					error = ex.Message
				});
			}
		}

		/// <summary>
		/// ADMIN ONLY: Migrate t?t c? HTML templates t? wwwroot/Templates/ vào database
		/// POST: api/DocumentTemplates/migrate-from-files
		/// </summary>
		[HttpPost("migrate-from-files")]

		public async Task<IActionResult> MigrateTemplatesFromFiles([FromServices] ILoggerFactory loggerFactory)
		{
			try
			{
				var migratorLogger = loggerFactory.CreateLogger<erp_backend.Migrations.Scripts.MigrateTemplatesToDatabase>();
				var migrator = new erp_backend.Migrations.Scripts.MigrateTemplatesToDatabase(_context, migratorLogger);
				await migrator.MigrateAllTemplatesAsync();

				return Ok(new
				{
					success = true,
					message = "?ã migrate t?t c? templates vào database thành công"
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error migrating templates from files");
				return StatusCode(500, new
				{
					success = false,
					message = "L?i khi migrate templates",
					error = ex.Message
				});
			}
		}
	}
}
