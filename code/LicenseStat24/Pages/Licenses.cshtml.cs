using LicenseStat24.BDRepos;
using LicenseStat24.PageCalcs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using System.Text;
using LicenseStat24.NewModels;
using Microsoft.AspNetCore.Authorization;

namespace LicenseStat24.Pages
{
    [Authorize(Roles = "ANALYSIS")]

    public class LicensesModel : PageModel
    {

        [BindProperty]
        public PagePostMod pageMod { get; set; }

        public List<License> allLicesies { get; set; }
        public List<DateRange> dateRanges { get; set; }
        public void OnGet()
        {
            #region getSetDate
            pageMod = new PagePostMod();

            // Получаем данные из сессии
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
            #endregion

            // Получили ВСЕ лицензии из базы
            allLicesies = SqlNew.GetLicenses();

            // Разбиение интервала на подинтервалы
            if (pageMod.modDate == 1)
                dateRanges = DateSplitter.SplitByWeeks(pageMod.startDate, pageMod.endDate);
            else if (pageMod.modDate == 2)
                dateRanges = DateSplitter.SplitByMonths(pageMod.startDate, pageMod.endDate);
            else if (pageMod.modDate == 3)
                dateRanges = DateSplitter.SplitByQuarters(pageMod.startDate, pageMod.endDate);
            else if (pageMod.modDate == 4)
                dateRanges = DateSplitter.SplitByHalfYears(pageMod.startDate, pageMod.endDate);
            else if (pageMod.modDate == 5)
                dateRanges = DateSplitter.SplitByYears(pageMod.startDate, pageMod.endDate);

        }
    }
}
