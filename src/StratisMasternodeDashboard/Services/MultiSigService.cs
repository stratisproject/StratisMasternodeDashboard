using Microsoft.Extensions.Logging;
using Stratis.FederatedSidechains.AdminDashboard.Settings;
using System.Threading.Tasks;

namespace Stratis.FederatedSidechains.AdminDashboard.Services
{
    public abstract class MultiSigService : NodeDataService
    {
        public (double confirmedBalance, double unconfirmedBalance) FedWalletBalance { get; set; } = (0, 0);
        public object WalletHistory { get; set; }

        public MultiSigService(ApiRequester apiRequester, string endpoint, ILoggerFactory loggerFactory, string environment, string dataFolder)
            : base(apiRequester, endpoint, loggerFactory, environment, dataFolder)
        {
        }

        protected async Task<NodeDataService> UpdateMultiSig()
        {
            await base.Update().ConfigureAwait(false);

            FedWalletBalance = await this.UpdateWalletBalance().ConfigureAwait(false);
            WalletHistory = await this.UpdateHistory().ConfigureAwait(false);

            return this;
        }
    }

    public sealed class MultiSigMainChainService : MultiSigService
    {
        public MultiSigMainChainService(ApiRequester apiRequester, DefaultEndpointsSettings defaultEndpointSettings, ILoggerFactory loggerFactory)
            : base(apiRequester, defaultEndpointSettings.StratisNode, loggerFactory, defaultEndpointSettings.EnvType, defaultEndpointSettings.DataFolder)
        {
        }

        public override async Task<NodeDataService> Update()
        {
            await UpdateMultiSig().ConfigureAwait(false);
            AddressIndexerHeight = await UpdateAddressIndexerTipAsync().ConfigureAwait(false);

            return this;
        }
    }

    public sealed class MultiSigSideChainService : MultiSigService
    {
        public MultiSigSideChainService(ApiRequester apiRequester, DefaultEndpointsSettings defaultEndpointSettings, ILoggerFactory loggerFactory)
            : base(apiRequester, defaultEndpointSettings.SidechainNode, loggerFactory, defaultEndpointSettings.EnvType, defaultEndpointSettings.DataFolder)
        {
        }

        public override async Task<NodeDataService> Update()
        {
            await UpdateMultiSig();

            // Sidechain related updates.
            WalletBalance = await UpdateMiningWalletBalance().ConfigureAwait(false);
            PendingPolls = await UpdatePolls().ConfigureAwait(false);
            KickFederationMememberPendingPolls = await UpdateKickFederationMemberPolls().ConfigureAwait(false);
            FederationMemberCount = await UpdateFederationMemberCount().ConfigureAwait(false);
            SidechainMinerStats = await UpdateFederationMemberInfo().ConfigureAwait(false);

            return this;
        }
    }
}
