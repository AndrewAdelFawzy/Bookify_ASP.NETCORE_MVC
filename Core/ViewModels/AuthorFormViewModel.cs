﻿namespace Boookify.Web.Core.ViewModels
{
    public class AuthorFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = Errors.MaxLength), Display(Name = "Author")]
        [Remote("AllowItem", "Authors", AdditionalFields = "Id", ErrorMessage = Errors.Dublicated)]
        public string Name { get; set; } = null!;
    }
}
