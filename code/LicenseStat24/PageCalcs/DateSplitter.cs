namespace LicenseStat24.PageCalcs
{
    public class DateRange
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class DateSplitter
    {
        public static List<DateRange> SplitByMonths(DateTime startDate, DateTime endDate)
        {
            List<DateRange> listDates = new List<DateRange>();
            DateTime monthStartDate = new DateTime(startDate.Year, startDate.Month, 1);
            DateTime monthEndDate = monthStartDate.AddMonths(1).AddDays(-1);

            while (monthEndDate <= endDate)
            {
                listDates.Add(new DateRange { StartDate = monthStartDate, EndDate = monthEndDate });
                monthStartDate = monthEndDate.AddDays(1);
                monthEndDate = monthStartDate.AddMonths(1).AddDays(-1);
            }

            if (startDate <= endDate && !listDates.Exists(d => d.StartDate == startDate))
                listDates.Add(new DateRange { StartDate = startDate, EndDate = endDate });


            return listDates;
        }

        public static List<DateRange> SplitByWeeks(DateTime startDate, DateTime endDate)
        {
            List<DateRange> listDates = new List<DateRange>();
            DateTime weekStartDate = startDate.AddDays(-1 * (int)startDate.DayOfWeek);
            DateTime weekEndDate = weekStartDate.AddDays(6);

            while (weekEndDate <= endDate)
            {
                listDates.Add(new DateRange { StartDate = weekStartDate, EndDate = weekEndDate });
                weekStartDate = weekEndDate.AddDays(1);
                weekEndDate = weekStartDate.AddDays(6);
            }

            if (startDate <= endDate && !listDates.Exists(d => d.StartDate == startDate))
                listDates.Add(new DateRange { StartDate = startDate, EndDate = endDate });


            return listDates;
        }

        public static List<DateRange> SplitByQuarters(DateTime startDate, DateTime endDate)
        {
            List<DateRange> listDates = new List<DateRange>();
            int currentYear = startDate.Year;
            int startMonth = startDate.Month;

            int currentQuarter = (int)Math.Ceiling(startMonth / 3.0);
            DateTime quarterStartDate = new DateTime(currentYear, (currentQuarter - 1) * 3 + 1, 1);
            DateTime quarterEndDate = quarterStartDate.AddMonths(2).AddDays(DateTime.DaysInMonth(currentYear, quarterStartDate.Month + 2));

            while (quarterEndDate <= endDate)
            {
                listDates.Add(new DateRange { StartDate = quarterStartDate, EndDate = quarterEndDate });

                quarterStartDate = quarterEndDate.AddDays(1);
                currentYear = quarterStartDate.Year;
                currentQuarter = (int)Math.Ceiling(quarterStartDate.Month / 3.0);
                quarterEndDate = new DateTime(currentYear, currentQuarter * 3, DateTime.DaysInMonth(currentYear, currentQuarter * 3));
            }

            if (startDate <= endDate && !listDates.Exists(d => d.StartDate == startDate))
                listDates.Add(new DateRange { StartDate = startDate, EndDate = endDate });


            return listDates;
        }

        public static List<DateRange> SplitByHalfYears(DateTime startDate, DateTime endDate)
        {
            List<DateRange> listDates = new List<DateRange>();
            int currentYear = startDate.Year;
            int startMonth = startDate.Month;

            DateTime halfYearStartDate, halfYearEndDate;
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

            while (halfYearEndDate <= endDate)
            {
                listDates.Add(new DateRange { StartDate = halfYearStartDate, EndDate = halfYearEndDate });

                halfYearStartDate = halfYearEndDate.AddDays(1);
                currentYear = halfYearStartDate.Year;
                if (halfYearStartDate.Month <= 6)
                    halfYearEndDate = new DateTime(currentYear, 6, 30);

                else
                    halfYearEndDate = new DateTime(currentYear, 12, 31);

            }

            if (startDate <= endDate && !listDates.Exists(d => d.StartDate == startDate))
                listDates.Add(new DateRange { StartDate = startDate, EndDate = endDate });


            return listDates;
        }

        public static List<DateRange> SplitByYears(DateTime startDate, DateTime endDate)
        {
            List<DateRange> listDates = new List<DateRange>();
            int currentYear = startDate.Year;

            DateTime yearStartDate = new DateTime(currentYear, 1, 1);
            DateTime yearEndDate = new DateTime(currentYear, 12, 31);

            while (yearEndDate <= endDate)
            {
                listDates.Add(new DateRange { StartDate = yearStartDate, EndDate = yearEndDate });

                yearStartDate = yearEndDate.AddDays(1);
                currentYear = yearStartDate.Year;
                yearEndDate = new DateTime(currentYear, 12, 31);
            }

            if (startDate <= endDate && !listDates.Exists(d => d.StartDate == startDate))
                listDates.Add(new DateRange { StartDate = startDate, EndDate = endDate });


            return listDates;
        }
    }

}
