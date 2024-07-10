using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using LicenseStat24.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using LicenseStat24.NewModels;
using Microsoft.EntityFrameworkCore;

namespace LicenseStat24.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<LicenseStat24User> _signInManager;
        public readonly UserManager<LicenseStat24User> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RegisterModel(
            UserManager<LicenseStat24User> userManager,
            SignInManager<LicenseStat24User> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            pageMod = new PagePostMod();
        }


        [BindProperty]
        public InputModel Input { get; set; }
        public PagePostMod pageMod { get; set; }

        public SelectList AvailableRoles { get; set; }
        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [Display(Name = "User role")]
            public string Role { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            pageMod.datesVisible = false;

            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            var roles = _roleManager.Roles.ToList();
            AvailableRoles = new SelectList(roles, nameof(IdentityRole.Name), nameof(IdentityRole.Name));

        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            pageMod.datesVisible = false;

            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new LicenseStat24User { UserName = Input.Email, Email = Input.Email };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    if (!string.IsNullOrEmpty(Input.Role))
                    {
                        await _userManager.AddToRoleAsync(user, Input.Role);
                        _logger.LogInformation(string.Format("Role added {0}.", Input.Role));

                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(string userId)
        {
            pageMod.datesVisible = false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var isAdmin = await _userManager.IsInRoleAsync(user, "ADMIN");
                if (isAdmin)
                {
                    // Получаем всех пользователей из базы данных
                    var allUsers = await _userManager.Users.ToListAsync();

                    // Считаем количество администраторов среди загруженных пользователей
                    var adminCount = allUsers.Count(u => _userManager.IsInRoleAsync(u, "ADMIN").Result);

                    if (adminCount == 1) // Если количество администраторов равно 1, отменяем запрос на удаление
                    {
                        ModelState.AddModelError("", "Нельзя удалить последнего администратора.");
                        return Page();
                    }
                }

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User deleted successfully.");
                    return RedirectToPage("/Account/Register");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return Page();
                }
            }
            else
            {
                return NotFound();
            }
        }



    }
}
