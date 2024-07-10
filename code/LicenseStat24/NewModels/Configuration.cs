using LicenseStat24.NewModels;
using System.Collections.Generic;


namespace LicenseStat24.NewModels
{
    public partial class Configuration
    {
        public string ConfName { get; set; } = null;

        public int ConfId { get; set; }

        public int? ConfCost { get; set; }

        public int? ConfBuyRule { get; set; }

        public int? ConfRenewRule { get; set; }

        public int? ConfRenewCost { get; set; }

    }

}
