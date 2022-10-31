using Microsoft.Extensions.Logging;
using Stratis.FederatedSidechains.AdminDashboard.Settings;
using System.Threading.Tasks;

namespace Stratis.FederatedSidechains.AdminDashboard.Services
{
    public sealed class MasterNodeMainChainService : NodeDataService
    {
        public MasterNodeMainChainService(ApiRequester apiRequester, DefaultEndpointsSettings defaultEndpointSettings, ILoggerFactory loggerFactory)
            : base(apiRequester, defaultEndpointSettings.MainchainNode, loggerFactory, defaultEndpointSettings.EnvType, defaultEndpointSettings.DataFolder)
        {
        }

        public override async Task<NodeDataService> Update()
        {
            await base.Update().ConfigureAwait(false);
            AddressIndexerHeight = await UpdateAddressIndexerTipAsync().ConfigureAwait(false);

            return this;
        }
    }

    public sealed class MasterNodeSideChainService : NodeDataService
    {
        public MasterNodeSideChainService(ApiRequester apiRequester, DefaultEndpointsSettings defaultEndpointSettings, ILoggerFactory loggerFactory)
            : base(apiRequester, defaultEndpointSettings.SidechainNode, loggerFactory, defaultEndpointSettings.EnvType, defaultEndpointSettings.DataFolder)
        {
            SidechainMinerStats = new SidechainMinerStats();
        }

        public override async Task<NodeDataService> Update()
        {
            await base.Update().ConfigureAwait(false);

            PendingPolls = await UpdatePolls().ConfigureAwait(false);
            KickFederationMememberPendingPolls = await UpdateKickFederationMemberPolls().ConfigureAwait(false);
            FederationMemberCount = await UpdateFederationMemberCount().ConfigureAwait(false);
           
            return this;
        }
    }
}
