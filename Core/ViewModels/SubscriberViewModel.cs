namespace Boookify.Web.Core.ViewModels
{
	public class SubscriberViewModel
	{
        public int Id { get; set; }
        public string? Key { get; set; }
		public string ImageUrl { get; set; } = null!;

		public string FullName { get; set; } = null!;

		public string Email { get; set; } = null!;

		public string MobileNumber { get; set; } = null!;

		public string NationalId { get; set; } = null!;

		public DateTime DateOfBirth { get; set; }

		public string Governorate{ get; set; } = null!;
		public string Area { get; set; } = null!;
		public string Address { get; set; } = null!;

		public DateTime CreatedAt { get; set; }

		public bool IsBlackListed { get; set; }

		public IEnumerable<SubscribtionViewModel> Subscribtions = new List<SubscribtionViewModel>();


    }
}
