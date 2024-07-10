namespace LicenseStat24.NewModels
{
    public class PagePostMod
    {
        // используется на всех страницах при пост запросах
        // это основная информация - начало и конец диапазона, делить ли диапазон на поддиапазоны
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public int modDate { get; set; }

        public bool datesVisible = true;


    }
}
