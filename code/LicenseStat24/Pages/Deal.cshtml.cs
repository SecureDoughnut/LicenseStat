using LicenseStat24.NewModels;
using LicenseStat24.PageCalcs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LicenseStat24.Pages
{
    [Authorize(Roles = "ANALYSIS")]
    public class DealModel : PageModel
    {
        [BindProperty]
        public PagePostMod pageMod { get; set; }
        public int CliId { get; set; }
        public int DealId { get; set; }

        public Client client = new Client();

        public async Task<IActionResult> OnGet()
        {
            pageMod = new PagePostMod();
            pageMod.datesVisible = false;

            if (int.TryParse(PageContext.HttpContext.Request.Query["CliId"], out int parsedClient) &&
                int.TryParse(PageContext.HttpContext.Request.Query["DealId"], out int parsedDeal))
            {
                CliId = parsedClient;
                DealId = parsedDeal;

                // База данных со всеми записями за все время
                client = DataHelper.cliAllData.clients.Where(c => c.CliId == CliId).First();
                return Page();

            }
            else
                return BadRequest("Bad client or deal ID");




        }
    }
}
