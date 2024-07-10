using LicenseStat24.NewModels;
using Microsoft.Data.SqlClient;

namespace LicenseStat24.BDRepos
{
    public class SqlNew
    {
        // задается при старте
        public static string  dataConnectionString = null;
        
        // жадныe запросы на получение информации (это ужасно)
        public static List<Client> getAllClients(DateTime start, DateTime end)
        {

            List<Client> clients = new List<Client>();

            using (SqlConnection connection = new SqlConnection(dataConnectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(
                "SELECT " +
                "   c.CLI_ID, " +
                "   c.CLI_FULL_NAME, " +
                "   c.CLI_LEGAL_ADDRESS, " +
                "   c.CLI_ACTUAL_ADDRESS, " +
                "   c.CLI_PHONE_NUMBER, " +
                "   c.CLI_EMAIL, " +
                "   c.CLI_DIRECTOR_NAME, " +
                "   co.CONT_ID, " +
                "   co.CONT_NUMBER, " +
                "   co.CONT_DATE, " +
                "   d.DEAL_ID, " +
                "   d.DEAL_DATE, " +
                "   d.DEAL_NUMBER, " +
                "   l.LIC_ID, " +
                "   l.LIC_TYPE, " +
                "   l.LIC_NEXT_ID, " +
                "   l.LIC_PREV_ID, " +
                "   l.LIC_BEGIN_DATE, " +
                "   l.LIC_END_DATE, " +
                "   cf.CONF_ID, " +
                "   cf.CONF_NAME, " +
                "   cf.CONF_COST, " +
                "   cf.CONF_RENEW_COST, " +
                "   cf.CONF_BUY_RULE, " +
                "   cf.CONF_RENEW_RULE, " +
                "   m.MOD_ID, " +
                "   m.MOD_NAME, " +
                "   m.MOD_COST, " +
                "   ml.MOD_LIC_ID " +
                "FROM dbo.CLIENT c " +
                "LEFT JOIN dbo.CONTRACT co ON c.CLI_ID = co.CLI_ID " +
                "LEFT JOIN dbo.DEAL d ON co.CONT_ID = d.CONT_ID " +
                "LEFT JOIN dbo.LICENSE l ON d.DEAL_ID = l.DEAL_ID  " +
                "LEFT JOIN dbo.CONFIGURATION cf ON l.CONF_ID = cf.CONF_ID " +
                "LEFT JOIN dbo.MODULES_LICENSIES ml ON l.LIC_ID = ml.LIC_ID " +
                "LEFT JOIN dbo.MODULE m ON ml.MOD_ID = m.MOD_ID " +
                "WHERE d.DEAL_DATE >= @start AND d.DEAL_DATE <= @end",
                connection
            );

                command.Parameters.AddWithValue("@start", start);
                command.Parameters.AddWithValue("@end", end);
                command.CommandTimeout = 120;

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int clientId = reader.IsDBNull(reader.GetOrdinal("CLI_ID")) ? -1 : reader.GetInt32(reader.GetOrdinal("CLI_ID"));

                        Client existingClient = clients.FirstOrDefault(c => c.CliId == clientId);

                        if (existingClient == null)
                        {
                            existingClient = new Client
                            {
                                CliId = clientId,
                                CliFullName = reader.IsDBNull(reader.GetOrdinal("CLI_FULL_NAME")) ? null : reader.GetString(reader.GetOrdinal("CLI_FULL_NAME")),
                                CliLegalAddress = reader.IsDBNull(reader.GetOrdinal("CLI_LEGAL_ADDRESS")) ? null : reader.GetString(reader.GetOrdinal("CLI_LEGAL_ADDRESS")),
                                CliActualAddress = reader.IsDBNull(reader.GetOrdinal("CLI_ACTUAL_ADDRESS")) ? null : reader.GetString(reader.GetOrdinal("CLI_ACTUAL_ADDRESS")),
                                CliPhoneNumber = reader.IsDBNull(reader.GetOrdinal("CLI_PHONE_NUMBER")) ? null : reader.GetString(reader.GetOrdinal("CLI_PHONE_NUMBER")),
                                CliEmail = reader.IsDBNull(reader.GetOrdinal("CLI_EMAIL")) ? null : reader.GetString(reader.GetOrdinal("CLI_EMAIL")),
                                CliDirectorName = reader.IsDBNull(reader.GetOrdinal("CLI_DIRECTOR_NAME")) ? null : reader.GetString(reader.GetOrdinal("CLI_DIRECTOR_NAME")),
                                CliContracts = new List<Contract>()
                            };

                            clients.Add(existingClient);
                        }

                        int contractId = reader.IsDBNull(reader.GetOrdinal("CONT_ID")) ? -1 : reader.GetInt32(reader.GetOrdinal("CONT_ID"));

                        Contract existingContract = existingClient.CliContracts.FirstOrDefault(co => co.ContId == contractId);

                        if (existingContract == null)
                        {
                            existingContract = new Contract
                            {
                                ContId = contractId,
                                CliId = clientId,
                                ContNumber = reader.IsDBNull(reader.GetOrdinal("CONT_NUMBER")) ? null : reader.GetString(reader.GetOrdinal("CONT_NUMBER")),
                                ContDate = reader.IsDBNull(reader.GetOrdinal("CONT_DATE")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("CONT_DATE")),
                                ContDeals = new List<Deal>()
                            };

                            existingClient.CliContracts.Add(existingContract);
                        }

                        int dealId = reader.IsDBNull(reader.GetOrdinal("DEAL_ID")) ? -1 : reader.GetInt32(reader.GetOrdinal("DEAL_ID"));

                        Deal existingDeal = existingContract.ContDeals.FirstOrDefault(d => d.DealId == dealId);

                        if (existingDeal == null)
                        {
                            existingDeal = new Deal
                            {
                                DealId = dealId,
                                ContId = contractId,
                                DealDate = reader.IsDBNull(reader.GetOrdinal("DEAL_DATE")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("DEAL_DATE")),
                                DealNum = reader.IsDBNull(reader.GetOrdinal("DEAL_NUMBER")) ? null : reader.GetString(reader.GetOrdinal("DEAL_NUMBER")),

                                DealLicenses = new List<License>()
                            };

                            existingContract.ContDeals.Add(existingDeal);
                        }


                        int licenseId = reader.IsDBNull(reader.GetOrdinal("LIC_ID")) ? -1 : reader.GetInt32(reader.GetOrdinal("LIC_ID"));

                        License existingLicense = existingDeal.DealLicenses.FirstOrDefault(l => l.LicId == licenseId);

                        if (existingLicense == null)
                        {
                            existingLicense = new License
                            {
                                LicId = licenseId,
                                LicType = reader.IsDBNull(reader.GetOrdinal("LIC_TYPE")) ? -1 : reader.GetInt32(reader.GetOrdinal("LIC_TYPE")),
                                LicNextId = reader.IsDBNull(reader.GetOrdinal("LIC_NEXT_ID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("LIC_NEXT_ID")),
                                LicPrevId = reader.IsDBNull(reader.GetOrdinal("LIC_PREV_ID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("LIC_PREV_ID")),
                                LicBeginDate = reader.IsDBNull(reader.GetOrdinal("LIC_BEGIN_DATE")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("LIC_BEGIN_DATE")),
                                LicEndDate = reader.IsDBNull(reader.GetOrdinal("LIC_END_DATE")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("LIC_END_DATE")),

                                LicConf = new List<Configuration>(),
                                LicMod = new List<ModulesLicensy>()
                            };

                            existingDeal.DealLicenses.Add(existingLicense);
                        }

                        int configurationId = reader.IsDBNull(reader.GetOrdinal("CONF_ID")) ? -1 : reader.GetInt32(reader.GetOrdinal("CONF_ID"));

                        Configuration existingConfiguration = existingLicense.LicConf.FirstOrDefault(conf => conf.ConfId == configurationId);

                        if (existingConfiguration == null && configurationId != -1)
                        {
                            existingConfiguration = new Configuration
                            {
                                ConfId = configurationId,
                                ConfName = reader.IsDBNull(reader.GetOrdinal("CONF_NAME")) ? null : reader.GetString(reader.GetOrdinal("CONF_NAME")),
                                ConfCost = reader.IsDBNull(reader.GetOrdinal("CONF_COST")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("CONF_COST")),
                                ConfRenewCost = reader.IsDBNull(reader.GetOrdinal("CONF_RENEW_COST")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("CONF_RENEW_COST")),
                                ConfBuyRule = reader.IsDBNull(reader.GetOrdinal("CONF_BUY_RULE")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("CONF_BUY_RULE")),
                                ConfRenewRule = reader.IsDBNull(reader.GetOrdinal("CONF_RENEW_RULE")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("CONF_RENEW_RULE"))
                            };

                            existingLicense.LicConf.Add(existingConfiguration);
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("MOD_LIC_ID")))
                        {
                            int modulesLicensyId = reader.GetInt32(reader.GetOrdinal("MOD_LIC_ID"));

                            ModulesLicensy existingModulesLicensy = existingLicense.LicMod.FirstOrDefault(ml => ml.ModLicId == modulesLicensyId);

                            if (existingModulesLicensy == null)
                            {
                                existingModulesLicensy = new ModulesLicensy
                                {
                                    ModLicId = modulesLicensyId,
                                    LicId = licenseId,
                                    ModId = reader.IsDBNull(reader.GetOrdinal("MOD_ID")) ? -1 : reader.GetInt32(reader.GetOrdinal("MOD_ID")),
                                    ModL = new List<Module>()
                                };

                                existingLicense.LicMod.Add(existingModulesLicensy);
                            }

                            int moduleId = reader.IsDBNull(reader.GetOrdinal("MOD_ID")) ? -1 : reader.GetInt32(reader.GetOrdinal("MOD_ID"));

                            Module existingModule = existingModulesLicensy.ModL.FirstOrDefault(m => m.ModId == moduleId);

                            if (existingModule == null)
                            {
                                existingModule = new Module
                                {
                                    ModId = moduleId,
                                    ModName = reader.IsDBNull(reader.GetOrdinal("MOD_NAME")) ? null : reader.GetString(reader.GetOrdinal("MOD_NAME")),
                                    ModCost = reader.GetInt32(reader.GetOrdinal("MOD_COST")),
                                };

                                existingModulesLicensy.ModL.Add(existingModule);
                            }
                        }
                    }
                }
            }

            return clients;
        }
        public static List<Client> getAllClientsWithoutDate()
        {

            List<Client> clients = new List<Client>();

            using (SqlConnection connection = new SqlConnection(dataConnectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(
                "SELECT " +
                "   c.CLI_ID, " +
                "   c.CLI_FULL_NAME, " +
                "   c.CLI_LEGAL_ADDRESS, " +
                "   c.CLI_ACTUAL_ADDRESS, " +
                "   c.CLI_PHONE_NUMBER, " +
                "   c.CLI_EMAIL, " +
                "   c.CLI_DIRECTOR_NAME, " +
                "   co.CONT_ID, " +
                "   co.CONT_NUMBER, " +
                "   co.CONT_DATE, " +
                "   d.DEAL_ID, " +
                "   d.DEAL_DATE, " +
                "   d.DEAL_NUMBER, " +
                "   l.LIC_ID, " +
                "   l.LIC_TYPE, " +
                "   l.LIC_NEXT_ID, " +
                "   l.LIC_PREV_ID, " +
                "   l.LIC_BEGIN_DATE, " +
                "   l.LIC_END_DATE, " +
                "   cf.CONF_ID, " +
                "   cf.CONF_NAME, " +
                "   cf.CONF_COST, " +
                "   cf.CONF_RENEW_COST, " +
                "   cf.CONF_BUY_RULE, " +
                "   cf.CONF_RENEW_RULE, " +
                "   m.MOD_ID, " +
                "   m.MOD_NAME, " +
                "   m.MOD_COST, " +
                "   ml.MOD_LIC_ID " +
                "FROM dbo.CLIENT c " +
                "LEFT JOIN dbo.CONTRACT co ON c.CLI_ID = co.CLI_ID " +
                "LEFT JOIN dbo.DEAL d ON co.CONT_ID = d.CONT_ID " +
                "LEFT JOIN dbo.LICENSE l ON d.DEAL_ID = l.DEAL_ID  " +
                "LEFT JOIN dbo.CONFIGURATION cf ON l.CONF_ID = cf.CONF_ID " +
                "LEFT JOIN dbo.MODULES_LICENSIES ml ON l.LIC_ID = ml.LIC_ID " +
                "LEFT JOIN dbo.MODULE m ON ml.MOD_ID = m.MOD_ID ",
                connection
            );

                command.CommandTimeout = 120;

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int clientId = reader.IsDBNull(reader.GetOrdinal("CLI_ID")) ? -1 : reader.GetInt32(reader.GetOrdinal("CLI_ID"));

                        Client existingClient = clients.FirstOrDefault(c => c.CliId == clientId);

                        if (existingClient == null)
                        {
                            existingClient = new Client
                            {
                                CliId = clientId,
                                CliFullName = reader.IsDBNull(reader.GetOrdinal("CLI_FULL_NAME")) ? null : reader.GetString(reader.GetOrdinal("CLI_FULL_NAME")),
                                CliLegalAddress = reader.IsDBNull(reader.GetOrdinal("CLI_LEGAL_ADDRESS")) ? null : reader.GetString(reader.GetOrdinal("CLI_LEGAL_ADDRESS")),
                                CliActualAddress = reader.IsDBNull(reader.GetOrdinal("CLI_ACTUAL_ADDRESS")) ? null : reader.GetString(reader.GetOrdinal("CLI_ACTUAL_ADDRESS")),
                                CliPhoneNumber = reader.IsDBNull(reader.GetOrdinal("CLI_PHONE_NUMBER")) ? null : reader.GetString(reader.GetOrdinal("CLI_PHONE_NUMBER")),
                                CliEmail = reader.IsDBNull(reader.GetOrdinal("CLI_EMAIL")) ? null : reader.GetString(reader.GetOrdinal("CLI_EMAIL")),
                                CliDirectorName = reader.IsDBNull(reader.GetOrdinal("CLI_DIRECTOR_NAME")) ? null : reader.GetString(reader.GetOrdinal("CLI_DIRECTOR_NAME")),
                                CliContracts = new List<Contract>()
                            };

                            clients.Add(existingClient);
                        }

                        int contractId = reader.IsDBNull(reader.GetOrdinal("CONT_ID")) ? -1 : reader.GetInt32(reader.GetOrdinal("CONT_ID"));

                        Contract existingContract = existingClient.CliContracts.FirstOrDefault(co => co.ContId == contractId);

                        if (existingContract == null)
                        {
                            existingContract = new Contract
                            {
                                ContId = contractId,
                                CliId = clientId,
                                ContNumber = reader.IsDBNull(reader.GetOrdinal("CONT_NUMBER")) ? null : reader.GetString(reader.GetOrdinal("CONT_NUMBER")),
                                ContDate = reader.IsDBNull(reader.GetOrdinal("CONT_DATE")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("CONT_DATE")),
                                ContDeals = new List<Deal>()
                            };

                            existingClient.CliContracts.Add(existingContract);
                        }

                        int dealId = reader.IsDBNull(reader.GetOrdinal("DEAL_ID")) ? -1 : reader.GetInt32(reader.GetOrdinal("DEAL_ID"));

                        Deal existingDeal = existingContract.ContDeals.FirstOrDefault(d => d.DealId == dealId);

                        if (existingDeal == null)
                        {
                            existingDeal = new Deal
                            {
                                DealId = dealId,
                                ContId = contractId,
                                DealDate = reader.IsDBNull(reader.GetOrdinal("DEAL_DATE")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("DEAL_DATE")),
                                DealNum = reader.IsDBNull(reader.GetOrdinal("DEAL_NUMBER")) ? null : reader.GetString(reader.GetOrdinal("DEAL_NUMBER")),

                                DealLicenses = new List<License>()
                            };

                            existingContract.ContDeals.Add(existingDeal);
                        }


                        int licenseId = reader.IsDBNull(reader.GetOrdinal("LIC_ID")) ? -1 : reader.GetInt32(reader.GetOrdinal("LIC_ID"));

                        License existingLicense = existingDeal.DealLicenses.FirstOrDefault(l => l.LicId == licenseId);

                        if (existingLicense == null)
                        {
                            existingLicense = new License
                            {
                                LicId = licenseId,
                                LicType = reader.IsDBNull(reader.GetOrdinal("LIC_TYPE")) ? -1 : reader.GetInt32(reader.GetOrdinal("LIC_TYPE")),
                                LicNextId = reader.IsDBNull(reader.GetOrdinal("LIC_NEXT_ID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("LIC_NEXT_ID")),
                                LicPrevId = reader.IsDBNull(reader.GetOrdinal("LIC_PREV_ID")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("LIC_PREV_ID")),
                                LicBeginDate = reader.IsDBNull(reader.GetOrdinal("LIC_BEGIN_DATE")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("LIC_BEGIN_DATE")),
                                LicEndDate = reader.IsDBNull(reader.GetOrdinal("LIC_END_DATE")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("LIC_END_DATE")),

                                LicConf = new List<Configuration>(),
                                LicMod = new List<ModulesLicensy>()
                            };

                            existingDeal.DealLicenses.Add(existingLicense);
                        }

                        int configurationId = reader.IsDBNull(reader.GetOrdinal("CONF_ID")) ? -1 : reader.GetInt32(reader.GetOrdinal("CONF_ID"));

                        Configuration existingConfiguration = existingLicense.LicConf.FirstOrDefault(conf => conf.ConfId == configurationId);

                        if (existingConfiguration == null && configurationId != -1)
                        {
                            existingConfiguration = new Configuration
                            {
                                ConfId = configurationId,
                                ConfName = reader.IsDBNull(reader.GetOrdinal("CONF_NAME")) ? null : reader.GetString(reader.GetOrdinal("CONF_NAME")),
                                ConfCost = reader.IsDBNull(reader.GetOrdinal("CONF_COST")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("CONF_COST")),
                                ConfRenewCost = reader.IsDBNull(reader.GetOrdinal("CONF_RENEW_COST")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("CONF_RENEW_COST")),
                                ConfBuyRule = reader.IsDBNull(reader.GetOrdinal("CONF_BUY_RULE")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("CONF_BUY_RULE")),
                                ConfRenewRule = reader.IsDBNull(reader.GetOrdinal("CONF_RENEW_RULE")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("CONF_RENEW_RULE"))
                            };

                            existingLicense.LicConf.Add(existingConfiguration);
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("MOD_LIC_ID")))
                        {
                            int modulesLicensyId = reader.GetInt32(reader.GetOrdinal("MOD_LIC_ID"));

                            ModulesLicensy existingModulesLicensy = existingLicense.LicMod.FirstOrDefault(ml => ml.ModLicId == modulesLicensyId);

                            if (existingModulesLicensy == null)
                            {
                                existingModulesLicensy = new ModulesLicensy
                                {
                                    ModLicId = modulesLicensyId,
                                    LicId = licenseId,
                                    ModId = reader.IsDBNull(reader.GetOrdinal("MOD_ID")) ? -1 : reader.GetInt32(reader.GetOrdinal("MOD_ID")),
                                    ModL = new List<Module>()
                                };

                                existingLicense.LicMod.Add(existingModulesLicensy);
                            }

                            int moduleId = reader.IsDBNull(reader.GetOrdinal("MOD_ID")) ? -1 : reader.GetInt32(reader.GetOrdinal("MOD_ID"));

                            Module existingModule = existingModulesLicensy.ModL.FirstOrDefault(m => m.ModId == moduleId);

                            if (existingModule == null)
                            {
                                existingModule = new Module
                                {
                                    ModId = moduleId,
                                    ModName = reader.IsDBNull(reader.GetOrdinal("MOD_NAME")) ? null : reader.GetString(reader.GetOrdinal("MOD_NAME")),
                                    ModCost = reader.GetInt32(reader.GetOrdinal("MOD_COST")),
                                };

                                existingModulesLicensy.ModL.Add(existingModule);
                            }
                        }
                    }
                }
            }

            return clients;
        }
        public static List<Client> GetDeploymets(DateTime start, DateTime end)
        {
            List<Client> clients = new List<Client>();
            using (SqlConnection connection = new SqlConnection(dataConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(
                    "SELECT " +
                "   c.CLI_ID, " +
                "   co.CONT_ID, " +
                "   co.CONT_NUMBER, " +
                "   co.CONT_DATE " +
                "FROM dbo.CLIENT c " +
                "LEFT JOIN dbo.CONTRACT co ON c.CLI_ID = co.CLI_ID " +
                "WHERE co.CONT_DATE >= @start AND co.CONT_DATE <= @end",
                    connection);
                command.Parameters.AddWithValue("@start", start);
                command.Parameters.AddWithValue("@end", end);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int cliId = reader.GetInt32(reader.GetOrdinal("CLI_ID"));

                        Client currentClient = new Client
                        {
                            CliId = cliId
                        };

                        clients.Add(currentClient);

                        int contractId = reader.IsDBNull(reader.GetOrdinal("CONT_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("CONT_ID"));

                        Contract existingContract = currentClient.CliContracts.FirstOrDefault(co => co.ContId == contractId);

                        existingContract = new Contract
                        {
                            ContId = contractId,
                            CliId = cliId,
                            ContNumber = reader.IsDBNull(reader.GetOrdinal("CONT_NUMBER")) ? null : reader.GetString(reader.GetOrdinal("CONT_NUMBER")),
                            ContDate = reader.IsDBNull(reader.GetOrdinal("CONT_DATE")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("CONT_DATE")),
                        };

                        currentClient.CliContracts.Add(existingContract);

                    }
                }

                connection.Close();
            }

            return clients;

        }
        public static List<License> GetLicenses()
        {
            List<License> licenses = new List<License>();

            using (SqlConnection connection = new SqlConnection(dataConnectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(
                    "SELECT " +
                    "   l.LIC_ID, " +
                    "   l.LIC_TYPE, " +
                    "   l.LIC_NEXT_ID, " +
                    "   l.LIC_PREV_ID, " +
                    "   l.LIC_BEGIN_DATE, " +
                    "   l.LIC_END_DATE, " +
                    "   cf.CONF_ID, " +
                    "   cf.CONF_NAME, " +
                    "   cf.CONF_COST, " +
                    "   cf.CONF_RENEW_COST, " +
                    "   cf.CONF_BUY_RULE, " +
                    "   cf.CONF_RENEW_RULE " +
                    "FROM dbo.LICENSE l " +
                    "LEFT JOIN dbo.CONFIGURATION cf ON l.CONF_ID = cf.CONF_ID " 
                    ,connection);


                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int licId = reader.GetInt32(reader.GetOrdinal("LIC_ID"));
                        License license = new License
                        {
                            LicId = licId,
                            LicType = reader.GetInt32(reader.GetOrdinal("LIC_TYPE")),
                            LicPrevId = reader.IsDBNull(reader.GetOrdinal("LIC_PREV_ID")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("LIC_PREV_ID")),
                            LicNextId = reader.IsDBNull(reader.GetOrdinal("LIC_NEXT_ID")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("LIC_NEXT_ID")),
                            LicBeginDate = reader.GetDateTime(reader.GetOrdinal("LIC_BEGIN_DATE")),
                            LicEndDate = reader.GetDateTime(reader.GetOrdinal("LIC_END_DATE")),
                            LicConf = new List<Configuration>()
                        };

                        Configuration configuration = new Configuration
                        {
                            ConfId = reader.IsDBNull(reader.GetOrdinal("CONF_ID")) ? 0 : reader.GetInt32(reader.GetOrdinal("CONF_ID")),
                            ConfName = reader.IsDBNull(reader.GetOrdinal("CONF_NAME")) ? null : reader.GetString(reader.GetOrdinal("CONF_NAME")),
                            ConfCost = reader.IsDBNull(reader.GetOrdinal("CONF_COST")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("CONF_COST")),
                            ConfRenewCost = reader.IsDBNull(reader.GetOrdinal("CONF_RENEW_COST")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("CONF_RENEW_COST")),
                            ConfBuyRule = reader.IsDBNull(reader.GetOrdinal("CONF_BUY_RULE")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("CONF_BUY_RULE")),
                            ConfRenewRule = reader.IsDBNull(reader.GetOrdinal("CONF_RENEW_RULE")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("CONF_RENEW_RULE"))
                        };

                        license.LicConf.Add(configuration);
                        licenses.Add(license);
                    }
                }

                connection.Close();
            }

            return licenses;
        }
    }
}
