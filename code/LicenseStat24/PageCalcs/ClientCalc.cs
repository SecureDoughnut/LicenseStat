using LicenseStat24.NewModels;
using Microsoft.CodeAnalysis.FlowAnalysis;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static LicenseStat24.PageCalcs.DataHelper;
using License = LicenseStat24.NewModels.License;

namespace LicenseStat24.PageCalcs
{

    public class ClientCalc : BaseCalc
    {

        // все сделки клиента, в дальнейшем преобразуются в другой тип чтобы показать на представлении
        public List<Deal> cliDeal = new List<Deal>();
        // в вот этот тип
        public List<DealsTable> cliDealConverted { get; set; }


        // активный или ушедший клиент, а также вляется ли он новым
        public bool cliActive { get; set; }
        public int cliLifetime { get; set; }
        public bool cliNew { get; set; }

        // сделки с именем продукта, количеством продукта и количеством выручки с каждого продукта
        public List<ProductTable> tableRenews { get; set; }
        public List<ProductTable> tableConfSales { get; set; }
        public List<ProductTable> tableModSales { get; set; }

        public List<ProductTable> tableConfExpands { get; set; }
        public List<ProductTable> tableModExpands { get; set; }

        public List<ProductTable> tableConfChanges { get; set; }
        public List<ProductTable> tableModChanges { get; set; }


        // то что выше но оной таблицей
        public List<ProductTable> combinedProd { get; set; }
        public double averageCheck { get; set; }
        public Client thisClient { get; set; }

        public ClientCalc(NewModels.Client client, int? dealID = null)
        {
            thisClient = client;

            if (dealID != null)
            {
                cliDeal.Add(client.CliContracts.SelectMany(deal => deal.ContDeals.Where(d => d.DealId == (int)dealID)).ToList()[0]);
                allLicenses = cliDeal.SelectMany(deal => deal.DealLicenses).Where(lic => lic.LicId != -1).ToList();
                licBuy = cliDeal[0].DealLicenses.Where(l => l.LicType == 1).ToList();
                licRenew = cliDeal[0].DealLicenses.Where(l => l.LicType == 4).ToList();

                licChange = cliDeal[0].DealLicenses.Where(l => l.LicType == 2).ToList();
                licExp = cliDeal[0].DealLicenses.Where(l => l.LicType == 3).ToList();
            }
            else
            {
                cliDeal = client.CliContracts.SelectMany(contract => contract.ContDeals).ToList();
                allLicenses = cliDeal.SelectMany(deal => deal.DealLicenses).Where(lic => lic.LicId != -1).ToList();
                licBuy = allLicenses.Where(license => license.LicType == 1).ToList(); // приобретение
                licRenew = allLicenses.Where(license => license.LicType == 4).ToList(); // продление

                licChange = cliDeal[0].DealLicenses.Where(l => l.LicType == 2).ToList(); // замена
                licExp = cliDeal[0].DealLicenses.Where(l => l.LicType == 3).ToList(); // расширение
            }

            // расчет информации о конфигурациях и модулях
            SellsConfAndMod();
            RenewConfAndMod();
            ChangesConfAndMod();
            ExpandsConfAndMod();

            salesCount = licBuy.Count;
            renewsCount = licRenew.Count;
            changesCount = licChange.Count;
            expandsCount = licExp.Count;

            // слили все таблицы в одну для отображении на представлении
            Combine();

            cliDealConverted = GetDealsTable().OrderByDescending(d => d.DealDate).ToList();
            AverageCheckConfAndMod();

            IsActive();


        }


        // Метод для проверки корректности лицензии
        private bool IsValidLicense(License license)
        {
            return license.LicBeginDate > DateTime.MinValue && license.LicEndDate < DateTime.MaxValue;
        }

        void IsActive()
        {
            cliActive = allLicenses.Any(license => IsValidLicense(license) && license.LicEndDate >= DateTime.Now);
        }

