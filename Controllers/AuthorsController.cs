namespace Boookify.Web.Controllers
{
    [Authorize(Roles =AppRoles.Archive)]
    public class AuthorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public AuthorsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var authors = _context.Authors.AsNoTracking().ToList();
            var viewModel = _mapper.Map<IEnumerable<AuthorViewModel>>(authors);

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("AuthorForm");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AuthorFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            var author = _mapper.Map<Author>(viewModel);
            author.CreatedById =User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            _context.Authors.Add(author);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int Id)
        {
            var author = _context.Authors.Find(Id);

            if (author is null)
                return NotFound();

            var viewModel = _mapper.Map<AuthorFormViewModel>(author);
            return View("AuthorForm", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AuthorFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View("AuthorForm", viewModel);

            var author = _context.Authors.Find(viewModel.Id);

            if (author is null)
                return NotFound();

            author.Name = viewModel.Name;
            author.UpdatedAt = DateTime.Now;
            author.UpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));

        }

        [HttpPost]
        public IActionResult ToggleStatus(int Id)
        {
            var author = _context.Authors.Find(Id);

            if (author is null)
                return NotFound();

            author.IsDeleted = !author.IsDeleted;
            author.UpdatedAt = DateTime.Now;
            author.UpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value; 
            _context.SaveChanges();

            return Ok(author.UpdatedAt.ToString());

        }

        public IActionResult AllowItem(AuthorFormViewModel viewModel)
        {
            var author = _context.Authors.SingleOrDefault(a => a.Name == viewModel.Name);

            var isAllowed = author is null || author.Id == viewModel.Id;

            return Json(isAllowed);

        }

    }
}
