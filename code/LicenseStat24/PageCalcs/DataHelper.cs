using LicenseStat24.BDRepos;
using LicenseStat24.NewModels;

namespace LicenseStat24.PageCalcs
{
   
    public static class DataHelper
    {
        // список клиентов в диапазоне
        public static ClientEx cliSingle = new ClientEx();

        // список клиентов с подинтервалами (основной диапазон времени делится на недели/месяцы и тп)
        public static List<ClientEx> cliMultiple = new List<ClientEx>();

        public static ClientEx cliAllData = new ClientEx();


        // промежуточные модели для таблиц
        public class ProductTable
        {
            public string Name { get; set; }
            public int Revenue { get; set; }
            public int Sales { get; set; }
            public string RGBColor { get; set; }

        }

        public class DealsTable
        {
            public int DealID { get; set; }
            public string DealNum { get; set; }
            public string CliName { get; set; }
            public int CliId { get; set; }
            public int ContID { get; set; }
            public DateTime DealDate { get; set; }
            public int Revenue { get; set; }

        }

        public class ClientEx
        {
            public DateTime startDate { get; set; }
            public DateTime endDate { get; set; }
            public List<NewModels.Client> clients { get; set; }

        }

        public static string[] bublikColors = new string[] {
    "#6495ED", "#FFA07A", "#20B2AA", "#87CEEB", "#FFD700",
    "#90EE90", "#CD5C5C", "#87CEFA", "#FF69B4", "#00BFFF",
    "#FFA500", "#3CB371", "#D8BFD8", "#B0C4DE", "#00FA9A",
    "#FF6347", "#4682B4", "#00FFFF", "#FF4500", "#87CEEB",
    "#32CD32", "#1E90FF", "#FFD700", "#6A5ACD", "#FF69B4",
    "#87CEFA", "#FFA07A", "#20B2AA", "#6495ED", "#FF6347",
    "#CD5C5C", "#90EE90", "#D8BFD8", "#B0C4DE", "#00FA9A",
    "#FF4500", "#4682B4", "#00FFFF", "#FFA500", "#32CD32",
    "#1E90FF", "#FFD700", "#6A5ACD", "#FF69B4", "#87CEEB",
    "#FF6347", "#CD5C5C", "#90EE90", "#D8BFD8", "#B0C4DE"
};

        public static bool IsValidLicense(License license)
        {
            return license.LicBeginDate > DateTime.MinValue && license.LicEndDate < DateTime.MaxValue;
        }

        // деление диапазона на под диапазоны
        public static List<ClientEx> SplitByMonths(DateTime startDate, DateTime endDate)
        {
            DateTime monthStartDate, monthEndDate;
            List<ClientEx> listDates = new List<ClientEx>();
            monthStartDate = startDate.AddDays(-1 * startDate.Day + 1);
            monthEndDate = monthStartDate.AddMonths(1).AddDays(-1);

            if (monthEndDate < endDate)
            {
                while (monthEndDate < endDate)
                {
                    listDates.Add(new ClientEx { startDate = startDate, endDate = monthEndDate });
                    monthStartDate = monthEndDate.AddDays(1);
                    startDate = monthStartDate;
                    monthEndDate = monthStartDate.AddMonths(1).AddDays(-1);
                }

                listDates.Add(new ClientEx { startDate = monthStartDate, endDate = endDate });
            }
            else
            {
                listDates.Add(new ClientEx { startDate = startDate, endDate = endDate });
            }

            return listDates;
        }

        public static List<ClientEx> SplitByWeeks(DateTime startDate, DateTime endDate)
        {
            DateTime weekStartDate, weekEndDate;
            List<ClientEx> listDates = new List<ClientEx>();
            weekStartDate = startDate.AddDays(-1 * (int)startDate.DayOfWeek);
            weekEndDate = weekStartDate.AddDays(6);

            if (weekEndDate < endDate)
            {
                while (weekEndDate < endDate)
                {
                    listDates.Add(new ClientEx { startDate = startDate, endDate = weekEndDate });
                    weekStartDate = weekEndDate.AddDays(1);
                    startDate = weekStartDate;
                    weekEndDate = weekStartDate.AddDays(6);
                }

                listDates.Add(new ClientEx { startDate = weekStartDate, endDate = endDate });
            }
            else
            {
                listDates.Add(new ClientEx { startDate = startDate, endDate = endDate });
            }

            return listDates;
        }

