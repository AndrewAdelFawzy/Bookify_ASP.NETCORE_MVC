using Hangfire;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using WhatsAppCloudApi;
using WhatsAppCloudApi.Services;
using static System.Net.Mime.MediaTypeNames;

namespace Boookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Reception)]
    public class SubscribersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostenvironment;
        private readonly IDataProtector _dataProtector;
        private readonly IMapper _mapper;
        private readonly IWhatsAppClient _whatsAppClient;
        private readonly IImageService _imageService;
		private readonly IEmailSender _emailSender;
		private readonly IEmailBodyBuilder _emailBodyBuilder;

		public SubscribersController(ApplicationDbContext context,
			IMapper mapper,
			IDataProtectionProvider dataProtector,
			IImageService imageService,
			IWebHostEnvironment webHostEnvironment,
			IWhatsAppClient whatsAppClient,
			IEmailBodyBuilder emailBodyBuilder,
			IEmailSender emailSender)
		{
			_context = context;
			_mapper = mapper;
			_dataProtector = dataProtector.CreateProtector("MySecureKey");
			_imageService = imageService;
			_webHostenvironment = webHostEnvironment;
			_whatsAppClient = whatsAppClient;
			_emailBodyBuilder = emailBodyBuilder;
            _emailSender = emailSender;
		}

		public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        { 
            return View("Form", PopulateViewModel());
        }

        [AjaxOnly]
        public IActionResult GetAreas(int governorateId)
        {
            var areas = _context.Areas
                .Where(a => a.GovernorateID == governorateId)
                .Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name,
                })
                .OrderBy(g => g.Text)
                .ToList();

            return Ok(areas);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubescriberFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", PopulateViewModel(model));

            var subescriber = _mapper.Map<Subscriper>(model);

            if (model.Image is not null)
            {
                var ImageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";
                var ImagePath = "/Images/subscribers";

                var (isUploaded, errorMessage) = await _imageService.UploadASync(model.Image, ImageName, ImagePath);

                if (isUploaded)
                {
                    subescriber.ImageUrl = $"{ImagePath}/{ImageName}";
                }
                else
                {
                    ModelState.AddModelError(nameof(Image), errorMessage!);
                    return View("Form", PopulateViewModel(model));
                }
            }
            subescriber.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            Subscribtion subscribtion = new()
            {
                CreatedById = subescriber.CreatedById,
                CreatedAt = subescriber.CreatedAt,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddYears(1),
            };

            subescriber.Subscribtions.Add(subscribtion);

            _context.Add(subescriber);
            _context.SaveChanges();

            if (model.HasWhatsApp)
            {
                    var components = new List<WhatsAppComponent>()
                {
                    new WhatsAppComponent
                    {
                        Type = "body",
                        Parameters = new List<object>()
                        {
                            new WhatsAppTextParameter{Text ="Roro"}
                        }
                    }
                };

                var mobileNumber = _webHostenvironment.IsDevelopment() ? "01227151035" : model.MobileNumber;

                BackgroundJob.Enqueue(() => _whatsAppClient
                .SendMessage($"2{mobileNumber}", WhatsAppLanguageCode.English, WhatsAppTemplates.WelcomeMessage, components));

            }

            var subescriberId = _dataProtector.Protect(subescriber.Id.ToString());
			return RedirectToAction(nameof(Details), new {id =subescriberId});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(SearchFormViewModel model)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var subscriber = _context.Subscripers
                .SingleOrDefault(s => s.NationalId == model.Value
                                 || s.MobileNumber == model.Value
                                 || s.Email == model.Value);

            var viewModel = _mapper.Map<SubscriberSearchResultViewModel>(subscriber);

            if(subscriber is not null)
            viewModel.Key = _dataProtector.Protect(subscriber.Id.ToString());

            return PartialView("_Result", viewModel);
        }

		public IActionResult Details(string id)
		{
			var subscriberId = int.Parse(_dataProtector.Unprotect(id));

			var subscriber = _context.Subscripers
				.Include(a => a.Area)
				.Include(a => a.Governorate)
                .Include(a=>a.Subscribtions)
				.SingleOrDefault(s => s.Id == subscriberId);

			if (subscriber is null)
				return NotFound();

			var viewModel = _mapper.Map<SubscriberViewModel>(subscriber);
            viewModel.Key = id;

			return View(viewModel);
		}


		public IActionResult Edit(string id)
        {
          
            var subscriberId =int.Parse(_dataProtector.Unprotect(id));
            var subescriber = _context.Subscripers.Find(subscriberId);


            if (subescriber is null)
                return NotFound();

            var model = _mapper.Map<SubescriberFormViewModel>(subescriber);
            model.Key = id;

            return View("Form",PopulateViewModel(model));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubescriberFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", PopulateViewModel(model));

            var subscriberId = int.Parse(_dataProtector.Unprotect(model.Key));

            var Subscriber = _context.Subscripers.Find(subscriberId);

            if (Subscriber is null)
                return NotFound();

            if (model.Image is not null)
            {
                if (!string.IsNullOrEmpty(Subscriber.ImageUrl))
                    _imageService.Delete(Subscriber.ImageUrl);

                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";
                var imagePath = "/Images/subscribers";

                var (isUploaded, errorMessage) = await _imageService.UploadASync(model.Image, imageName, imagePath);

                if (!isUploaded)
                {
                    ModelState.AddModelError("Image", errorMessage!);
                    return View("Form", PopulateViewModel(model));
                }

                model.ImageUrl = $"{imagePath}/{imageName}";

            }

            else if (!string.IsNullOrEmpty(Subscriber.ImageUrl))
            {
                model.ImageUrl = Subscriber.ImageUrl;

            }

            Subscriber = _mapper.Map(model, Subscriber);
            Subscriber.UpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            Subscriber.UpdatedAt = DateTime.Now;

            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = model.Key });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RenewSubscription(string sKey)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(sKey));

            var subscriber = _context.Subscripers
                                     .Include(s=>s.Subscribtions)
                                     .SingleOrDefault(s=>s.Id==subscriberId);

            if (subscriber is null)
                return NotFound();

            if (subscriber.IsBlackListed)
                return BadRequest();

            var lastSubscribtion = subscriber.Subscribtions.Last();

            var startDate = lastSubscribtion.EndDate < DateTime.Today 
                            ? DateTime.Today
                            : lastSubscribtion.EndDate.AddYears(1);

            Subscribtion NewSubscribtion = new()
            {
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value,
                CreatedAt = DateTime.Now,
                StartDate = startDate,
                EndDate = startDate.AddYears(1)
            };

            subscriber.Subscribtions.Add(NewSubscribtion);

            _context.SaveChanges();
            var viewModel = _mapper.Map<SubscribtionViewModel>(NewSubscribtion);

			if (subscriber.HasWhatsApp)
			{
				var components = new List<WhatsAppComponent>()
				{
					new WhatsAppComponent
					{
						Type = "body",
						Parameters = new List<object>()
						{

							new WhatsAppTextParameter { Text = NewSubscribtion.EndDate.ToString("d MMM, yyyy") },
						}
					}
				};

				var mobileNumber = _webHostenvironment.IsDevelopment() ? "01227151035" : subscriber.MobileNumber;

				//Change 2 with your country code
				BackgroundJob.Enqueue(() => _whatsAppClient
					.SendMessage($"2{mobileNumber}", WhatsAppLanguageCode.English,
					WhatsAppTemplates.SuccessfullSubscribtion, components));
				
			}

            BackgroundJob.Enqueue(() => _emailSender
            .SendEmailAsync("androadel@icloud.com", "Congratulations!!", "your Bookify Subscribtion is renewed succssfully"));
			 

			return PartialView("_SubscribtionRow", viewModel);
        }

        public IActionResult AllowNationalID(SubescriberFormViewModel model)
        {
            var subscriberId = 0;

            if(!string.IsNullOrEmpty(model.Key))
                subscriberId =int.Parse(_dataProtector.Unprotect(model.Key));

            var subscriber = _context.Subscripers.SingleOrDefault(s => s.NationalId == model.NationalId);
            var isAllowed = subscriber is null || subscriber.Id.Equals(subscriberId);

            return Json(isAllowed);
        }


        public IActionResult AllowEmail(SubescriberFormViewModel model)
        {
            var subscriberId = 0;

            if (!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key));

            var subscriber = _context.Subscripers.SingleOrDefault(s => s.Email == model.Email);
            var isAllowed = subscriber is null || subscriber.Id.Equals(subscriberId);

            return Json(isAllowed);
        }

        public IActionResult AllowMobileNumber(SubescriberFormViewModel model)
        {
            var subscriberId = 0;
            if (!string.IsNullOrEmpty(model.Key))
                subscriberId = int.Parse(_dataProtector.Unprotect(model.Key));

            var subscriber = _context.Subscripers.SingleOrDefault(s => s.MobileNumber == model.MobileNumber);
            var isallowed = subscriber is null || subscriber.Id.Equals(subscriberId);

            return Json(isallowed);
        }
    

        private SubescriberFormViewModel PopulateViewModel(SubescriberFormViewModel? model = null)
        {
            SubescriberFormViewModel viewModel = model is null ? new SubescriberFormViewModel() : model;

            var governorates = _context.Governorates.Where(a => !a.IsDeleted).OrderBy(a => a.Name).ToList();
           viewModel.Governorates = _mapper.Map<IEnumerable<SelectListItem>>(governorates);

            if (model?.GovernorateID > 0)
            {
                var areas = _context.Areas
                    .Where(a => a.GovernorateID == model.GovernorateID && !a.IsDeleted)
                    .OrderBy(a => a.Name)
                    .ToList();

                viewModel.Areas = _mapper.Map<IEnumerable<SelectListItem>>(areas);
            }
           return viewModel;
        }


   }
}
