namespace Boookify.Web.Core.Models
{
    public class BaseModel
    {
        public bool IsDeleted { get; set; }

        public string? CreatedById { get; set; }
        public ApplicationUser? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? UpdatedById { get; set; }
        public ApplicationUser? UpdatedBy { get; set; }


        public DateTime? UpdatedAt { get; set; }

    }
}
