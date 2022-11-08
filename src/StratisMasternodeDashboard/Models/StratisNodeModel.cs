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
      
        public List<Peer> FederationMembers { get; set; }
        public List<FederationWalletHistoryModel> FederationWalletHistory { get; set; }
        public List<LogRule> LogRules { get; set; }       
        public string SwaggerUrl { get; set; }
    }
}