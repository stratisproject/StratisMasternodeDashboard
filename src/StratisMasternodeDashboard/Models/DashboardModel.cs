using Newtonsoft.Json.Linq;

namespace Stratis.FederatedSidechains.AdminDashboard.Models
{
    public class DashboardModel
    {
        public bool IsCacheBuilt { get; set; }
        public bool Status { get; set; }
        public string SidechainMiningAddress { get; set; }
        public JArray MiningPublicKeys { get; set; }
        public StratisNodeModel StratisNode { get; set; }
        public SidechainNodeModel SidechainNode { get; set; }
    }
}