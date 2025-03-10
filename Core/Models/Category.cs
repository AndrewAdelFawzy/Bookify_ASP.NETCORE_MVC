﻿namespace Boookify.Web.Core.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Category : BaseModel
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = null!;

        //to reach the books of the Category
        public ICollection<BookCategory> Books { get; set; } = new List<BookCategory>();

    }
}
