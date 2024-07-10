using LicenseStat24.NewModels;
using System;
using System.Collections.Generic;

namespace LicenseStat24.NewModels
{
    public class Deal
    {
        public int DealId { get; set; }

        public int ContId { get; set; }

        public string DealNum { get; set; }

        public DateTime DealDate { get; set; }

        public List<License> DealLicenses { get; set; } = new List<License>();

    }
}
