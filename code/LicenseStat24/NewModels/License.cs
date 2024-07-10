using System;
using System.Collections.Generic;
using System.Configuration;

namespace LicenseStat24.NewModels
{
    public class License
    {

        public int LicId { get; set; }

        public int LicType { get; set; }

        public int? LicPrevId { get; set; }

        public int? LicNextId { get; set; }

        public DateTime LicBeginDate { get; set; }

        public DateTime LicEndDate { get; set; }

        public List<Configuration> LicConf { get; set; } = new List<Configuration>();

        public List<ModulesLicensy> LicMod { get; set; } = new List<ModulesLicensy>();
    }
}
