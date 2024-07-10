using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using LicenseStat24.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace LicenseStat24.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginWithTelegram : PageModel
    {
        private readonly SignInManager<LicenseStat24User> _signInManager;
        private readonly ILogger<LoginWithTelegram> _logger;

        public LoginWithTelegram(SignInManager<LicenseStat24User> signInManager, ILogger<LoginWithTelegram> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public bool RememberMe { get; set; }
        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Authenticator code")]
            public string TwoFactorCode { get; set; }

            [Display(Name = "Remember this machine")]
            public bool RememberMachine { get; set; }
        }

        public async Task<IActionResult> OnPostAsync(bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            // Получаем введенный пользователем код из модели
            var inputCode = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            // Получаем сохраненный код из сессии
            var savedCode = HttpContext.Session.GetString("TelegramCode");

            // Сравниваем введенный пользователем код с сохраненным кодом
            if (inputCode != savedCode)
            {
                // Если коды не совпадают, возвращаем страницу с ошибкой
                ModelState.AddModelError(string.Empty, "Invalid code.");
                return Page();
            }

            // Удаляем сохраненный код из сессии, чтобы его нельзя было использовать повторно
            HttpContext.Session.Remove("TelegramCode");

            // Пользователь успешно аутентифицирован, выполняем вход
            await _signInManager.SignInAsync(user, isPersistent: rememberMe);
            _logger.LogInformation("User logged in with 2fa.");

            // Перенаправляем пользователя на страницу, с которой он начал аутентификацию
            return LocalRedirect(returnUrl);
        }


    }
}
