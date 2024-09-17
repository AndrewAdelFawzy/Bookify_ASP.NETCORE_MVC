namespace Boookify.Web.Core.Models
{
    public class Subscribtion
    {
        public int Id { get; set; }

        public int SubscriberId { get; set; }

        public Subscriper? Subscriber { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? CreatedById { get; set; }
        public ApplicationUser? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
