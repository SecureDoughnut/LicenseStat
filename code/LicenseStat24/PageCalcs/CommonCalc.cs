using LicenseStat24.BDRepos;
using LicenseStat24.NewModels;
using NuGet.Packaging;
using static LicenseStat24.PageCalcs.DataHelper;
using static LicenseStat24.Pages.CommonModel;
using static LicenseStat24.Pages.DealsModel;
using Client = LicenseStat24.NewModels.Client;

namespace LicenseStat24.PageCalcs
{
    public class CommonCalc : BaseCalc
    {
        // полные списки модулей и конфигов из сделок по продажам
        public List<ProductTable> modSell { get; set; }
        public List<ProductTable> confSell { get; set; }
        public List<ProductTable> modConfRenew { get; set; }

        public int deploymentsCount = 0;
        public int dealsCount = 0;
        public List<NewModels.Client> newClients = new List<Client>();


        public CommonCalc(List<NewModels.Client> clients, DateTime start, DateTime end)
        {
            // количество внедрений берется по диапазону внедрений
            deploymentsCount = SqlNew.GetDeploymets(start, end).Count();

            // часто используемое
            allContracts = clients.SelectMany(client => client.CliContracts).ToList();
            allDeals = allContracts.SelectMany(contract => contract.ContDeals).ToList();
            allLicenses = allDeals.SelectMany(deal => deal.DealLicenses).Where(lic => lic.LicId != -1).ToList();
            licBuy = allLicenses.Where(license => license.LicType == 1).ToList(); // приобретение
            licRenew = allLicenses.Where(license => license.LicType == 4).ToList(); // продление


            dealsCount = licBuy.Count();

            SellsConfAndMod();
            RenewConfAndMod();

            // определение новых клиентов
            newClients = cliAllData.clients.Where(c => c.CliContracts.Count == 1 && c.CliContracts.All(co => co.ContDate >= start && co.ContDate <= end)).ToList();
        }

        void SellsConfAndMod()
        {
            modSell = licBuy
            .SelectMany(license => license.LicMod)
            .SelectMany(modulesLicensy => modulesLicensy.ModL)
            .GroupBy(module => module.ModName)
            .Select(group => new ProductTable
            {
                Name = group.Key,
                Sales = group.Count(),
                Revenue = group.Sum(module => module.ModCost)
            })
            .ToList();



            confSell = licBuy
           .SelectMany(license => license.LicConf)
           .GroupBy(configuration => configuration.ConfName)
           .Select(group => new ProductTable
           {
               Name = group.Key,
               Sales = group.Count(),
               Revenue = group.Sum(configuration => configuration.ConfCost ?? 0)
           })
           .ToList();


            salesRevenue = confSell.Sum(product => product.Revenue) + modSell.Sum(product => product.Revenue);
        }

        void RenewConfAndMod()
        {
            modConfRenew = licRenew
                .SelectMany(license => license.LicConf)
                .GroupBy(configuration => configuration.ConfName)
                .Select(group =>
                {
                    int sales = group.Count();
                    int revenue = 0;

                    switch (group.First().ConfBuyRule)
                    {
                        case 1:
                            revenue = group.Sum(configuration => configuration.ConfRenewCost ?? 0);
                            break;
                        case 2:
                            revenue = group.Sum(configuration => configuration.ConfRenewCost ?? 0)
                                       + licRenew
                                            .SelectMany(license => license.LicMod)
                                            .SelectMany(modulesLicensy => modulesLicensy.ModL)
                                            .Sum(module => module.ModCost);
                            break;
                        case 3:
                            revenue = licRenew
                                        .SelectMany(license => license.LicMod)
                                        .SelectMany(modulesLicensy => modulesLicensy.ModL)
                                        .Where(module => group.Any(configuration => configuration.ConfId == module.ProdId))
                                        .Sum(module => module.ModCost);
                            break;
                        default:
                            break;
                    }

                    return new ProductTable
                    {
                        Name = group.Key,
                        Sales = sales,
                        Revenue = revenue
                    };
                })
                .ToList();

            renewsRevenue = modConfRenew.Sum(product => product.Revenue);
        }

    }
}
