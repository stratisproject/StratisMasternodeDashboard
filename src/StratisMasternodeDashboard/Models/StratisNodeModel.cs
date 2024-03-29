using System.Collections.Generic;
using Stratis.FederatedSidechains.AdminDashboard.Entities;

namespace Stratis.FederatedSidechains.AdminDashboard.Models
{
    public class StratisNodeModel
    {
        public string AgentVersion { get; set; }
        public float SyncingStatus { get; set; }
        public string WebAPIUrl { get; set; } = "http://localhost:38221/api";
        public string SwaggerUrl { get; set; } = "http://localhost:38221/swagger";
        public int BlockHeight { get; set; }
        public int MempoolSize { get; set; }
        public string BlockHash { get; set; }
        public double ConfirmedBalanceFed { get; set; }
        public double UnconfirmedBalanceFed { get; set; }
        public double ConfirmedBalance { get; set; }
        public double UnconfirmedBalance { get; set; }
        public List<Peer> Peers { get; set; }
        public List<Peer> FederationMembers { get; set; }
        public object History { get; set; }
        public List<LogRule> LogRules { get; set; }
        public string OrphanSize { get; set; }
        public bool IsMining { get; set; }
        public int HeaderHeight { get; set; }
        public int AddressIndexer { get; set; }
        public string Uptime { get; set; }
    }
}