namespace Boookify.Web.Core.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

    }
}
