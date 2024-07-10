using LicenseStat24.NewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using System.Text;

namespace LicenseStat24.Pages
{
    [Authorize(Roles = "ANALYSIS")]
    public class ClientsModel : PageModel
    {
       
        [BindProperty]
        public PagePostMod pageMod { get; set; }

        public async Task<IActionResult> OnGet()
        {
            // Получение значений из сессии
            pageMod = new PagePostMod();

            if (HttpContext.Session.TryGetValue("StartDate", out byte[] startDateBytes) &&
               HttpContext.Session.TryGetValue("EndDate", out byte[] endDateBytes) &&
               HttpContext.Session.TryGetValue("ModDate", out byte[] modDateBytes))
            {
                string startDateString = Encoding.UTF8.GetString(startDateBytes);
                string endDateString = Encoding.UTF8.GetString(endDateBytes);
                string modDateString = Encoding.UTF8.GetString(modDateBytes);

                if (DateTime.TryParseExact(startDateString, "dd-MM-yy", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime startDate) &&
                    DateTime.TryParseExact(endDateString, "dd-MM-yy", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime endDate) &&
                    int.TryParse(modDateString, out int modDate))
                {
                    pageMod.startDate = startDate;
                    pageMod.endDate = endDate;
                    pageMod.modDate = modDate;
                }
            }
            else
            {
                pageMod.startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                pageMod.endDate = DateTime.Now;
                pageMod.modDate = 0;
            }


            return Page();
        }

        public async Task<IActionResult> OnPost()
        {

            return Page();
        }

      
    }
}
