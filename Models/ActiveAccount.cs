namespace erp_backend.Models
{
	public class ActiveAccount
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public bool FirstLogin { get; set; }

		public User User { get; set; }
	}
}
