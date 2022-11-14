using Stratis.FederatedSidechains.AdminDashboard.Entities;
using System;
using System.Collections.Generic;

namespace Stratis.FederatedSidechains.AdminDashboard.Models
{
    public sealed class SidechainNodeModel : StratisNodeModel
    {
        public List<PendingPoll> PoAPendingPolls { get; set; }
        public int FederationMemberCount { get; set; }       
        public List<PendingPoll> KickFederationMemberPolls { get; set; }

        public string SidechainNodeHeading { get; set; }
        public DateTime SidechainNodeStarted { get; set; }
    }
}