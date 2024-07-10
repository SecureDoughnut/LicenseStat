using LicenseStat24.BDRepos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Globalization;
using static LicenseStat24.PageCalcs.DataHelper;
using LicenseStat24.NewModels;
using LicenseStat24.PageCalcs;

namespace LicenseStat24.Pages
{
    [Authorize]
    public class CommonModel : PageModel
    {

        [BindProperty]
        public PagePostMod pageMod { get; set; }

        public async Task<IActionResult> OnGet()
        {
            if (User.IsInRole("ADMIN"))
                return RedirectToPage("/Account/Register", new { area = "Identity" });

            pageMod = new PagePostMod();
            cliAllData.clients = SqlNew.getAllClientsWithoutDate();

            // Получили данные из сессии
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
                pageMod.startDate = new DateTime(DateTime.Now.Year, 1, 1);
                pageMod.endDate = DateTime.Now;
                pageMod.modDate = 0;

                HttpContext.Session.SetString("StartDate", pageMod.startDate.ToString("dd-MM-yy"));
                HttpContext.Session.SetString("EndDate", pageMod.endDate.ToString("dd-MM-yy"));
                HttpContext.Session.SetString("ModDate", pageMod.modDate.ToString());
            }

            // Если нет разбиения на подинтервалы
            DataHelper.cliSingle.clients = SqlNew.getAllClients(pageMod.startDate, pageMod.endDate);
            DataHelper.cliSingle.startDate = pageMod.startDate;
            DataHelper.cliSingle.endDate = pageMod.endDate;




            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            HttpContext.Session.SetString("StartDate", pageMod.startDate.ToString("dd-MM-yy"));
            HttpContext.Session.SetString("EndDate", pageMod.endDate.ToString("dd-MM-yy"));
            HttpContext.Session.SetString("ModDate", pageMod.modDate.ToString());

            DataHelper.cliSingle.clients = SqlNew.getAllClients(pageMod.startDate, pageMod.endDate);
            

            // Если есть разбиение на подинтервалы

            if (pageMod.modDate > 0)
            {
                if (pageMod.modDate == 1)
                    DataHelper.cliMultiple = SplitByWeeks(pageMod.startDate, pageMod.endDate);
                else if (pageMod.modDate == 2)
                    DataHelper.cliMultiple = SplitByMonths(pageMod.startDate, pageMod.endDate);
                else if (pageMod.modDate == 3)
                    DataHelper.cliMultiple = SplitByQuarters(pageMod.startDate, pageMod.endDate);
                else if (pageMod.modDate == 4)
                    DataHelper.cliMultiple = SplitByHalfYears(pageMod.startDate, pageMod.endDate);
                else if (pageMod.modDate == 5)
                    DataHelper.cliMultiple = SplitByYears(pageMod.startDate, pageMod.endDate);

                for (int i = 0; i < DataHelper.cliMultiple.Count; i++)
                    DataHelper.cliMultiple[i].clients = SqlNew.getAllClients(DataHelper.cliMultiple[i].startDate, DataHelper.cliMultiple[i].endDate);


            }

            return Page();
        }
    }
}
