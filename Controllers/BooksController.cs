using static System.Net.Mime.MediaTypeNames;

namespace Boookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IImageService _imageService;

        private List<string> _allowedExtentions = new() { ".jpg", ".png", ".jpeg" };
        private int _maxAllowedSize = 2097152;// 2MB(inBytes) 2 * 1024 * 1024

		public BooksController(ApplicationDbContext context, IMapper mapper, IWebHostEnvironment webHostEnvironment, IImageService imageService)
		{
			_context = context;
			_mapper = mapper;
			_webHostEnvironment = webHostEnvironment;
			_imageService = imageService;
		}

		public IActionResult Index()
        {
            var books = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .ThenInclude(b => b.Category)
                .ToList();
            var viewModel = _mapper.Map<IEnumerable<BookViewModel>>(books);
            return View(viewModel);
        }

        public IActionResult Details(int id)
        {
            var book = _context.Books
                .Include(a => a.Author)
                .Include(a=>a.Copies)
                .Include(a => a.Categories)
                .ThenInclude(a=>a.Category)
                .SingleOrDefault(b => b.Id == id);

            if(book is null)
                return NotFound();

            var viewModel = _mapper.Map<BookViewModel>(book);

            return View(viewModel);
        }

        public IActionResult Create()
        {
            return View("BookForm",PopulateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookFormViewModel model)
        {
            if(!ModelState.IsValid)
                return View("BookForm",PopulateViewModel(model));

            var book = _mapper.Map<Book>(model);
           
            if (model.Image is not null)
            {
                var ImageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";

                var (isUploaded, errorMessage) = await _imageService.UploadASync(model.Image, ImageName, "/Images/Books");

				if (isUploaded)
                {
                    book.ImageUrl = ImageName;
                }
                else
                {
                    ModelState.AddModelError(nameof(Image),errorMessage!);
                    return View("BookForm", PopulateViewModel(model));
                }	
			}

            foreach (var category in model.SelectedCategories)
                book.Categories.Add(new BookCategory { CategoryId = category });

			book.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
			_context.Add(book);
            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new {id=book.Id});
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var book = _context.Books.Include(b=>b.Categories).SingleOrDefault(b=>b.Id ==id);
            if (book is null)
                return NotFound();

            var model = _mapper.Map<BookFormViewModel>(book);
            var viewModel = PopulateViewModel(model);

            viewModel.SelectedCategories = book.Categories.Select(c=>c.CategoryId).ToList();

            return View("BookForm",viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("BookForm", PopulateViewModel(model));
 
            var book = _context.Books
                .Include(b => b.Categories)
                .Include(b=>b.Copies)
                .SingleOrDefault(b => b.Id == model.Id);

            if (book is null)
                return NotFound();

            if (model.Image is not null)
            {
                if (!string.IsNullOrEmpty(book.ImageUrl))
                {
                   _imageService.Delete($"/Images/Books/{book.ImageUrl}");
                }

                var extention = Path.GetExtension(model.Image.FileName);

				var ImageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";

				var (isUploaded, errorMessage) = await _imageService.UploadASync(model.Image, ImageName, "/Images/Books");

				if (isUploaded)
				{
					model.ImageUrl = ImageName;
				}
				else
				{
					ModelState.AddModelError(nameof(Image), errorMessage!);
					return View("BookForm", PopulateViewModel(model));
				}
	
            }

            else if(!string.IsNullOrEmpty(book.ImageUrl))
             model.ImageUrl = book.ImageUrl; 

           
            book = _mapper.Map(model,book);
            book.UpdatedAt = DateTime.Now;
            book.UpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            foreach (var category in model.SelectedCategories)
                book.Categories.Add(new BookCategory { CategoryId = category });

            if (!model.IsAvailableForRental)
            {
                foreach (var copy in book.Copies)
                {
                    copy.IsAvailableForRental = false;
                }
            }
                


            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = book.Id });
        }

        public IActionResult ToggleStatus(int id)
        {
            var book = _context.Books.Find(id);
            if (book is null)
                return NotFound();

            book.IsDeleted = !book.IsDeleted;
            book.UpdatedAt= DateTime.Now;
            book.UpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            _context.SaveChanges();
            return Ok();
        }


        public IActionResult AllowItem(BookFormViewModel model)
        {
            var book = _context.Books.SingleOrDefault(b => b.Title == model.Title && b.AuthorId==model.AuthorId);
            var isAllowed = book is null || book.Id == model.Id;

            return Json(isAllowed);
        }


        private BookFormViewModel PopulateViewModel(BookFormViewModel? model = null)
        {
            BookFormViewModel viewModel = model is null ? new BookFormViewModel() : model;

            var authors = _context.Authors.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToList();
            var categories = _context.Categories.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToList();

            viewModel.Authors = _mapper.Map<IEnumerable<SelectListItem>>(authors);
            viewModel.Categories = _mapper.Map<IEnumerable<SelectListItem>>(categories);


            return viewModel;
        }
    }
}
