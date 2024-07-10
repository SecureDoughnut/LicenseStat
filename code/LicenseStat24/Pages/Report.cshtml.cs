
using LicenseStat24.BDRepos;
using LicenseStat24.NewModels;
using LicenseStat24.PageCalcs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using System.Text;
using static LicenseStat24.PageCalcs.DataHelper;

namespace LicenseStat24.Pages
{
    [Authorize(Roles = "ANALYSIS")]
    public class ReportModel : PageModel
    {
        public class PagePostReport
        {

            public DateTime startDate { get; set; }
            public DateTime endDate { get; set; }
            public int modDate { get; set; } 
            public bool getCommon { get; set; }
            public bool getDeals { get; set; }
            public bool getClients { get; set; }

        }


        [BindProperty]
        public PagePostMod pageMod { get; set; } // на всех страницах системы должно быть это свойство

        [BindProperty]
        public PagePostReport pageRepMod { get; set; } 

        public ClientEx cliSingleReport = new ClientEx();
        public  List<ClientEx> cliMultipleReport = new List<ClientEx>();
        public async Task<IActionResult> OnGet()
        {
            // инициализация полей, скрываем pageMod если он не нужен
            pageMod = new PagePostMod();
            pageMod.datesVisible = false;
            pageRepMod = new PagePostReport();

            // получение данных из сессии
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

            pageRepMod.startDate = pageMod.startDate;
            pageRepMod.endDate = pageMod.endDate;
            pageRepMod.modDate = pageMod.modDate;
            pageRepMod.getCommon = true;
            pageRepMod.getDeals = true;
            pageRepMod.getClients = true;

            return Page();

        }