        // ПЛОХАЯ ФУНКЦИЯ
        // средний чек по всем лицензиям и продлениям клиента
        void AverageCheckConfAndMod()
        {
            if (cliDealConverted.Count > 0)
            {
                averageCheck = cliDealConverted.Sum(r => r.Revenue) / cliDealConverted.Count;
            }
            else
                averageCheck = 0;

        }

        // Расчеты для круговых диаграмм

        void ChangesConfAndMod()
        {
            tableModChanges = licChange
           .SelectMany(license => license.LicMod)
           .SelectMany(modulesLicensy => modulesLicensy.ModL)
           .GroupBy(module => module.ModName)
           .Select(group => new ProductTable
           {
               Name = group.Key,
               Sales = group.Count(),
               Revenue = 0
           })
           .ToList();

            tableConfChanges = licChange
           .SelectMany(license => license.LicConf)
           .GroupBy(configuration => configuration.ConfName)
           .Select(group => new ProductTable
           {
               Name = group.Key,
               Sales = group.Count(),
               Revenue = 0
           })
           .ToList();

        }

        void ExpandsConfAndMod()
        {
            tableModExpands = licExp
           .SelectMany(license => license.LicMod)
           .SelectMany(modulesLicensy => modulesLicensy.ModL)
           .GroupBy(module => module.ModName)
           .Select(group => new ProductTable
           {
               Name = group.Key,
               Sales = group.Count(),
               Revenue = 0
           })
           .ToList();

            tableConfExpands = licExp
           .SelectMany(license => license.LicConf)
           .GroupBy(configuration => configuration.ConfName)
           .Select(group => new ProductTable
           {
               Name = group.Key,
               Sales = group.Count(),
               Revenue = 0
           })
           .ToList();

        }
        void SellsConfAndMod()
        {
            tableModSales = licBuy
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

            for (int i = 0; i < tableModSales.Count; i++)
                tableModSales[i].RGBColor = bublikColors[i];



            tableConfSales = licBuy
           .SelectMany(license => license.LicConf)
           .GroupBy(configuration => configuration.ConfName)
           .Select(group => new ProductTable
           {
               Name = group.Key,
               Sales = group.Count(),
               Revenue = group.Sum(configuration => configuration.ConfCost ?? 0)
           })
           .ToList();

            for (int i = 0; i < tableConfSales.Count; i++)
                tableConfSales[i].RGBColor = bublikColors[i];

            salesRevenue = tableConfSales.Sum(product => product.Revenue) + tableModSales.Sum(product => product.Revenue);
        }
        void RenewConfAndMod()
        {
            tableRenews = licRenew
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

            for (int i = 0; i < tableRenews.Count; i++)
                tableRenews[i].RGBColor = bublikColors[i];

            renewsRevenue = tableRenews.Sum(product => product.Revenue);
        }

        // перевод сырых сделок в новый тип, тут в зависимости от типа лицензий берутся цены из таблицы конфигураций
        List<DealsTable> GetDealsTable()
        {
            List<DealsTable> dealTables = new List<DealsTable>();

            foreach (var deal in cliDeal.Where(s => s.DealLicenses.Any() && s.DealLicenses.First<License>().LicId != -1))
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
                if (thisClient.CliContracts.Any(contract => contract.ContDeals.Contains(deal)))
                {
                    dealTable.CliName = thisClient.CliFullName;
                    dealTable.CliId = thisClient.CliId;

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

        // сливаем все таблицы в одну чтобы показать на представлении
        void Combine()
        {
            combinedProd = new List<ProductTable>();

            foreach (var product in this.tableModSales)
            {
                product.Name = "[MOD] " + product.Name;
                combinedProd.Add(product);
            }

            foreach (var product in this.tableConfSales)
            {
                product.Name = "[CONF] " + product.Name;
                combinedProd.Add(product);
            }

            foreach (var product in this.tableRenews)
            {
                product.Name = "[RENEW] " + product.Name;
                combinedProd.Add(product);
            }
        }

       

    }
}
