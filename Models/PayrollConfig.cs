using System.ComponentModel.DataAnnotations;

namespace erp_backend.Models
{
	// Tên cũ: SystemConfig -> Tên mới: PayrollConfig
	public class PayrollConfig
	{
		[Key]
		public string Key { get; set; }
		public string Value { get; set; }
		public string Description { get; set; }
	}
}