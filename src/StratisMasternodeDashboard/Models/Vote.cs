using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Stratis.FederatedSidechains.AdminDashboard.Entities;

namespace Stratis.FederatedSidechains.AdminDashboard.Models
{
    public class Vote
    {
        [Required(AllowEmptyStrings = false)]
        public string Hash { get; set; }
        
        public List<PendingPoll> Polls { get; set; }
        public List<PendingPoll> KickFederationMemberPolls { get; set; }
        public string PubKey { get; set; }
        public string Message { get; set; }
        public int FederationMemberCount { get; set; }
    }
}