using LicenseStat24.NewModels;

namespace LicenseStat24.PageCalcs
{
    public class LicenseCalc
    {
        // стуруктура для пончика
        public class LicProduct
        {
            public string Product { get; set; }
            public int Count { get; set; }
            public string RGBColor { get; set; }
        }

        // структура для таблицы лицензий
        public class LicTable
        {
            public int licId { get; set; }
            public DateTime licStart { get; set; }
            public DateTime licEnd { get; set; }
            public string confName { get; set; }
            public string? licComment { get; set; }

        }

        // allLicTypes и allLicTypesForDonut сожержат лицензии, активные в некоторый момент времени
        // в каждом списке находятся лицензии типа 1, 2, 3, 4
       
        public List<List<LicTable>> actLicTypes = new List<List<LicTable>>();
        public List<List<LicProduct>> actLicTypesForDonut = new List<List<LicProduct>>();

        public List<LicProduct> activeLicListGrouped = new List<LicProduct>();
        public List<LicProduct> notActiveLicListGrouped = new List<LicProduct>();
        public List<LicProduct> activeLicListForDonutGrouped = new List<LicProduct>();

        public int activeLicCount = 0;
        public int notActiveLicCount = 0;
        public int nowLicCount = 0;
        

        // Для круговой диаграммы на вкладке "Сейчас"
        public List<LicProduct> nowLicListForDonut = new List<LicProduct>();

        public LicenseCalc(List<License> lics, DateTime startInt, DateTime endInt)
        {

            // Выбрали все лицензии типа замены
            var licensesWithType2 = new HashSet<int>(lics.Where(license => license.LicType == 2).Select(license => license.LicId));


            // Определение активных и непродленных лицензий
            var activeLicensesQuery = lics.Where(license => license.LicEndDate >= startInt && license.LicBeginDate <= endInt && (license.LicNextId == null || !licensesWithType2.Contains((int)license.LicNextId))).ToList();
            var notActiveLicensesQuery = lics.Where(license => license.LicEndDate <= endInt && license.LicNextId == null);
            activeLicCount = activeLicensesQuery.Count();
            notActiveLicCount = notActiveLicensesQuery.Count();


            // Группировка по названию продукта и подсчет количества лицензий
            activeLicListGrouped = activeLicensesQuery.GroupBy(license => license.LicConf[0].ConfName)
                                                 .Select(group => new LicProduct { Product = group.Key, Count = group.Count() })
                                                 .ToList();

            // Задание цвета
            for (int i = 0; i < activeLicListGrouped.Count; i++)
                activeLicListGrouped[i].RGBColor = DataHelper.bublikColors[i];

            // Группировка для круговой диаграммы
            activeLicListForDonutGrouped = GroupBySales(activeLicListGrouped, 10);

            notActiveLicListGrouped = notActiveLicensesQuery.GroupBy(license => license.LicConf[0].ConfName)
                                                       .Select(group => new LicProduct { Product = group.Key, Count = group.Count() })
                                                       .ToList();

            // вкладка Информация по интервалам
            CalcInterval(activeLicensesQuery);

            // вкладка Информация сейчас
            CalcNowData(lics);

        }

        // Активные лицензии на данный момент
        void CalcNowData(List<License> lics)
         {
            // Выбрали
            List<License> nowLicenses = lics.Where(license => license.LicEndDate >= DateTime.Now && license.LicNextId == null).ToList();
            nowLicCount = nowLicenses.Count;

            List<LicTable> table = ToTable(nowLicenses);
            
            // Сгруппировали по продукту
            var prodlist = table.GroupBy(license => license.confName)
                                                .Select(group => new LicProduct { Product = group.Key, Count = group.Count() })
                                                .ToList();

            // Назначили цвета
            for (int k = 0; k < prodlist.Count; k++)
                prodlist[k].RGBColor = DataHelper.bublikColors[k];

            // Отобрали топ 10 для круговой диаграммы и сохранили
            nowLicListForDonut.AddRange(GroupBySales(prodlist, 10).OrderByDescending(c => c.Count).ToList());

          }
        
        void CalcInterval(List<License> lics)
        {
            List<License> tempLics = new List<License>();
            int[] types = { 1, 2, 3, 4 };

            // прошлись по всем типам лицензий
            for (int i = 0; i < types.Length; i++)
            {
                // берем каждый список
                tempLics = lics.Where(license => license.LicType == types[i]).ToList();

                // ТАБЛИЦА перегнали список в таблицу для отображения на странице
                List<LicTable> table = ToTable(tempLics);

                // ПОНЧИК сгруппировали таблицу чтобы сделать тип данных для пончика
                var prodlist = table.GroupBy(license => license.confName)
                                                 .Select(group => new LicProduct { Product = group.Key, Count = group.Count() })
                                                 .ToList();

                // назначили цвета
                for (int k = 0; k < prodlist.Count; k++)
                    prodlist[k].RGBColor = DataHelper.bublikColors[k];

                // добавляем в списки списков
                actLicTypesForDonut.Add(GroupBySales(prodlist, 5).OrderByDescending(c => c.Count).ToList());
                actLicTypes.Add(table);
            }

        }

        List<LicTable> ToTable(List<License> licList)
        {
            List<LicTable> retList = new List<LicTable>(); 
            foreach (License license in licList)
                retList.Add(new LicTable() { 
                    licId = license.LicId, 
                    licStart = license.LicBeginDate, 
                    licEnd = license.LicEndDate, 
                    confName = license.LicConf.FirstOrDefault().ConfName 
                });

            return retList.OrderByDescending(c => c.licStart).ToList();
        }

        // Формирование данных для круговой диаграммы (пончика)
        List<LicProduct> GroupBySales(List<LicProduct> licenses, int top)
        {
            List<LicProduct> returnList = new List<LicProduct>();

            if (licenses.Count > top)
            {
                var top5Categories = licenses.OrderByDescending(item => item.Count).Take(top).ToList();
                var restCategories = licenses.Except(top5Categories).ToList();
                returnList.AddRange(top5Categories);
                returnList.Add(new LicProduct
                {
                    Product = "Other",
                    Count = restCategories.Sum(module => module.Count),
                    RGBColor = "#808080" // Серый цвет

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
