namespace Stratis.FederatedSidechains.AdminDashboard.Settings
{
    public class DefaultEndpointsSettings
    {
        public string DataFolder { get; set; }
        public string EnvType { get; set; }
        public string IntervalTime { get; set; }
        public string SDADaoContractAddress { get; set; }
        public string MainchainNodeEndpoint { get; set; }
        public string SidechainNodeEndpoint { get; set; }
        public string SidechainNodeType { get; set; }

        public override string ToString()
        {
            return $"{nameof(this.MainchainNodeEndpoint)}: {this.MainchainNodeEndpoint}; {nameof(this.SidechainNodeEndpoint)}: {this.SidechainNodeEndpoint}; {nameof(this.SidechainNodeType)}: {this.SidechainNodeType}; {nameof(this.EnvType)}: {this.EnvType}";
        }
    }

    public class NodeTypes
    {
        public const string TenK = "10K";
        public const string FiftyK = "50K";
    }

    public class NodeEnv
    {
        public const string TestNet = "TestNet";
        public const string MainNet = "MainNet";
    }
}