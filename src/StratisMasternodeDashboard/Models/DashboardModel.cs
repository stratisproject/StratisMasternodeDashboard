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

        public string MainChainNodeHeading
        {
            get
            {

                if (MainchainNode == null)
                    return $"Mainchain Node [OFFLINE]";

                return $"Mainchain Node";
            }
        }

        public string SidechainNodeHeading
        {
            get
            {

                if (SidechainNode == null)
                    return $"Sidechain Node [OFFLINE]";

                return $"Sidechain Node";
            }
        }
    }
}