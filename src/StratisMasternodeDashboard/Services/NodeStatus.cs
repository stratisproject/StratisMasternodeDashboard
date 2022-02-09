namespace Stratis.FederatedSidechains.AdminDashboard.Services
{
    public class NodeStatus
    {
        public float SyncingProgress
        {
            get
            {
                return ConsensusHeight > 0 ? (BlockStoreHeight / HeaderHeight) * 100 : 0;
            }
        }

        public float BlockStoreHeight { get; set; } = 0;
        public float HeaderHeight { get; set; } = 0;
        public float ConsensusHeight { get; set; } = 0;
        public string Uptime { get; set; } = string.Empty;
        public string State { get; set; } = "Not Operational";
        public string Version { get; set; }
    }

    public class NodeDashboardStats
    {
        public int HeaderHeight { get; set; } = 0;
        public string OrphanSize { get; set; } = string.Empty;
    }

    public sealed class SidechainMinerStats
    {
        public int BlockProducerHits { get; set; }
        public int BlockProducerHitsValue { get; set; }
        public string MiningAddress { get; set; }
        public bool ProducedBlockInLastRound { get; set; }
    }
}