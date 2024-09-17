namespace Boookify.Web.Core.Models
{
    [Index(nameof(Name),nameof(GovernorateID) , IsUnique = true)]
    public class Area:BaseModel
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public int GovernorateID { get; set; }

        public Governorate? Governorate { get; set; }
    }
}
