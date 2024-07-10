using LicenseStat24.NewModels;
using System;
using System.Collections.Generic;


namespace LicenseStat24.NewModels

{
    public partial class ModulesLicensy
    {
        public int ModLicId { get; set; }

        public int LicId { get; set; }

        public int ModId { get; set; }

        public List<Module> ModL { get; set; } = new List<Module>();

    }

}

