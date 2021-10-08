using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stratis.FederatedSidechains.AdminDashboard.Models
{
    public class SDAVoteModel
    {
        [Required]
        public long ProposalId { get; set; }
        [Required]
        public string VotingDecision { get; set; }
        [Required]
        public string WalletName { get; set; }
        [Required]
        public string WalletPassword { get; set; }
    }
}
