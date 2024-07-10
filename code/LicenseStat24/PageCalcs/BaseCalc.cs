using LicenseStat24.NewModels;
using static LicenseStat24.PageCalcs.DataHelper;
using License = LicenseStat24.NewModels.License;

namespace LicenseStat24.PageCalcs
{
    public class BaseCalc
    {
        // из List<CientEx> берутся данные и пишутся сюда для удобства
        public List<Contract> allContracts { get; set; }
        public List<Deal> allDeals { get; set; }
        public List<License> allLicenses { get; set; }
        public List<License> licBuy { get; set; }
        public List<License> licRenew { get; set; }

        public List<License> licChange { get; set; }
        public List<License> licExp { get; set; }

        // максимально общая инфа о количестве и выручке продаж/продлений
        public int salesCount { get; set; }
        public int renewsCount { get; set; }
        public int salesRevenue { get; set; }
        public int renewsRevenue { get; set; }

        public int changesCount { get; set; }
        public int expandsCount { get; set; }

        public List<ProductTable> GroupBySales(List<ProductTable> licenses)
        {
            List<ProductTable> returnList = new List<ProductTable>();

            if (licenses.Count > 5)
            {
                var top5Categories = licenses.OrderByDescending(item => item.Sales).Take(5).ToList();
                var restCategories = licenses.Except(top5Categories).ToList();
                returnList.AddRange(top5Categories);
                returnList.Add(new ProductTable
                {
                    Name = "Other",
                    Sales = restCategories.Sum(module => module.Sales),
                    Revenue = restCategories.Sum(module => module.Revenue),
                    RGBColor = "#808080"

                });
            }
            else
            {
                returnList.AddRange(licenses);
            }

            return returnList;
        }

        public List<ProductTable> GroupByRev(List<ProductTable> licenses)
        {
            List<ProductTable> returnList = new List<ProductTable>();

            if (licenses.Count > 5)
            {
                var top5Categories = licenses.OrderByDescending(item => item.Revenue).Take(5).ToList();
                var restCategories = licenses.Except(top5Categories).ToList();
                returnList.AddRange(top5Categories);
                returnList.Add(new ProductTable
                {
                    Name = "Other",
                    Sales = restCategories.Sum(module => module.Sales),
                    Revenue = restCategories.Sum(module => module.Revenue),
                    RGBColor = "#808080"

                });
            }
            else
            {
                returnList.AddRange(licenses);
            }

            return returnList;
        }


    }
}
