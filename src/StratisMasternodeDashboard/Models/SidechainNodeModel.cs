using Stratis.FederatedSidechains.AdminDashboard.Entities;
using System.Collections.Generic;

namespace Stratis.FederatedSidechains.AdminDashboard.Models
{
    public class SidechainNodeModel : StratisNodeModel
    {
        public List<PendingPoll> PoAPendingPolls { get; set; }
        public int FederationMemberCount { get; set; }       
        public List<PendingPoll> KickFederationMemberPolls { get; set; }
    }
}