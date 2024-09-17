using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;

namespace Boookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly IEmailBodyBuilder _emailBodyBuilder;
       
        private readonly IMapper _mapper;

        public UsersController(UserManager<ApplicationUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager, IEmailSender emailSender, IEmailBodyBuilder emailBodyBuilder)
        {
            _userManager = userManager;
            _mapper = mapper;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _emailBodyBuilder = emailBodyBuilder;
        }

        public async Task<IActionResult> Index()
        {
			throw new Exception("My Exception");
			var users = await _userManager.Users.ToListAsync();
            var viewModel = _mapper.Map<IEnumerable<UsersViewModel>>(users);
            return View(viewModel);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new UserFormViewModel
            {
                Roles = await _roleManager.Roles
                .Select(r => new SelectListItem
                {
                    Text = r.Name,
                    Value = r.Name

                }).ToListAsync()
            };

            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = new ApplicationUser
            {
                FullName = viewModel.FullName,
                UserName = viewModel.UserName,
                Email = viewModel.Email,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            };

            var result = await _userManager.CreateAsync(user, viewModel.Password);

            if (result.Succeeded)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = user.Id, code },
                    protocol: Request.Scheme);

                var body = _emailBodyBuilder.GetEmailBody(
                    "https://res.cloudinary.com/devcreed/image/upload/v1668732314/icon-positive-vote-1_rdexez.svg",
                    $"hey {user.FullName} thanks for joining us",
                    "Please confirm your email",
                    $"{HtmlEncoder.Default.Encode(callbackUrl!)}",
                     "Active Account");

                await _emailSender.SendEmailAsync(user.Email, "Confirm your email", body);

                await _userManager.AddToRolesAsync(user, viewModel.SelectedRoles);
                return RedirectToAction("Index");
            }
            return BadRequest(string.Join(",", result.Errors.Select(e=>e.Description)));
        }

        public async Task <IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();

            ResetPasswordViewModel viewModel = new()
            {
                Id= id,
            };

            return View("ResetPassword",viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task <IActionResult> ResetPassword(ResetPasswordViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByIdAsync(viewModel.Id);

            if (user is null)
                return NotFound();

            var currentPasswordHash  =user.PasswordHash;

            await _userManager.RemovePasswordAsync(user);

            var result = await _userManager.AddPasswordAsync(user, viewModel.Password);

            if (result.Succeeded)
            {
                user.UpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                user.UpdatedAt = DateTime.Now;

                await _userManager.UpdateAsync(user);
                return RedirectToAction(nameof(Index));
            }
            user.PasswordHash = currentPasswordHash;
            await _userManager.UpdateAsync(user);

            return BadRequest(string.Join(",", result.Errors.Select(e => e.Description)));  
        }

        [HttpGet]
        public async Task <IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
                return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            var viewModel = _mapper.Map<UserFormViewModel>(user);

            viewModel.Roles = _roleManager.Roles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name,
            }).ToList();

            viewModel.SelectedRoles = currentRoles.ToList();
            return View("Form",viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task <IActionResult> Edit(UserFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(viewModel.Id);

            if (user is null)
                return NotFound();
            
            user = _mapper.Map(viewModel, user);
            user.UpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            user.UpdatedAt = DateTime.Now;

            var result =await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                var rolsUpdated = !currentRoles.SequenceEqual(viewModel.SelectedRoles);

                if(rolsUpdated)
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRolesAsync(user, viewModel.SelectedRoles);
                }

                return RedirectToAction(nameof(Index));
            }

            return BadRequest(string.Join(",", result.Errors.Select(e => e.Description)));

            
        }

        public async Task <IActionResult> UnlockAccount(string id)
        {
            var user =await _userManager.FindByIdAsync(id);

            if (user is null)
                return NotFound();

            var isLocked = await _userManager.IsLockedOutAsync(user);

            if (isLocked)
                await _userManager.SetLockoutEndDateAsync(user, null);
           
            return RedirectToAction(nameof(Index));
        }


        public async Task <IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
                return NotFound();
            
            user.IsDeleted = !user.IsDeleted;
            user.UpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            user.UpdatedAt = DateTime.Now;

            await _userManager.UpdateAsync(user);

            if(user.IsDeleted)
                await _userManager.UpdateSecurityStampAsync(user);// to log out the user the admin deleted the user 

            return Ok();
        }

        public async Task<IActionResult> AllowUserName(UserFormViewModel viewModel)
        {
            var user= await _userManager.FindByNameAsync(viewModel.UserName);

            var isAllowed = user is null || user.Id.Equals(viewModel.Id);

            return Json(isAllowed);
        }

        public async Task<IActionResult> AllowUserEmail(UserFormViewModel viewModel)
        {
            var user = await _userManager.FindByEmailAsync(viewModel.Email);
            var isAllowed = user is null || user.Id.Equals(viewModel.Id);
            return Json(isAllowed);
        }

    }   
            
}