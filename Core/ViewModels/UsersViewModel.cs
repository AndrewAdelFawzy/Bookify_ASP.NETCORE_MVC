namespace Boookify.Web.Core.ViewModels
{
    public class UsersViewModel
    {
        public string Id { get; set; } = null!;

        public string UserName { get; set; } = null!;

        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public bool IsDeleted{ get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; } 
    }
}