        // получаем данные из всех калькуляторов и вставляем их в шаблон, возвращаем пользователю
        public async Task<IActionResult> OnPostAsync()
        {
            byte[] reportBytes = null;
            string htmlReport = System.IO.File.ReadAllText("reportTemplate.html");

            cliSingleReport.startDate = pageRepMod.startDate;
            cliSingleReport.endDate = pageRepMod.endDate;
            cliSingleReport.clients = SqlNew.getAllClients(pageRepMod.startDate, pageRepMod.endDate);

            if (pageRepMod.modDate == 1)
                cliMultipleReport = SplitByWeeks(pageRepMod.startDate, pageRepMod.endDate);
            else if (pageRepMod.modDate == 2)
                cliMultipleReport = SplitByMonths(pageRepMod.startDate, pageRepMod.endDate);
            else if (pageRepMod.modDate == 3)
                cliMultipleReport = SplitByQuarters(pageRepMod.startDate, pageRepMod.endDate);
            else if (pageRepMod.modDate == 4)
                cliMultipleReport = SplitByHalfYears(pageRepMod.startDate, pageRepMod.endDate);
            else if (pageRepMod.modDate == 5)
                cliMultipleReport = SplitByYears(pageRepMod.startDate, pageRepMod.endDate);

            for (int i = 0; i < cliMultipleReport.Count; i++)
                cliMultipleReport[i].clients = SqlNew.getAllClients(cliMultipleReport[i].startDate, cliMultipleReport[i].endDate);


            // common info
            CommonCalc commonSingle = new CommonCalc(cliSingleReport.clients, cliSingleReport.startDate, cliSingleReport.endDate);
            List<CommonCalc> commonMult = new List<CommonCalc>();

            string comMultDates = Newtonsoft.Json.JsonConvert.SerializeObject(cliMultipleReport.Select(x => x.startDate.ToShortDateString()).ToList());

            foreach (ClientEx item in cliMultipleReport)
                commonMult.Add(new CommonCalc(item.clients, item.startDate, item.endDate));

            string comMultDeploys = Newtonsoft.Json.JsonConvert.SerializeObject(commonMult.Select(c => c.deploymentsCount));
            string comMultSalesRev = Newtonsoft.Json.JsonConvert.SerializeObject(commonMult.Select(c => c.salesRevenue));
            string comMulltRenewRev = Newtonsoft.Json.JsonConvert.SerializeObject(commonMult.Select(c => c.renewsRevenue));

            htmlReport = htmlReport.Replace("[date1]", cliSingleReport.startDate.ToShortDateString()).Replace("[date2]", cliSingleReport.endDate.ToShortDateString());
            htmlReport = htmlReport.Replace("[deploy]", commonSingle.deploymentsCount.ToString()).Replace("[totalrev]", String.Format("{0:n0}", commonSingle.salesRevenue + commonSingle.renewsRevenue));
            htmlReport = htmlReport.Replace("[comMultDates]", comMultDates).Replace("[comMultDeploys]", comMultDeploys).Replace("[comMultSalesRev]", comMultSalesRev).Replace("[comMulltRenewRev]", comMulltRenewRev);
            htmlReport = htmlReport.Replace("[comMultClients]", Newtonsoft.Json.JsonConvert.SerializeObject(commonMult.Select(c => c.newClients.Count)));
            htmlReport = htmlReport.Replace("[cliNew]", Newtonsoft.Json.JsonConvert.SerializeObject(commonMult.Select(c => c.newClients.Count).Sum()));

            // deals
            DealsCalc dealsSingle = new DealsCalc(cliSingleReport.clients, cliSingleReport.startDate, cliSingleReport.endDate);

            // сгруппированы для бубликов
            var dealsConfSellsCount = dealsSingle.GroupBySales(dealsSingle.confSell);
            var dealsConfSellsRev = dealsSingle.GroupByRev(dealsSingle.confSell);
            var dealsModSellsCount = dealsSingle.GroupBySales(dealsSingle.modSell);
            var dealsModSellsRev = dealsSingle.GroupByRev(dealsSingle.modSell);
            var dealsAllRenewCount = dealsSingle.GroupBySales(dealsSingle.modConfRenew);
            var dealsAllRenewRev = dealsSingle.GroupByRev(dealsSingle.modConfRenew);

            htmlReport = htmlReport.Replace("[dealsRevSales]", String.Format("{0:n0}", dealsSingle.confSell.Sum(product => product.Revenue) + dealsSingle.modSell.Sum(product => product.Revenue)));
            htmlReport = htmlReport.Replace("[dealsRevRenews]", String.Format("{0:n0}", dealsSingle.modConfRenew.Sum(product => product.Revenue)));
            htmlReport = htmlReport.Replace("[dealsRevTotal]", String.Format("{0:n0}", dealsSingle.confSell.Sum(product => product.Revenue) + dealsSingle.modSell.Sum(product => product.Revenue) + dealsSingle.modConfRenew.Sum(product => product.Revenue)));
            htmlReport = htmlReport.Replace("[dealsNotRenewed]", dealsSingle.notRenewed.ToString());

            // топ продуктов по выручке (продажи и продления)
            string topTableRev = "";
            foreach (ProductTable prod in dealsSingle.combinedProd.OrderByDescending(r => r.Revenue).Take(10).ToList())
            {
                topTableRev += "<tr>" + Environment.NewLine;
                topTableRev += "<td>" + prod.Name.ToString() + "</td>" + Environment.NewLine;
                topTableRev += "<td>" + prod.Sales.ToString() + "</td>" + Environment.NewLine;
                topTableRev += "<td>" + prod.Revenue.ToString() + "</td>" + Environment.NewLine;
                topTableRev += "</tr>" + Environment.NewLine;
            }
            htmlReport = htmlReport.Replace("[topTableRev]", topTableRev);

            htmlReport = htmlReport.Replace("[dealsSalesCount]", dealsSingle.salesCount.ToString());
            htmlReport = htmlReport.Replace("[dealsRenewCount]", dealsSingle.renewsCount.ToString());
            htmlReport = htmlReport.Replace("[dealsSalesRev]", dealsSingle.salesRevenue.ToString());
            htmlReport = htmlReport.Replace("[dealsRenewRev]", dealsSingle.renewsRevenue.ToString());

            // ПРОДАЖИ модули и конфигурации
            htmlReport = htmlReport.Replace("[dealsConfRev]", String.Format("{0:n0}", dealsSingle.confSell.Sum(product => product.Revenue)));
            htmlReport = htmlReport.Replace("[dealsModRev]", String.Format("{0:n0}", dealsSingle.modSell.Sum(product => product.Revenue)));
            htmlReport = htmlReport.Replace("[dealsSalesRev2]", String.Format("{0:n0}", dealsSingle.confSell.Sum(product => product.Revenue) + dealsSingle.modSell.Sum(product => product.Revenue)));
            htmlReport = htmlReport.Replace("[dealsSalesCount]", dealsSingle.confSell.Sum(product => product.Sales).ToString());

            // ПРОДЛЕНИЯ модули и конфигурации
            htmlReport = htmlReport.Replace("[renewRev]", String.Format("{0:n0}", dealsSingle.renewsRevenue));
            htmlReport = htmlReport.Replace("[renewCount]", String.Format("{0:n0}", dealsSingle.renewsCount));

            // ЛИЦЕНЗИИ
            List<License> licenses  = SqlNew.GetLicenses();
            LicenseCalc licenseCalc = new LicenseCalc(licenses, pageRepMod.startDate, pageRepMod.endDate);

            List<LicenseCalc> multLic = new List<LicenseCalc>();
            foreach (ClientEx item in cliMultipleReport)
            {
                multLic.Add(new LicenseCalc(licenses, item.startDate, item.startDate));
            }

            htmlReport = htmlReport.Replace("[activeLicListGrouped]", licenseCalc.activeLicListGrouped.Sum(s => s.Count).ToString());

            // топ лицензий
            string positionsTable = "";
            foreach (var prod in licenseCalc.activeLicListForDonutGrouped.OrderByDescending(c => c.Count).ToList())
            {
                positionsTable += "<tr>" + Environment.NewLine;
                positionsTable += "<td>" + prod.Product.ToString() + "</td>" + Environment.NewLine;
                positionsTable += "<td>" + prod.Count.ToString() + "</td>" + Environment.NewLine;
                positionsTable += "</tr>" + Environment.NewLine;
            }
            htmlReport = htmlReport.Replace("[positionsTable]", positionsTable);

            // конец первой страницы
            htmlReport = htmlReport.Replace("[activeLicListForDonutGroupedProd]", Newtonsoft.Json.JsonConvert.SerializeObject(licenseCalc.activeLicListForDonutGrouped.Select(x => x.Product).ToList()));
            htmlReport = htmlReport.Replace("[activeLicListForDonutGroupedCount]", Newtonsoft.Json.JsonConvert.SerializeObject(licenseCalc.activeLicListForDonutGrouped.Select(x => x.Count).ToList()));
            htmlReport = htmlReport.Replace("[activeLicListForDonutGroupedRgb]", Newtonsoft.Json.JsonConvert.SerializeObject(licenseCalc.activeLicListForDonutGrouped.Select(x => x.RGBColor).ToList()));

            // ЛИЦЕНЗИИ ПО ИНТЕРВАЛАМ
            htmlReport = htmlReport.Replace("[activeLicCount]", Newtonsoft.Json.JsonConvert.SerializeObject(multLic.Select(s => s.activeLicCount)));
            htmlReport = htmlReport.Replace("[actLicBuy]", Newtonsoft.Json.JsonConvert.SerializeObject(multLic.Select(s => s.actLicTypes[0].Count())));
            htmlReport = htmlReport.Replace("[notActiveLicCount]", Newtonsoft.Json.JsonConvert.SerializeObject(multLic.Select(s => s.notActiveLicCount)));

            htmlReport = htmlReport.Replace("[changeChart]", Newtonsoft.Json.JsonConvert.SerializeObject(multLic.Select(s => s.actLicTypes[1].Count())));
            htmlReport = htmlReport.Replace("[extChart]", Newtonsoft.Json.JsonConvert.SerializeObject(multLic.Select(s => s.actLicTypes[2].Count())));
            htmlReport = htmlReport.Replace("[renChart]", Newtonsoft.Json.JsonConvert.SerializeObject(multLic.Select(s => s.actLicTypes[3].Count())));


            // КЛИЕНТЫ
            List<ClientCalc> deadClients = new List<ClientCalc>();
            List<ClientCalc> activeClients = new List<ClientCalc>();
            List<ClientCalc> clients = new List<ClientCalc>();

            foreach (var client in cliSingleReport.clients)
                clients.Add(new ClientCalc(client));

            activeClients = clients.Where(client => client.allLicenses.Any(license => IsValidLicense(license) && license.LicEndDate >= pageRepMod.endDate)).ToList();
            deadClients = clients.Except(activeClients).ToList();

            double avgSellRevenue = 0;
            double avgRenewRevenue = 0;

            if (clients.Count != 0)
            {
                // все продажи и продления клиентам не равные нулю
                var allClientsSales = clients.Where(client => client.salesRevenue != 0);
                var allClientsRenews = clients.Where(client => client.renewsRevenue != 0);
                // средние значения по клиентам на картах
                avgSellRevenue = allClientsSales.Count() != 0 ? allClientsSales.Sum(client => client.salesRevenue) / allClientsSales.Count() : 0;
                avgRenewRevenue = allClientsSales.Count() != 0 ? allClientsRenews.Sum(client => client.renewsRevenue) / allClientsRenews.Count() : 0;

            }


            htmlReport = htmlReport.Replace("[cliActive]", activeClients.Count.ToString());
            htmlReport = htmlReport.Replace("[cliDead]", deadClients.Count.ToString());
            htmlReport = htmlReport.Replace("[cliAvgSellRevenue]", String.Format("{0:n0}", avgSellRevenue));
            htmlReport = htmlReport.Replace("[cliAvgRenewRevenue]", String.Format("{0:n0}", avgRenewRevenue));

            // ТОП АКТИВНЫХ по выручке
            // топ лицензий
            string cliTable = "";
            foreach (ClientCalc cCalc in activeClients.OrderByDescending(item => item.salesRevenue).Take(10).ToList())
            {
                cliTable += "<tr>" + Environment.NewLine;
                cliTable += "<td>" +  cCalc.thisClient.CliFullName + "</td>" + Environment.NewLine;
                cliTable += "<td>" + cCalc.thisClient.CliEmail + "</td>" + Environment.NewLine;
                cliTable += "<td>" + cCalc.thisClient.CliPhoneNumber + "</td>" + Environment.NewLine;
                cliTable += "<td>" + cCalc.thisClient.CliDirectorName + "</td>" + Environment.NewLine;
                cliTable += "<td>" + cCalc.salesRevenue + "</td>" + Environment.NewLine;
                cliTable += "<td>" + cCalc.renewsRevenue + "</td>" + Environment.NewLine;
                cliTable += "</tr>" + Environment.NewLine;
            }
            htmlReport = htmlReport.Replace("[cliTable]", cliTable);


            // report creation

            reportBytes = Encoding.UTF8.GetBytes(htmlReport);

            return File(reportBytes, "text/html", "REPORT-" + cliSingleReport.startDate.ToShortDateString() + "-" + cliSingleReport.endDate.ToShortDateString() + ".html");
        }


    }
}
