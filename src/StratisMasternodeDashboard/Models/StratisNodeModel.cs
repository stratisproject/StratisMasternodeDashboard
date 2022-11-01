using System.Collections.Generic;
using Stratis.FederatedSidechains.AdminDashboard.Entities;

namespace Stratis.FederatedSidechains.AdminDashboard.Models
{
    public class StratisNodeModel
    {
        public StratisNodeModel()
        {
            FederationWalletHistory = new List<FederationWalletHistoryModel>();
        }

        public string AgentVersion { get; set; }
        public float SyncingStatus { get; set; }
        public string WebAPIUrl { get; set; } = "http://localhost:38221/api";
        public string SwaggerUrl { get; set; } = "http://localhost:38221/swagger";
        public int BlockHeight { get; set; }
        public int MempoolSize { get; set; }
        public string BlockHash { get; set; }
        public List<Peer> Peers { get; set; }
        public List<Peer> FederationMembers { get; set; }
        public List<FederationWalletHistoryModel> FederationWalletHistory { get; set; }
        public List<LogRule> LogRules { get; set; }
        public bool IsMining { get; set; }
        public int HeaderHeight { get; set; }
        public int AddressIndexer { get; set; }
        public string Uptime { get; set; }
    }
}