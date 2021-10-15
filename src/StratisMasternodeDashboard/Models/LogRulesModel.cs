using Stratis.FederatedSidechains.AdminDashboard.Entities;
using Stratis.FederatedSidechains.AdminDashboard.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Stratis.FederatedSidechains.AdminDashboard.Models
{
    public class LogRulesModel
    {
        public List<LogRule> Rules { get; set; }

        public LogRulesModel LoadRules(List<LogRule> stratisLogRules, List<LogRule> sidechainLogRules)
        {
            this.Rules = new List<LogRule>();

            foreach (var rule in LogLevelHelper.DefaultLogRules)
            {
                this.Rules.Add(new LogRule()
                {
                    Name = rule,
                    MinLevel = LogLevel.Trace,
                    Filename = string.Empty,
                    StratisActualLevel = LogLevel.Trace,
                    SidechainActualLevel = LogLevel.Trace
                });

                if (stratisLogRules != null && stratisLogRules.Any(x => x.Name.Equals(rule)))
                {
                    this.Rules.FirstOrDefault(x => x.Name.Equals(rule)).StratisActualLevel = stratisLogRules.FirstOrDefault(x => x.Name.Equals(rule)).MinLevel;
                }

                if (sidechainLogRules != null && sidechainLogRules.Any(x => x.Name.Equals(rule)))
                {
                    this.Rules.FirstOrDefault(x => x.Name.Equals(rule)).SidechainActualLevel = sidechainLogRules.FirstOrDefault(x => x.Name.Equals(rule)).MinLevel;
                }
            }

            return this;
        }
    }
}
