using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Boookify.Web.Core.ViewModels
{
    public class SubescriberFormViewModel
    {
        public string? Key { get; set; }

        [Display(Name = "First name")]
        [MaxLength(100,ErrorMessage =Errors.MaxLength)]
        [RegularExpression(RegexPatterns.CharactersOnlyEnglish,ErrorMessage =Errors.EnglishCharacters)]
        public string FirstName { get; set; } = null!;

        [Display(Name = "Last name")]
        [MaxLength(100, ErrorMessage = Errors.MaxLength)]
        [RegularExpression(RegexPatterns.CharactersOnlyEnglish, ErrorMessage = Errors.EnglishCharacters)]
        public string LastName { get; set; } = null!;

        [Display(Name = "Date of birth")]
        [AssertThat("DateOfBirth<= Today() ",ErrorMessage =Errors.NotAllowedDate)]
        public DateTime DateOfBirth { get; set; } = DateTime.Now;

        [MaxLength(14)]
        [RegularExpression(RegexPatterns.NationalId, ErrorMessage = Errors.InvalidNationalID)]
        [Remote("AllowNationalID", null!, AdditionalFields = "Key", ErrorMessage = Errors.Dublicated)]
        public string NationalId { get; set; } = null!;

        [MaxLength(11)]
        [RegularExpression(RegexPatterns.MobileNumber, ErrorMessage = Errors.InvalidPhoneNumber)]
        [Remote("AllowMobileNumber", null!, AdditionalFields = "Key", ErrorMessage = Errors.Dublicated)]
        public string MobileNumber { get; set; } = null!;

        public bool HasWhatsApp { get; set; }

        [EmailAddress, MaxLength(150, ErrorMessage = Errors.MaxLength)]
        [Remote("AllowEmail", null!, AdditionalFields = "Key", ErrorMessage = Errors.Dublicated)]
        public string Email { get; set; } = null!;

        [RequiredIf("Key == ''",ErrorMessage =Errors.EmptyImage)]
        public IFormFile? Image { get; set; } 

        public string? ImageUrl { get; set;}

        [Display(Name = "Governorate")] 
        public int GovernorateID { get; set; }
        public IEnumerable<SelectListItem>? Governorates { get; set; }

        [Display(Name ="Area")]
        public int AreaId { get; set; }
        public IEnumerable<SelectListItem>? Areas{ get; set; } = new List<SelectListItem>();

        [MaxLength(500)]
        [Display(Name ="Address")]
        public string Address { get; set; } = null!;

        public bool IsBlackListed { get; set; } 

    }
}
