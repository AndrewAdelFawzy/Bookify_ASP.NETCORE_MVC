namespace Boookify.Web.Core.ViewModels
{
    public class BookCopyFormViewModel
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        [Display(Name = "Is Available For Rental")]
        public bool IsAvailableForRental { get; set; }

        [Range(1,1000,ErrorMessage =Errors.NumberRange)]
        [Display(Name ="Edition Number")]
        public int EditionNumber { get; set; }

        public bool ShowAvailableForRental { get; set; }
    }
}