        public static List<ClientEx> SplitByQuarters(DateTime startDate, DateTime endDate)
        {
            DateTime quarterStartDate, quarterEndDate;
            List<ClientEx> listDates = new List<ClientEx>();
            int currentYear = startDate.Year;
            int startMonth = startDate.Month;

            // Determine the start and end dates for the first quarter
            int currentQuarter = (int)Math.Ceiling(startMonth / 3.0);
            quarterStartDate = new DateTime(currentYear, (currentQuarter - 1) * 3 + 1, 1);
            quarterEndDate = quarterStartDate.AddMonths(2).AddDays(DateTime.DaysInMonth(currentYear, quarterStartDate.Month + 2));

            // Split the date range into quarters
            while (quarterEndDate < endDate)
            {
                listDates.Add(new ClientEx { startDate = startDate, endDate = quarterEndDate });

                // Move to the start of the next quarter
                quarterStartDate = quarterEndDate.AddDays(1);
                startDate = quarterStartDate;

                // Determine the end date for the next quarter
                currentYear = quarterStartDate.Year;
                currentQuarter = (int)Math.Ceiling(quarterStartDate.Month / 3.0);
                quarterEndDate = new DateTime(currentYear, currentQuarter * 3, DateTime.DaysInMonth(currentYear, currentQuarter * 3));
            }

            // Add the last segment if needed
            if (startDate <= endDate)
            {
                listDates.Add(new ClientEx { startDate = startDate, endDate = endDate });
            }

            return listDates;
        }

        public static List<ClientEx> SplitByHalfYears(DateTime startDate, DateTime endDate)
        {
            DateTime halfYearStartDate, halfYearEndDate;
            List<ClientEx> listDates = new List<ClientEx>();
            int currentYear = startDate.Year;
            int startMonth = startDate.Month;

            if (startMonth <= 6)
            {
                halfYearStartDate = new DateTime(currentYear, 1, 1);
                halfYearEndDate = new DateTime(currentYear, 6, 30);
            }
            else
            {
                halfYearStartDate = new DateTime(currentYear, 7, 1);
                halfYearEndDate = new DateTime(currentYear, 12, 31);
            }

            while (halfYearEndDate < endDate)
            {
                listDates.Add(new ClientEx { startDate = startDate, endDate = halfYearEndDate });

                halfYearStartDate = halfYearEndDate.AddDays(1);
                startDate = halfYearStartDate;

                currentYear = halfYearStartDate.Year;
                if (halfYearStartDate.Month <= 6)
                {
                    halfYearEndDate = new DateTime(currentYear, 6, 30);
                }
                else
                {
                    halfYearEndDate = new DateTime(currentYear, 12, 31);
                }
            }

            if (startDate <= endDate)
            {
                listDates.Add(new ClientEx { startDate = startDate, endDate = endDate });
            }

            return listDates;
        }

        public static List<ClientEx> SplitByYears(DateTime startDate, DateTime endDate)
        {
            DateTime yearStartDate, yearEndDate;
            List<ClientEx> listDates = new List<ClientEx>();
            int currentYear = startDate.Year;
            yearStartDate = new DateTime(currentYear, 1, 1);
            yearEndDate = new DateTime(currentYear, 12, 31);

            while (yearEndDate < endDate)
            {
                listDates.Add(new ClientEx { startDate = startDate, endDate = yearEndDate });

                yearStartDate = yearEndDate.AddDays(1);
                startDate = yearStartDate;

                currentYear = yearStartDate.Year;
                yearEndDate = new DateTime(currentYear, 12, 31);
            }

            if (startDate <= endDate)
                listDates.Add(new ClientEx { startDate = startDate, endDate = endDate });


            return listDates;
        }
    }
}
