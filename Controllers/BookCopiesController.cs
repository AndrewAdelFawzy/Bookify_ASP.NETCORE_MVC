using Boookify.Web.Core.Models;
using Microsoft.AspNetCore.Authentication;

namespace Boookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BookCopiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public BookCopiesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Create(int bookId)
        {
            var book = _context.Books.Find(bookId);
            if (book is null)
                return NotFound();

            var viewModel = new BookCopyFormViewModel
            {
                ShowAvailableForRental = book.IsAvailableForRental,
                BookId = bookId
            };
            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BookCopyFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View("Form", viewModel);

            var book = _context.Books.Find(viewModel.BookId);
            if (book is null)
                return NotFound();

            var copy = _mapper.Map<BookCopy>(viewModel);
            copy.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            _context.BookCopies.Add(copy);
            _context.SaveChanges();
            return RedirectToAction("Details", "Books", new { @id = viewModel.BookId });
        }

        [HttpGet]
        public IActionResult Edit(int copyId)
        {
            var copy = _context.BookCopies.Find(copyId);

            if (copy is null)
                return NotFound();

            var book = _context.Books.Find(copy.BookId);

            if (book is null)
                return NotFound();


            var viewModel = new BookCopyFormViewModel
            {
                ShowAvailableForRental = copy.IsAvailableForRental,
                IsAvailableForRental = copy.IsAvailableForRental,
                Id = copy.Id,
                EditionNumber = copy.EditionNumber,
            };

            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BookCopyFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View("Form", viewModel);

            var copy = _context.BookCopies.Find(viewModel.Id);

            if (copy is null)
                return NotFound();

            var book = _context.Books.Find(copy.BookId);

            if (book is null)
                return NotFound();


          copy.EditionNumber = viewModel.EditionNumber;
          copy.IsAvailableForRental = viewModel.IsAvailableForRental;
          copy.UpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
          copy.UpdatedAt= DateTime.Now;
        
            _context.SaveChanges();

            return RedirectToAction("Details", "Books", new { @id = copy.BookId });
        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var copy = _context.BookCopies.Find(id);
            if (copy is null)
                return NotFound();

            copy.IsDeleted = !copy.IsDeleted;
            copy.UpdatedAt = DateTime.Now;
            copy.UpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            _context.SaveChanges();
            return Ok();
        }
    }
}
