
using LicenseStat24.NewModels;
using LicenseStat24.PageCalcs;
using System.Collections.Generic;

namespace LicenseStat24.NewModels
{
    public class Client
    {
        public string CliFullName { get; set; } = null;

        public string CliLegalAddress { get; set; }

        public string CliActualAddress { get; set; }

        public string CliPhoneNumber { get; set; }

        public string CliEmail { get; set; }

        public string CliDirectorName { get; set; }

        public int CliId { get; set; }

        public List<Contract> CliContracts { get; set; } = new List<Contract>();
    }
}
