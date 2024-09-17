namespace Boookify.Web.Core.ViewModels
{
    public class SubscribtionViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Status 
        {
            get
            {
                return DateTime.Today > EndDate ? SubscribtionStatus.Expired : DateTime.Today < StartDate ? string.Empty : SubscribtionStatus.Active;
            }
                
        }
    }
}
