using erp_backend.Data;
using erp_backend.Models;
using erp_backend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace erp_backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class CustomersController : ControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogger<CustomersController> _logger;

		public CustomersController(ApplicationDbContext context, ILogger<CustomersController> logger)
		{
			_context = context;
			_logger = logger;
		}

		// Lấy danh sách tất cả khách hàng
		[HttpGet]
		[Authorize]
		public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
		{
			return await _context.Customers.ToListAsync();
		}

		// Lấy khách hàng đang hoạt động
		[HttpGet("active")]
		[Authorize]
		public async Task<ActionResult<IEnumerable<Customer>>> GetActiveCustomers()
		{
			return await _context.Customers.Where(c => c.IsActive).ToListAsync();
		}

		// Lấy khách hàng theo ID
		[HttpGet("{id}")]
		[Authorize]
		public async Task<ActionResult<Customer>> GetCustomer(int id)
		{
			var customer = await _context.Customers.FindAsync(id);

			if (customer == null)
			{
				return NotFound();
			}

			return customer;
		}

		// Tạo khách hàng mới
		[HttpPost]
		[Authorize]
		public async Task<ActionResult<Customer>> CreateCustomer(Customer customer)
		{
			// Kiểm tra model validation
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			// Validation cho CustomerType
			if (string.IsNullOrWhiteSpace(customer.CustomerType))
			{
				return BadRequest("Loại khách hàng là bắt buộc");
			}

			if (customer.CustomerType != "individual" && customer.CustomerType != "company")
			{
				return BadRequest("Loại khách hàng phải là 'individual' hoặc 'company'");
			}

			// Validation theo loại khách hàng
			if (customer.CustomerType == "individual")
			{
				if (string.IsNullOrWhiteSpace(customer.Name))
					return BadRequest("Tên là bắt buộc cho khách hàng cá nhân");
				if (string.IsNullOrWhiteSpace(customer.Email))
					return BadRequest("Email là bắt buộc cho khách hàng cá nhân");
				if (string.IsNullOrWhiteSpace(customer.PhoneNumber))
					return BadRequest("Số điện thoại là bắt buộc cho khách hàng cá nhân");
			}
			else if (customer.CustomerType == "company")
			{
				if (string.IsNullOrWhiteSpace(customer.CompanyName))
					return BadRequest("Tên công ty là bắt buộc");
				if (string.IsNullOrWhiteSpace(customer.RepresentativeName))
					return BadRequest("Tên người đại diện là bắt buộc");
				if (string.IsNullOrWhiteSpace(customer.RepresentativeEmail))
					return BadRequest("Email người đại diện là bắt buộc");
				if (string.IsNullOrWhiteSpace(customer.RepresentativePhone))
					return BadRequest("Số điện thoại người đại diện là bắt buộc");
			}

			// Kiểm tra email trùng (individual)
			if (!string.IsNullOrWhiteSpace(customer.Email))
			{
				var existingCustomer = await _context.Customers
					.FirstOrDefaultAsync(c => c.Email == customer.Email);
				if (existingCustomer != null)
					return BadRequest("Email đã tồn tại");
			}

			// Kiểm tra email trùng (representative)
			if (!string.IsNullOrWhiteSpace(customer.RepresentativeEmail))
			{
				var existingCustomer = await _context.Customers
					.FirstOrDefaultAsync(c => c.RepresentativeEmail == customer.RepresentativeEmail);
				if (existingCustomer != null)
					return BadRequest("Email người đại diện đã tồn tại");
			}

			// Gán thời gian tạo UTC
			customer.CreatedAt = DateTime.UtcNow;

			// Fix DateTime UTC cho BirthDate và EstablishedDate
			if (customer.BirthDate.HasValue)
				customer.BirthDate = ToUtc(customer.BirthDate.Value);

			if (customer.EstablishedDate.HasValue)
				customer.EstablishedDate = ToUtc(customer.EstablishedDate.Value);

			_context.Customers.Add(customer);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
		}

		// Cập nhật khách hàng
		[HttpPut("{id}")]
		[Authorize]
		public async Task<ActionResult<UpdateCustomerResponse>> UpdateCustomer(int id, [FromBody] Dictionary<string, object?> updateData)
		{
			try
			{
				// Kiểm tra xem khách hàng có tồn tại không
				var existingCustomer = await _context.Customers.FindAsync(id);
				if (existingCustomer == null)
				{
					return NotFound(new { message = "Không tìm thấy khách hàng" });
				}

				// Cập nhật từng trường nếu có trong request
				foreach (var kvp in updateData)
				{
					var propertyName = kvp.Key;
					var value = kvp.Value;

					switch (propertyName.ToLower())
					{
						case "customertype":
							if (!string.IsNullOrWhiteSpace(value?.ToString()))
							{
								var customerType = value.ToString();
								if (customerType != "individual" && customerType != "company")
								{
									return BadRequest(new { message = "Loại khách hàng phải là 'individual' hoặc 'company'" });
								}
								existingCustomer.CustomerType = customerType;
							}
							break;

						case "name":
							if (!string.IsNullOrWhiteSpace(value?.ToString()))
							{
								existingCustomer.Name = value.ToString();
							}
							break;

						case "email":
							if (!string.IsNullOrWhiteSpace(value?.ToString()))
							{
								var email = value.ToString();
								if (email.Length > 150)
								{
									return BadRequest(new { message = "Email không được vượt quá 150 ký tự" });
								}
								if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
								{
									return BadRequest(new { message = "Định dạng email không hợp lệ" });
								}
								var emailExists = await _context.Customers.AnyAsync(c => c.Email == email && c.Id != id);
								if (emailExists)
								{
									return BadRequest(new { message = "Email đã được sử dụng bởi khách hàng khác" });
								}
								existingCustomer.Email = email;
							}
							break;

						case "phonenumber":
							if (!string.IsNullOrWhiteSpace(value?.ToString()))
							{
								var phoneNumber = value.ToString();
								if (phoneNumber.Length > 20)
								{
									return BadRequest(new { message = "Số điện thoại không được vượt quá 20 ký tự" });
								}
								existingCustomer.PhoneNumber = phoneNumber;
							}
							break;

						case "address":
							if (!string.IsNullOrWhiteSpace(value?.ToString()))
							{
								var address = value.ToString();
								if (address.Length > 500)
								{
									return BadRequest(new { message = "Địa chỉ không được vượt quá 500 ký tự" });
								}
								existingCustomer.Address = address;
							}
							break;

						case "birthdate":
							if (value != null && value is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.String)
							{
								if (DateTime.TryParse(jsonElement.GetString(), out DateTime birthDate))
								{
									existingCustomer.BirthDate = ToUtc(birthDate);
								}
								else
								{
									return BadRequest(new { message = "Định dạng ngày sinh không hợp lệ" });
								}
							}
							break;

						case "idnumber":
							if (!string.IsNullOrWhiteSpace(value?.ToString()))
							{
								existingCustomer.IdNumber = value.ToString();
							}
							break;

						case "domain":
							if (!string.IsNullOrWhiteSpace(value?.ToString()))
							{
								existingCustomer.Domain = value.ToString();
							}
							break;

						case "companyname":
							if (!string.IsNullOrWhiteSpace(value?.ToString()))
							{
								existingCustomer.CompanyName = value.ToString();
							}
							break;

						case "companyaddress":
							if (!string.IsNullOrWhiteSpace(value?.ToString()))
							{
								var companyAddress = value.ToString();
								if (companyAddress.Length > 500)
								{
									return BadRequest(new { message = "Địa chỉ công ty không được vượt quá 500 ký tự" });
								}
								existingCustomer.CompanyAddress = companyAddress;
							}
							break;

						case "establisheddate":
							if (value != null && value is System.Text.Json.JsonElement jsonElement2 && jsonElement2.ValueKind == System.Text.Json.JsonValueKind.String)
							{
								if (DateTime.TryParse(jsonElement2.GetString(), out DateTime establishedDate))
								{
									existingCustomer.EstablishedDate = ToUtc(establishedDate);
								}
								else
								{
									return BadRequest(new { message = "Định dạng ngày thành lập không hợp lệ" });
								}
							}
							break;

						case "taxcode":
							if (!string.IsNullOrWhiteSpace(value?.ToString()))
							{
								existingCustomer.TaxCode = value.ToString();
							}
							break;

						case "companydomain":
							if (!string.IsNullOrWhiteSpace(value?.ToString()))
							{
								existingCustomer.CompanyDomain = value.ToString();
							}
							break;

						case "representativename":
							if (!string.IsNullOrWhiteSpace(value?.ToString()))
							{
								existingCustomer.RepresentativeName = value.ToString();
							}
							break;

						case "representativeposition":
							if (!string.IsNullOrWhiteSpace(value?.ToString()))
							{
								existingCustomer.RepresentativePosition = value.ToString();
							}
							break;

						case "representativeidnumber":
							if (!string.IsNullOrWhiteSpace(value?.ToString()))
							{
								existingCustomer.RepresentativeIdNumber = value.ToString();
							}
							break;

						case "representativephone":
							if (!string.IsNullOrWhiteSpace(value?.ToString()))
							{
								var representativePhone = value.ToString();
								if (representativePhone.Length > 20)
								{
									return BadRequest(new { message = "Số điện thoại người đại diện không được vượt quá 20 ký tự" });
								}
								existingCustomer.RepresentativePhone = representativePhone;
							}
							break;

						case "representativeemail":
							if (!string.IsNullOrWhiteSpace(value?.ToString()))
							{
								var repEmail = value.ToString();
								if (repEmail.Length > 150)
								{
									return BadRequest(new { message = "Email người đại diện không được vượt quá 150 ký tự" });
								}
								if (!System.Text.RegularExpressions.Regex.IsMatch(repEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
								{
									return BadRequest(new { message = "Định dạng email người đại diện không hợp lệ" });
								}
								var repEmailExists = await _context.Customers.AnyAsync(c => c.RepresentativeEmail == repEmail && c.Id != id);
								if (repEmailExists)
								{
									return BadRequest(new { message = "Email người đại diện đã được sử dụng bởi khách hàng khác" });
								}
								existingCustomer.RepresentativeEmail = repEmail;
							}
							break;

						case "techcontactname":
							if (!string.IsNullOrWhiteSpace(value?.ToString()))
							{
								existingCustomer.TechContactName = value.ToString();
							}
							break;

						case "techcontactphone":
							if (!string.IsNullOrWhiteSpace(value?.ToString()))
							{
								var techContactPhone = value.ToString();
								if (techContactPhone.Length > 20)
								{
									return BadRequest(new { message = "Số điện thoại liên hệ kỹ thuật không được vượt quá 20 ký tự" });
								}
								existingCustomer.TechContactPhone = techContactPhone;
							}
							break;

						case "techcontactemail":
							if (!string.IsNullOrWhiteSpace(value?.ToString()))
							{
								var techContactEmail = value.ToString();
								if (techContactEmail.Length > 150)
								{
									return BadRequest(new { message = "Email liên hệ kỹ thuật không được vượt quá 150 ký tự" });
								}
								if (!System.Text.RegularExpressions.Regex.IsMatch(techContactEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
								{
									return BadRequest(new { message = "Định dạng email liên hệ kỹ thuật không hợp lệ" });
								}
								existingCustomer.TechContactEmail = techContactEmail;
							}
							break;

						case "status":
							if (!string.IsNullOrWhiteSpace(value?.ToString()))
							{
								existingCustomer.Status = value.ToString();
							}
							break;

						case "notes":
							existingCustomer.Notes = value?.ToString(); // Cho phép notes là null hoặc rỗng
							break;

						case "isactive":
							if (value != null && bool.TryParse(value.ToString(), out bool isActive))
							{
								existingCustomer.IsActive = isActive;
							}
							break;

						case "id":
						case "createdat":
						case "updatedat":
							// Bỏ qua các trường này
							break;

						default:
							// Bỏ qua các trường không được hỗ trợ
							break;
					}
				}

				// Validation sau khi cập nhật dựa trên CustomerType hiện tại
				if (existingCustomer.CustomerType == "individual")
				{
					if (string.IsNullOrWhiteSpace(existingCustomer.Name))
						return BadRequest(new { message = "Tên là bắt buộc cho khách hàng cá nhân" });
					if (string.IsNullOrWhiteSpace(existingCustomer.Email))
						return BadRequest(new { message = "Email là bắt buộc cho khách hàng cá nhân" });
					if (string.IsNullOrWhiteSpace(existingCustomer.PhoneNumber))
						return BadRequest(new { message = "Số điện thoại là bắt buộc cho khách hàng cá nhân" });
				}
				else if (existingCustomer.CustomerType == "company")
				{
					if (string.IsNullOrWhiteSpace(existingCustomer.CompanyName))
						return BadRequest(new { message = "Tên công ty là bắt buộc" });
					if (string.IsNullOrWhiteSpace(existingCustomer.RepresentativeName))
						return BadRequest(new { message = "Tên người đại diện là bắt buộc" });
					if (string.IsNullOrWhiteSpace(existingCustomer.RepresentativeEmail))
						return BadRequest(new { message = "Email người đại diện là bắt buộc" });
					if (string.IsNullOrWhiteSpace(existingCustomer.RepresentativePhone))
						return BadRequest(new { message = "Số điện thoại người đại diện là bắt buộc" });
				}

				// Cập nhật thời gian
				existingCustomer.UpdatedAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();

				// Tạo response
				var response = new UpdateCustomerResponse
				{
					Message = "Cập nhật thông tin khách hàng thành công",
					Customer = new CustomerInfo
					{
						Id = existingCustomer.Id,
						Name = existingCustomer.Name,
						Email = existingCustomer.Email,
						PhoneNumber = existingCustomer.PhoneNumber,
						CompanyName = existingCustomer.CompanyName,
						RepresentativeName = existingCustomer.RepresentativeName,
						RepresentativeEmail = existingCustomer.RepresentativeEmail,
						CustomerType = existingCustomer.CustomerType,
						IsActive = existingCustomer.IsActive,
						Status = existingCustomer.Status
					},
					UpdatedAt = existingCustomer.UpdatedAt.Value
				};

				return Ok(response);
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!CustomerExists(id))
				{
					return NotFound(new { message = "Không tìm thấy khách hàng" });
				}
				else
				{
					throw;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi cập nhật khách hàng với ID: {CustomerId}", id);
				return StatusCode(500, new { message = "Lỗi server khi cập nhật khách hàng", error = ex.Message });
			}
		}
		// Cập nhật một phần thông tin khách hàng (PATCH)
		[HttpPatch("{id}")]
		[Authorize]
		public async Task<IActionResult> PartialUpdateCustomer(int id, [FromBody] Dictionary<string, object> updateData)
		{
			var existing = await _context.Customers.FindAsync(id);
			if (existing == null)
				return NotFound();

			try
			{
				// Convert dynamic object to dictionary để dễ xử lý
				var updates = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(updateData.ToString());
				
				foreach (var update in updates)
				{
					var propertyName = update.Key;
					var value = update.Value?.ToString();

					switch (propertyName.ToLower())
					{
						case "name":
							if (!string.IsNullOrWhiteSpace(value))
								existing.Name = value;
							break;
						case "email":
							if (!string.IsNullOrWhiteSpace(value))
							{
								var emailToCheck = value;
								var duplicateEmail = await _context.Customers
									.FirstOrDefaultAsync(c => c.Email == emailToCheck && c.Id != id);
								if (duplicateEmail != null)
									return BadRequest("Email đã tồn tại");
								existing.Email = value;
							}
							break;
						case "phonenumber":
							if (!string.IsNullOrWhiteSpace(value))
								existing.PhoneNumber = value;
							break;
						case "address":
							if (!string.IsNullOrWhiteSpace(value))
								existing.Address = value;
							break;
						case "customertype":
							if (!string.IsNullOrWhiteSpace(value) && (value == "individual" || value == "company"))
								existing.CustomerType = value;
							break;
						case "companyname":
							if (!string.IsNullOrWhiteSpace(value))
								existing.CompanyName = value;
							break;
						case "representativename":
							if (!string.IsNullOrWhiteSpace(value))
								existing.RepresentativeName = value;
							break;
						case "representativeemail":
							if (!string.IsNullOrWhiteSpace(value))
							{
								var repEmailToCheck = value;
								var duplicateRepEmail = await _context.Customers
									.FirstOrDefaultAsync(c => c.RepresentativeEmail == repEmailToCheck && c.Id != id);
								if (duplicateRepEmail != null)
									return BadRequest("Email người đại diện đã tồn tại");
								existing.RepresentativeEmail = value;
							}
							break;
						case "representativephone":
							if (!string.IsNullOrWhiteSpace(value))
								existing.RepresentativePhone = value;
							break;
						case "isactive":
							if (bool.TryParse(value, out bool isActive))
								existing.IsActive = isActive;
							break;
						case "status":
							if (!string.IsNullOrWhiteSpace(value))
								existing.Status = value;
							break;
						case "notes":
							existing.Notes = value; // Cho phép cập nhật notes thành null/empty
							break;
					}
				}

				existing.UpdatedAt = DateTime.UtcNow;
				await _context.SaveChangesAsync();

				return Ok(existing);
			}
			catch (Exception ex)
			{
				return BadRequest($"Lỗi cập nhật: {ex.Message}");
			}
		}

		// Bật/tắt trạng thái hoạt động của khách hàng
		[HttpPatch("{id}/toggle-status")]
		[Authorize]
		public async Task<IActionResult> ToggleCustomerStatus(int id)
		{
			var customer = await _context.Customers.FindAsync(id);
			if (customer == null)
				return NotFound();

			customer.IsActive = !customer.IsActive;
			customer.UpdatedAt = DateTime.UtcNow;
			await _context.SaveChangesAsync();

			return Ok(new { id = customer.Id, isActive = customer.IsActive });
		}

		// Xóa khách hàng
		[HttpDelete("{id}")]
		[Authorize]
		public async Task<ActionResult<DeleteCustomerResponse>> DeleteCustomer(int id)
		{
			try
			{
				var customer = await _context.Customers.FindAsync(id);
				if (customer == null)
				{
					return NotFound(new { message = "Không tìm thấy khách hàng" });
				}

				// Lưu thông tin customer trước khi xóa để trả về trong response
				var deletedCustomerInfo = new CustomerInfo
				{
					Id = customer.Id,
					Name = customer.Name,
					Email = customer.Email,
					PhoneNumber = customer.PhoneNumber,
					CompanyName = customer.CompanyName,
					RepresentativeName = customer.RepresentativeName,
					RepresentativeEmail = customer.RepresentativeEmail,
					CustomerType = customer.CustomerType,
					IsActive = customer.IsActive,
					Status = customer.Status
				};

				_context.Customers.Remove(customer);
				await _context.SaveChangesAsync();

				// Tạo response
				var response = new DeleteCustomerResponse
				{
					Message = "Xóa khách hàng thành công",
					DeletedCustomer = deletedCustomerInfo,
					DeletedAt = DateTime.UtcNow
				};

				return Ok(response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi xóa customer với ID: {CustomerId}", id);
				return StatusCode(500, new { message = "Lỗi server khi xóa khách hàng", error = ex.Message });
			}
		}

		// Lấy khách hàng theo loại (individual / company)
		[HttpGet("by-type/{customerType}")]
		[Authorize]
		public async Task<ActionResult<IEnumerable<Customer>>> GetCustomersByType(string customerType)
		{
			return await _context.Customers
				.Where(c => c.CustomerType == customerType)
				.ToListAsync();
		}

		// Thống kê khách hàng theo loại
		[HttpGet("type-statistics")]
		[Authorize]
		public async Task<ActionResult<object>> GetTypeStatistics()
		{
			var statistics = await _context.Customers
				.GroupBy(c => c.CustomerType)
				.Select(g => new
				{
					CustomerType = g.Key,
					Count = g.Count(),
					ActiveCount = g.Count(c => c.IsActive)
				})
				.ToListAsync();

			return Ok(statistics);
		}

		// Lấy danh sách khách hàng cá nhân
		[HttpGet("individuals")]
		[Authorize]
		public async Task<ActionResult<IEnumerable<Customer>>> GetIndividualCustomers()
		{
			return await _context.Customers
				.Where(c => c.CustomerType == "individual")
				.ToListAsync();
		}

		// Lấy danh sách khách hàng công ty
		[HttpGet("companies")]
		[Authorize]
		public async Task<ActionResult<IEnumerable<Customer>>> GetCompanyCustomers()
		{
			return await _context.Customers
				.Where(c => c.CustomerType == "company")
				.ToListAsync();
		}

		private bool CustomerExists(int id)
		{
			return _context.Customers.Any(e => e.Id == id);
		}

		// Hàm hỗ trợ chuyển DateTime về UTC an toàn
		private DateTime ToUtc(DateTime dateTime)
		{
			if (dateTime.Kind == DateTimeKind.Unspecified)
				return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
			if (dateTime.Kind == DateTimeKind.Local)
				return dateTime.ToUniversalTime();
			return dateTime;
		}
	}
}
