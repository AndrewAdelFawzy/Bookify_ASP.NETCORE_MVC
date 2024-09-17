using Microsoft.AspNetCore.Identity;

namespace Boookify.Web.Core.Models
{
    [Index(nameof(UserName),IsUnique =true)]
    [Index(nameof(Email),IsUnique =true)]
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public string? CreatedById { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? UpdatedById { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }
}
