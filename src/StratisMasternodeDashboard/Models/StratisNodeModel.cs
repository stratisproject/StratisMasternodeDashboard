using System.Collections.Generic;
using Stratis.FederatedSidechains.AdminDashboard.Entities;

namespace Stratis.FederatedSidechains.AdminDashboard.Models
{
    public class StratisNodeModel
    {
        public StratisNodeModel()
        {
            FederationMembers = new List<Peer>();
            FederationWalletHistory = new List<FederationWalletHistoryModel>();
        }

        public string AgentVersion { get; set; }
        public string BlockHash { get; set; }
        public int BlockHeight { get; set; }
        public int MempoolSize { get; set; }
        public List<Peer> FederationMembers { get; set; }
        public List<FederationWalletHistoryModel> FederationWalletHistory { get; set; }

        public List<LogRule> LogRules { get; set; }
        public bool IsMining { get; set; }
        public int HeaderHeight { get; set; }
        public string SwaggerUrl { get; set; }
    }
}