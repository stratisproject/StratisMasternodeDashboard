using Microsoft.Extensions.Logging;
using Stratis.FederatedSidechains.AdminDashboard.Settings;
using System.Threading.Tasks;

namespace Stratis.FederatedSidechains.AdminDashboard.Services
{
    public abstract class MultiSigNodeDataService : NodeDataService
    {
        public MultiSigNodeDataService(ApiRequester apiRequester, string endpoint, ILoggerFactory loggerFactory, string environment, string dataFolder)
            : base(apiRequester, endpoint, loggerFactory, environment, dataFolder)
        {
        }

        protected async Task<NodeDataService> UpdateMultiSig()
        {
            await base.Update().ConfigureAwait(false);
            return this;
        }
    }

    public sealed class MultiSigMainChainService : MultiSigNodeDataService
    {
        public MultiSigMainChainService(ApiRequester apiRequester, DefaultEndpointsSettings defaultEndpointSettings, ILoggerFactory loggerFactory)
            : base(apiRequester, defaultEndpointSettings.MainchainNode, loggerFactory, defaultEndpointSettings.EnvType, defaultEndpointSettings.DataFolder)
        {
        }

        public override async Task<NodeDataService> Update()
        {
            await UpdateMultiSig().ConfigureAwait(false);
            AddressIndexerHeight = await UpdateAddressIndexerTipAsync().ConfigureAwait(false);

            return this;
        }
    }

    public sealed class MultiSigSideChainService : MultiSigNodeDataService
    {
        public MultiSigSideChainService(ApiRequester apiRequester, DefaultEndpointsSettings defaultEndpointSettings, ILoggerFactory loggerFactory)
            : base(apiRequester, defaultEndpointSettings.SidechainNode, loggerFactory, defaultEndpointSettings.EnvType, defaultEndpointSettings.DataFolder)
        {
            SidechainMinerStats = new SidechainMinerStats();
        }

        public override async Task<NodeDataService> Update()
        {
            await UpdateMultiSig().ConfigureAwait(false);

            // Sidechain related updates.
            PendingPolls = await UpdatePolls().ConfigureAwait(false);
            KickFederationMememberPendingPolls = await UpdateKickFederationMemberPolls().ConfigureAwait(false);
            FederationMemberCount = await UpdateFederationMemberCount().ConfigureAwait(false);
           
            return this;
        }
    }
}
