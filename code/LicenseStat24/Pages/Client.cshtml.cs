using LicenseStat24.NewModels;
using LicenseStat24.PageCalcs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LicenseStat24.Pages
{
    [Authorize(Roles = "ANALYSIS")]
    public class ClientModel : PageModel
    {
        [BindProperty]
        public PagePostMod pageMod { get; set; }

        public int CliId = 16;

        public Client client = new Client();

        public async Task<IActionResult> OnGet()
        {
            pageMod = new PagePostMod();
            pageMod.datesVisible = false;

            string cliIdString = PageContext.HttpContext.Request.Query["CliId"];
            if (!int.TryParse(cliIdString, out int CliId))
                return BadRequest("Invalid client ID");

            client = DataHelper.cliAllData.clients.FirstOrDefault(c => c.CliId == CliId);
            if (client == null)
                return NotFound("Client not found");

            return Page();
        }

    }
}
