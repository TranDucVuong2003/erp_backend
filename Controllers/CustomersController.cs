using erp_backend.Data;
using erp_backend.Models;
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

		public CustomersController(ApplicationDbContext context)
		{
			_context = context;
		}

		// Lấy danh sách tất cả khách hàng
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
		{
			return await _context.Customers.ToListAsync();
		}

		// Lấy khách hàng đang hoạt động
		[HttpGet("active")]
		public async Task<ActionResult<IEnumerable<Customer>>> GetActiveCustomers()
		{
			return await _context.Customers.Where(c => c.IsActive).ToListAsync();
		}

		// Lấy khách hàng theo ID
		[HttpGet("{id}")]
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
		public async Task<IActionResult> UpdateCustomer(int id, Customer customer)
		{
			if (id != customer.Id)
				return BadRequest("ID không khớp với dữ liệu khách hàng");

			// Lấy customer hiện tại từ DB
			var existing = await _context.Customers.FindAsync(id);
			if (existing == null)
				return NotFound();

			// Chỉ validate CustomerType nếu có giá trị mới
			if (!string.IsNullOrWhiteSpace(customer.CustomerType))
			{
				if (customer.CustomerType != "individual" && customer.CustomerType != "company")
					return BadRequest("Loại khách hàng phải là 'individual' hoặc 'company'");
				
				existing.CustomerType = customer.CustomerType;
			}

			// Cập nhật các trường Individual fields chỉ khi có giá trị
			if (!string.IsNullOrWhiteSpace(customer.Name))
				existing.Name = customer.Name;
			
			if (!string.IsNullOrWhiteSpace(customer.Email))
			{
				// Kiểm tra email trùng
				var duplicateEmail = await _context.Customers
					.FirstOrDefaultAsync(c => c.Email == customer.Email && c.Id != id);
				if (duplicateEmail != null)
					return BadRequest("Email đã tồn tại");
				existing.Email = customer.Email;
			}
			
			if (!string.IsNullOrWhiteSpace(customer.PhoneNumber))
				existing.PhoneNumber = customer.PhoneNumber;
			
			if (!string.IsNullOrWhiteSpace(customer.Address))
				existing.Address = customer.Address;
			
			if (customer.BirthDate.HasValue)
				existing.BirthDate = ToUtc(customer.BirthDate.Value);
			
			if (!string.IsNullOrWhiteSpace(customer.IdNumber))
				existing.IdNumber = customer.IdNumber;
			
			if (!string.IsNullOrWhiteSpace(customer.Domain))
				existing.Domain = customer.Domain;

			// Cập nhật các trường Company fields chỉ khi có giá trị
			if (!string.IsNullOrWhiteSpace(customer.CompanyName))
				existing.CompanyName = customer.CompanyName;
			
			if (!string.IsNullOrWhiteSpace(customer.CompanyAddress))
				existing.CompanyAddress = customer.CompanyAddress;
			
			if (customer.EstablishedDate.HasValue)
				existing.EstablishedDate = ToUtc(customer.EstablishedDate.Value);
			
			if (!string.IsNullOrWhiteSpace(customer.TaxCode))
				existing.TaxCode = customer.TaxCode;
			
			if (!string.IsNullOrWhiteSpace(customer.CompanyDomain))
				existing.CompanyDomain = customer.CompanyDomain;

			// Cập nhật các trường Representative info chỉ khi có giá trị
			if (!string.IsNullOrWhiteSpace(customer.RepresentativeName))
				existing.RepresentativeName = customer.RepresentativeName;
			
			if (!string.IsNullOrWhiteSpace(customer.RepresentativePosition))
				existing.RepresentativePosition = customer.RepresentativePosition;
			
			if (!string.IsNullOrWhiteSpace(customer.RepresentativeIdNumber))
				existing.RepresentativeIdNumber = customer.RepresentativeIdNumber;
			
			if (!string.IsNullOrWhiteSpace(customer.RepresentativePhone))
				existing.RepresentativePhone = customer.RepresentativePhone;
			
			if (!string.IsNullOrWhiteSpace(customer.RepresentativeEmail))
			{
				// Kiểm tra email người đại diện trùng
				var duplicateRepEmail = await _context.Customers
					.FirstOrDefaultAsync(c => c.RepresentativeEmail == customer.RepresentativeEmail && c.Id != id);
				if (duplicateRepEmail != null)
					return BadRequest("Email người đại diện đã tồn tại");
				existing.RepresentativeEmail = customer.RepresentativeEmail;
			}

			// Cập nhật các trường Technical contact chỉ khi có giá trị
			if (!string.IsNullOrWhiteSpace(customer.TechContactName))
				existing.TechContactName = customer.TechContactName;
			
			if (!string.IsNullOrWhiteSpace(customer.TechContactPhone))
				existing.TechContactPhone = customer.TechContactPhone;
			
			if (!string.IsNullOrWhiteSpace(customer.TechContactEmail))
				existing.TechContactEmail = customer.TechContactEmail;

			// Cập nhật các trường khác chỉ khi có giá trị
			if (!string.IsNullOrWhiteSpace(customer.Status))
				existing.Status = customer.Status;
			
			if (!string.IsNullOrWhiteSpace(customer.Notes))
				existing.Notes = customer.Notes;

			// Cập nhật IsActive (boolean luôn có giá trị, kiểm tra xem có khác với giá trị hiện tại không)
			existing.IsActive = customer.IsActive;

			// Validation sau khi cập nhật dựa trên CustomerType hiện tại
			if (existing.CustomerType == "individual")
			{
				if (string.IsNullOrWhiteSpace(existing.Name))
					return BadRequest("Tên là bắt buộc cho khách hàng cá nhân");
				if (string.IsNullOrWhiteSpace(existing.Email))
					return BadRequest("Email là bắt buộc cho khách hàng cá nhân");
				if (string.IsNullOrWhiteSpace(existing.PhoneNumber))
					return BadRequest("Số điện thoại là bắt buộc cho khách hàng cá nhân");
			}
			else if (existing.CustomerType == "company")
			{
				if (string.IsNullOrWhiteSpace(existing.CompanyName))
					return BadRequest("Tên công ty là bắt buộc");
				if (string.IsNullOrWhiteSpace(existing.RepresentativeName))
					return BadRequest("Tên người đại diện là bắt buộc");
				if (string.IsNullOrWhiteSpace(existing.RepresentativeEmail))
					return BadRequest("Email người đại diện là bắt buộc");
				if (string.IsNullOrWhiteSpace(existing.RepresentativePhone))
					return BadRequest("Số điện thoại người đại diện là bắt buộc");
			}

			// Cập nhật thời gian
			existing.UpdatedAt = DateTime.UtcNow;

			await _context.SaveChangesAsync();

			// Trả về object vừa được update (HTTP 200 OK)
			return Ok(existing);
		}


		// Cập nhật một phần thông tin khách hàng (PATCH)
		[HttpPatch("{id}")]
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
		public async Task<IActionResult> DeleteCustomer(int id)
		{
			var customer = await _context.Customers.FindAsync(id);
			if (customer == null)
				return NotFound();

			_context.Customers.Remove(customer);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		// Lấy khách hàng theo loại (individual / company)
		[HttpGet("by-type/{customerType}")]
		public async Task<ActionResult<IEnumerable<Customer>>> GetCustomersByType(string customerType)
		{
			return await _context.Customers
				.Where(c => c.CustomerType == customerType)
				.ToListAsync();
		}

		// Thống kê khách hàng theo loại
		[HttpGet("type-statistics")]
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
		public async Task<ActionResult<IEnumerable<Customer>>> GetIndividualCustomers()
		{
			return await _context.Customers
				.Where(c => c.CustomerType == "individual")
				.ToListAsync();
		}

		// Lấy danh sách khách hàng công ty
		[HttpGet("companies")]
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
