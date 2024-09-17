namespace Boookify.Web.Core.ViewModels
{
    public class AuthorViewModel
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
