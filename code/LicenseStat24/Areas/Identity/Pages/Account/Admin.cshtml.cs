using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using LicenseStat24.Areas.Identity.Data;
using LicenseStat24.NewModels;
using Microsoft.EntityFrameworkCore;

namespace LicenseStat24.Areas.Identity.Pages.Account
{
    [Authorize(Roles = "ADMIN")]
    public class AdminModel : PageModel
    {
        public UserManager<LicenseStat24User> _userManager;

        public AdminModel(UserManager<LicenseStat24User> userManager)
        {
            _userManager = userManager;
        }

        public IList<LicenseStat24User> Users { get; set; } // Подставьте вашу модель пользователя


        [BindProperty]
        public PagePostMod pageMod { get; set; }

        public void OnGet()
        {
            pageMod = new PagePostMod();
            pageMod.datesVisible = false;
            Users = _userManager.Users.ToList(); // Получаем всех пользователей
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

                    return RedirectToPage("/Account/Admin");
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
