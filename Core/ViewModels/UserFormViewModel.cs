using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Boookify.Web.Core.ViewModels
{
    public class UserFormViewModel
    {
        public string? Id { get; set; }

        [MaxLength(20,ErrorMessage =Errors.MaxLength)]
        [Remote("AllowUserName", null!,AdditionalFields = "Id", ErrorMessage =Errors.Dublicated)]
        [RegularExpression(RegexPatterns.UserNameRegex,ErrorMessage =Errors.AlphaNumric)]
        public string UserName { get; set; } = null!;

        [MaxLength(20, ErrorMessage = Errors.MaxLength), Display(Name = "Full Name")]
        [RegularExpression(RegexPatterns.CharactersOnlyEnglish,ErrorMessage =Errors.EnglishCharacters)]
        public string FullName { get; set; } = null!;

        [EmailAddress,MaxLength(50, ErrorMessage = Errors.MaxLength)]
        [Remote("AllowUserEmail", null!,AdditionalFields ="Id", ErrorMessage = Errors.Dublicated)]
        public string Email { get; set; } = null!;

        [DataType(DataType.Password),
            StringLength(20,ErrorMessage =Errors.MaxMinLength,MinimumLength =8)]
        [RegularExpression(RegexPatterns.PasswordRegex, ErrorMessage =Errors.WeakPassword)]
        [RequiredIf("Id==null",ErrorMessage =Errors.RequiredField)]
        public string? Password { get; set; } = null!;

        [DataType(DataType.Password),
            Compare("Password",ErrorMessage = Errors.ConfirmPasswordNotMatch),Display(Name ="Confirm Password")]
        [RequiredIf("Id==null", ErrorMessage = Errors.RequiredField)]
        public string? ConfirmPassword { get; set; } = null!;

        [Display(Name = "Roles")]
        public IList<string> SelectedRoles { get; set; } = new List<string>();
        public IEnumerable<SelectListItem>? Roles { get; set; }
    }

}
