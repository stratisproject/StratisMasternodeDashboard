using System;

namespace Stratis.FederatedSidechains.AdminDashboard.Models
{
    public class SideChainNodeStatsModel
    {
        public string AgentVersion { get; set; }
        public string Uptime { get; set; }
        public DateTime NodeStartDateTime { get; set; }
    }
}
