using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Boookify.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Governorate> Governorates { get; set; }
        public DbSet<Subscriper> Subscripers { get; set; }
        public DbSet<Subscribtion> Subscribtions { get; set; }


        




        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasSequence<int>("SerialNumber", schema: "shared")
                   .StartsAt(10000001);

            builder.Entity<BookCopy>()
                    .Property(e => e.SerialNumber)
                    .HasDefaultValueSql("NEXT VALUE FOR shared.SerialNumber");

            // To make a compiste key
            builder.Entity<BookCategory>().HasKey(e => new {e.BookId,e.CategoryId});

            var cascadeFKs = builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade && !fk.IsOwnership);

            foreach (var fk in cascadeFKs)
                fk.DeleteBehavior = DeleteBehavior.Restrict; 

            base.OnModelCreating(builder);
        }
    }
}
