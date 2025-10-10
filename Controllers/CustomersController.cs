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

        [HttpGet]
		public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            return await _context.Customers.ToListAsync();
		}

		[HttpGet("active")]
		public async Task<ActionResult<IEnumerable<Customer>>> GetActiveCustomers()
		{
			return await _context.Customers.Where(c => c.IsActive).ToListAsync();
		}

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

		[HttpPost]
		public async Task<ActionResult<Customer>> CreateCustomer(Customer customer)
		{
			// Ki?m tra model validation
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			// Validation cho CustomerType
			if (string.IsNullOrWhiteSpace(customer.CustomerType))
			{
				return BadRequest("Lo?i khách hàng là b?t bu?c");
			}

			if (customer.CustomerType != "individual" && customer.CustomerType != "company")
			{
				return BadRequest("Lo?i khách hàng ph?i là 'individual' ho?c 'company'");
			}

			// Validation theo lo?i khách hàng
			if (customer.CustomerType == "individual")
			{
				if (string.IsNullOrWhiteSpace(customer.Name))
				{
					return BadRequest("Tên là b?t bu?c cho khách hàng cá nhân");
				}
				if (string.IsNullOrWhiteSpace(customer.Email))
				{
					return BadRequest("Email là b?t bu?c cho khách hàng cá nhân");
				}
				if (string.IsNullOrWhiteSpace(customer.PhoneNumber))
				{
					return BadRequest("S? di?n tho?i là b?t bu?c cho khách hàng cá nhân");
				}
			}
			else if (customer.CustomerType == "company")
			{
				if (string.IsNullOrWhiteSpace(customer.CompanyName))
				{
					return BadRequest("Tên công ty là b?t bu?c");
				}
				if (string.IsNullOrWhiteSpace(customer.RepresentativeName))
				{
					return BadRequest("Tên ngu?i d?i di?n là b?t bu?c");
				}
				if (string.IsNullOrWhiteSpace(customer.RepresentativeEmail))
				{
					return BadRequest("Email ngu?i d?i di?n là b?t bu?c");
				}
				if (string.IsNullOrWhiteSpace(customer.RepresentativePhone))
				{
					return BadRequest("S? di?n tho?i ngu?i d?i di?n là b?t bu?c");
				}
			}

			// Ki?m tra email unique (individual)
			if (!string.IsNullOrWhiteSpace(customer.Email))
			{
				var existingCustomer = await _context.Customers
					.FirstOrDefaultAsync(c => c.Email == customer.Email);
				if (existingCustomer != null)
				{
					return BadRequest("Email dã t?n t?i");
				}
			}

			// Ki?m tra email unique (representative)
			if (!string.IsNullOrWhiteSpace(customer.RepresentativeEmail))
			{
				var existingCustomer = await _context.Customers
					.FirstOrDefaultAsync(c => c.RepresentativeEmail == customer.RepresentativeEmail);
				if (existingCustomer != null)
				{
					return BadRequest("Email ngu?i d?i di?n dã t?n t?i");
				}
			}

			// Fix PostgreSQL DateTime UTC issue
			customer.CreatedAt = DateTime.UtcNow;
			
			// Convert nullable DateTime fields to UTC if they have values
			if (customer.BirthDate.HasValue)
			{
				if (customer.BirthDate.Value.Kind == DateTimeKind.Unspecified)
				{
					customer.BirthDate = DateTime.SpecifyKind(customer.BirthDate.Value, DateTimeKind.Utc);
				}
				else if (customer.BirthDate.Value.Kind == DateTimeKind.Local)
				{
					customer.BirthDate = customer.BirthDate.Value.ToUniversalTime();
				}
			}
			
			if (customer.EstablishedDate.HasValue)
			{
				if (customer.EstablishedDate.Value.Kind == DateTimeKind.Unspecified)
				{
					customer.EstablishedDate = DateTime.SpecifyKind(customer.EstablishedDate.Value, DateTimeKind.Utc);
				}
				else if (customer.EstablishedDate.Value.Kind == DateTimeKind.Local)
				{
					customer.EstablishedDate = customer.EstablishedDate.Value.ToUniversalTime();
				}
			}
			
			_context.Customers.Add(customer);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateCustomer(int id, Customer customer)
		{
			if (id != customer.Id)
			{
				return BadRequest();
			}

			// Ki?m tra model validation
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			// Validation cho CustomerType
			if (string.IsNullOrWhiteSpace(customer.CustomerType))
			{
				return BadRequest("Lo?i khách hàng là b?t bu?c");
			}

			if (customer.CustomerType != "individual" && customer.CustomerType != "company")
			{
				return BadRequest("Lo?i khách hàng ph?i là 'individual' ho?c 'company'");
			}

			// Validation theo lo?i khách hàng
			if (customer.CustomerType == "individual")
			{
				if (string.IsNullOrWhiteSpace(customer.Name))
				{
					return BadRequest("Tên là b?t bu?c cho khách hàng cá nhân");
				}
				if (string.IsNullOrWhiteSpace(customer.Email))
				{
					return BadRequest("Email là b?t bu?c cho khách hàng cá nhân");
				}
				if (string.IsNullOrWhiteSpace(customer.PhoneNumber))
				{
					return BadRequest("S? di?n tho?i là b?t bu?c cho khách hàng cá nhân");
				}
			}
			else if (customer.CustomerType == "company")
			{
				if (string.IsNullOrWhiteSpace(customer.CompanyName))
				{
					return BadRequest("Tên công ty là b?t bu?c");
				}
				if (string.IsNullOrWhiteSpace(customer.RepresentativeName))
				{
					return BadRequest("Tên ngu?i d?i di?n là b?t bu?c");
				}
				if (string.IsNullOrWhiteSpace(customer.RepresentativeEmail))
				{
					return BadRequest("Email ngu?i d?i di?n là b?t bu?c");
				}
				if (string.IsNullOrWhiteSpace(customer.RepresentativePhone))
				{
					return BadRequest("S? di?n tho?i ngu?i d?i di?n là b?t bu?c");
				}
			}

			// Ki?m tra email unique (individual - tr? customer hi?n t?i)
			if (!string.IsNullOrWhiteSpace(customer.Email))
			{
				var existingCustomer = await _context.Customers
					.FirstOrDefaultAsync(c => c.Email == customer.Email && c.Id != id);
				if (existingCustomer != null)
				{
					return BadRequest("Email dã t?n t?i");
				}
			}

			// Ki?m tra email unique (representative - tr? customer hi?n t?i)
			if (!string.IsNullOrWhiteSpace(customer.RepresentativeEmail))
			{
				var existingCustomer = await _context.Customers
					.FirstOrDefaultAsync(c => c.RepresentativeEmail == customer.RepresentativeEmail && c.Id != id);
				if (existingCustomer != null)
				{
					return BadRequest("Email ngu?i d?i di?n dã t?n t?i");
				}
			}

			// Fix PostgreSQL DateTime UTC issue
			customer.UpdatedAt = DateTime.UtcNow;
			
			// Convert nullable DateTime fields to UTC if they have values
			if (customer.BirthDate.HasValue)
			{
				if (customer.BirthDate.Value.Kind == DateTimeKind.Unspecified)
				{
					customer.BirthDate = DateTime.SpecifyKind(customer.BirthDate.Value, DateTimeKind.Utc);
				}
				else if (customer.BirthDate.Value.Kind == DateTimeKind.Local)
				{
					customer.BirthDate = customer.BirthDate.Value.ToUniversalTime();
				}
			}
			
			if (customer.EstablishedDate.HasValue)
			{
				if (customer.EstablishedDate.Value.Kind == DateTimeKind.Unspecified)
				{
					customer.EstablishedDate = DateTime.SpecifyKind(customer.EstablishedDate.Value, DateTimeKind.Utc);
				}
				else if (customer.EstablishedDate.Value.Kind == DateTimeKind.Local)
				{
					customer.EstablishedDate = customer.EstablishedDate.Value.ToUniversalTime();
				}
			}
			
			_context.Entry(customer).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!CustomerExists(id))
				{
					return NotFound();
				}
				else
				{
					throw;
				}
			}

			return NoContent();
		}

		[HttpPatch("{id}/toggle-status")]
		public async Task<IActionResult> ToggleCustomerStatus(int id)
		{
			var customer = await _context.Customers.FindAsync(id);
			if (customer == null)
			{
				return NotFound();
			}

			customer.IsActive = !customer.IsActive;
			customer.UpdatedAt = DateTime.UtcNow;
			await _context.SaveChangesAsync();

			return Ok(new { id = customer.Id, isActive = customer.IsActive });
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteCustomer(int id)
		{
			var customer = await _context.Customers.FindAsync(id);
			if (customer == null)
			{
				return NotFound();
			}

			_context.Customers.Remove(customer);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		// L?y khách hàng theo lo?i
		[HttpGet("by-type/{customerType}")]
		public async Task<ActionResult<IEnumerable<Customer>>> GetCustomersByType(string customerType)
		{
			return await _context.Customers
				.Where(c => c.CustomerType == customerType)
				.ToListAsync();
		}						

		// Th?ng kê theo lo?i khách hàng
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

		// L?y khách hàng cá nhân
		[HttpGet("individuals")]
		public async Task<ActionResult<IEnumerable<Customer>>> GetIndividualCustomers()
		{
			return await _context.Customers
				.Where(c => c.CustomerType == "individual")
				.ToListAsync();
		}

		// L?y khách hàng công ty
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
	}
}
