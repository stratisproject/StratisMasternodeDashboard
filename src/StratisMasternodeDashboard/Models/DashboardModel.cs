using System;
using Newtonsoft.Json.Linq;

namespace Stratis.FederatedSidechains.AdminDashboard.Models
{
    public sealed class DashboardModel
    {
        public JArray MiningPublicKeys { get; set; }
        public StratisNodeModel MainchainNode { get; set; }
        public SidechainNodeModel SidechainNode { get; set; }

        public const string MainchainCoinTicker = "STRAX";
        public const string SidechainCoinTicker = "CRS";
    }
}