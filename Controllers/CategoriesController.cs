namespace Boookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CategoriesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Index()
        {

            var categories = _context.Categories.AsNoTracking().ToList();

            var viewModel = _mapper.Map<IEnumerable<CategoryViewModel>>(categories);
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("Form");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", model);


            var category = _mapper.Map<Category>(model);
            category.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            _context.Categories.Add(category);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int Id)
        {
            var category = _context.Categories.Find(Id);

            if (category is null)
                return NotFound();

            var viewModel = _mapper.Map<CategoryFormViewModel>(category);
            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", model);

            var category = _context.Categories.Find(model.Id);
            if (category is null)
                return NotFound();

            category.Name = model.Name;
            category.UpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            category.UpdatedAt = DateTime.Now;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult ToggleStatus(int Id)
        {
            var category = _context.Categories.Find(Id);

            if (category is null)
                return NotFound();

            category.UpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            category.IsDeleted = !category.IsDeleted;//to toggle the status to its opposite state(true or false) 
            category.UpdatedAt = DateTime.Now;
            _context.SaveChanges();

            return Ok(category.UpdatedAt.ToString());//succesfull 
        }

        public IActionResult AllowItem(CategoryFormViewModel model)
        {
            var category = _context.Categories.SingleOrDefault(c => c.Name == model.Name);
            var isAllowed = category is null || category.Id == model.Id;

            return Json(isAllowed);
        }

    }
}
