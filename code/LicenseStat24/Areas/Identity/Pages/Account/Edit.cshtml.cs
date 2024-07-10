using LicenseStat24.Areas.Identity.Data;
using LicenseStat24.NewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LicenseStat24.Areas.Identity.Pages.Account
{
    public class EditModel : PageModel
    {
        private readonly UserManager<LicenseStat24User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public EditModel(UserManager<LicenseStat24User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            pageMod = new PagePostMod();
        }

        [BindProperty]
        public string UserId { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }
        public PagePostMod pageMod { get; set; }

        public IList<SelectListItem> AvailableRoles { get; set; }

        public class InputModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string ConfirmPassword { get; set; }
            public string Role { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            pageMod.datesVisible = false;

            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            UserId = user.Id;
            Input = new InputModel
            {
                Email = user.Email
            };

            // Получаем список всех ролей из менеджера ролей
            var roles = _roleManager.Roles.ToList();
            AvailableRoles = new List<SelectListItem>();

            foreach (var role in roles)
            {
                AvailableRoles.Add(new SelectListItem { Value = role.Name, Text = role.Name });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            pageMod.datesVisible = false;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                return NotFound();
            }

            // Обновляем email пользователя
            user.Email = Input.Email;
            var emailResult = await _userManager.UpdateAsync(user);
            if (!emailResult.Succeeded)
            {
                foreach (var error in emailResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return Page();
            }

            // Изменяем пароль пользователя, если он был указан
            if (!string.IsNullOrEmpty(Input.Password) && Input.Password == Input.ConfirmPassword)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, Input.Password);
                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return Page();
                }
            }

            // Удаляем все текущие роли пользователя
            var roles = await _userManager.GetRolesAsync(user);
            var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!removeRolesResult.Succeeded)
            {
                foreach (var error in removeRolesResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return Page();
            }

            // Назначаем новую роль пользователю
            var addRoleResult = await _userManager.AddToRoleAsync(user, Input.Role);
            if (!addRoleResult.Succeeded)
            {
                foreach (var error in addRoleResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return Page();
            }

            return RedirectToPage("/Account/Register");
        }
    }
}
