using LicenseStat24.NewModels;
using System.Collections.Generic;

namespace LicenseStat24.NewModels
{
    public class Contract
    {
        public int ContId { get; set; }

        public int CliId { get; set; }

        public string ContNumber { get; set; } = null;

        public System.DateTime? ContDate { get; set; }

        public List<Deal> ContDeals { get; set; } = new List<Deal>();
    }
}
