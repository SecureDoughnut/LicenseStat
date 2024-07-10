using LicenseStat24.NewModels;
using System.Collections.Generic;
using Client = LicenseStat24.NewModels.Client;
using static LicenseStat24.PageCalcs.DataHelper;

namespace LicenseStat24.PageCalcs
{

    
    public class DealsCalc : BaseCalc
    {
        // начальные списки
        private List<NewModels.Client> clients { get; set; }
       
        // полные списки модулей и конфигов из сделок по продажам
        public List<ProductTable> modSell { get; set; }
        public List<ProductTable> confSell { get; set; }

        // полные списки модулей и конфигов из сделок по продлениям
        public List<ProductTable> modConfRenew { get; set; }

        // количество непродлений
        public double notRenewed { get; set; }
        public List<License> inactiveLicenses = new List<License>();

        // объединение списков modSell и  confSell
        public List<ProductTable> combinedProd { get; set; }

        public DealsCalc(List<NewModels.Client> clients, DateTime start, DateTime end)
        {
            this.clients = clients;
            allDeals = clients.SelectMany(client => client.CliContracts).SelectMany(contract => contract.ContDeals).ToList();
            allLicenses = allDeals.SelectMany(deal => deal.DealLicenses).Where(lic => lic.LicId != -1).ToList();

            inactiveLicenses = allLicenses.Where(license => license.LicEndDate <= end && license.LicNextId == null).ToList();

            notRenewed = inactiveLicenses.Count;
            //notRenewed = ((double)inactiveLicenses.Count / (double)allLicenses.Count) * 100;
            //notRenewed = Math.Round(notRenewed, 3);

            licBuy = allLicenses.Where(license => license.LicType == 1).ToList(); // приобретение
            licRenew = allLicenses.Where(license => license.LicType == 4).ToList(); // продление

            salesCount = licBuy.Count();
            renewsCount = licRenew.Count();

            SellsConfAndMod();
            RenewConfAndMod();

            combinedProd = new List<ProductTable>();
            Combine();
        }

        // Выручка с продаж от модулей и конфигураций
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

            for (int i = 0; i < modSell.Count; i++)
                modSell[i].RGBColor = bublikColors[i];


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

            for (int i = 0; i < confSell.Count; i++)
                confSell[i].RGBColor = bublikColors[i];

            salesRevenue = confSell.Sum(product => product.Revenue) + modSell.Sum(product => product.Revenue);
        }


        // Выручка с продлений от модулей и конфигураций
        void RenewConfAndMod()
        {
            modConfRenew = licRenew
                .SelectMany(license => license.LicConf)
                .GroupBy(configuration => configuration.ConfName)
                .Select(group =>
                {
                    int sales = group.Count();
                    int revenue = 0;

                    switch (group.First().ConfBuyRule) // В зависимости от правила продления (смотри структуру бд)
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

            for (int i = 0; i < modConfRenew.Count; i++)
                modConfRenew[i].RGBColor = bublikColors[i];
            renewsRevenue = modConfRenew.Sum(product => product.Revenue);
        }


        // Группировка круговой диаграммы по количеству


        void Combine()
        {

            foreach (var product in modSell)
            {
                product.Name = "[MOD] " + product.Name;
                combinedProd.Add(product);
            }

            foreach (var product in confSell)
            {
                product.Name = "[CONF] " + product.Name;
                combinedProd.Add(product);
            }

            foreach (var product in modConfRenew)
            {
                product.Name = "[RENEW] " + product.Name;
                combinedProd.Add(product);
            }

            combinedProd = combinedProd.OrderByDescending(s => s.Revenue).ToList();
        }

        // Таблица всех сделок клиентов
        public List<DealsTable> GetDealsTable()
        {
            List<DealsTable> dealTables = new List<DealsTable>();

            foreach (var deal in allDeals)
            {
                DealsTable dealTable = new DealsTable
                {
                    DealID = deal.DealId,
                    DealNum = deal.DealNum,
                    ContID = deal.ContId,
                    DealDate = deal.DealDate,
                    Revenue = 0
                };

                // Получаем имя клиента из списка клиентов
                if (clients.Any(client => client.CliContracts.Any(contract => contract.ContDeals.Contains(deal))))
                {
                    Client client = clients.First(client => client.CliContracts.Any(contract => contract.ContDeals.Contains(deal)));
                    dealTable.CliName = client.CliFullName;
                    dealTable.CliId = client.CliId;

                }

                // Рассчитываем сумму сделки
                if (deal.DealLicenses.Any()) // Если есть лицензии в сделке
                {
                    dealTable.Revenue = deal.DealLicenses.Sum(license =>
                    {
                        if (license.LicType == 1) // Приобретение
                        {
                            return license.LicConf.Sum(configuration => configuration.ConfCost ?? 0) + license.LicMod.SelectMany(modulesLicensy => modulesLicensy.ModL).Sum(module => module.ModCost);
                        }
                        else if (license.LicType == 4) // Продление
                        {
                            int confRenewCost = 0;
                            int modCost = 0;

                            foreach (var conf in license.LicConf)
                            {
                                switch (conf.ConfBuyRule)
                                {
                                    case 1:
                                        confRenewCost += conf.ConfRenewCost ?? 0;
                                        break;
                                    case 2:
                                        confRenewCost += conf.ConfRenewCost ?? 0;
                                        modCost += license.LicMod.SelectMany(modulesLicensy => modulesLicensy.ModL).Sum(module => module.ModCost);
                                        break;
                                    case 3:
                                        modCost += license.LicMod.SelectMany(modulesLicensy => modulesLicensy.ModL)
                                            .Where(module => license.LicConf.Any(configuration => configuration.ConfId == module.ProdId))
                                            .Sum(module => module.ModCost);
                                        break;
                                    default:
                                        break;
                                }
                            }

                            return confRenewCost + modCost;
                        }
                        else
                        {
                            return 0;
                        }
                    });
                }

                dealTables.Add(dealTable);
            }

            return dealTables;
        }


    }
}
