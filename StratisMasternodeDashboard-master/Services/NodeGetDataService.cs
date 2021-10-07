using Microsoft.Extensions.Logging;
using NBitcoin;
using NBitcoin.DataEncoders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stratis.FederatedSidechains.AdminDashboard.Entities;
using Stratis.FederatedSidechains.AdminDashboard.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace Stratis.FederatedSidechains.AdminDashboard.Services
{
    public abstract class NodeGetDataService
    {
        public NodeStatus NodeStatus { get; set; }
        public List<LogRule> LogRules { get; set; }
        public int RawMempool { get; set; } = 0;
        public string BestHash { get; set; } = string.Empty;
        public ApiResponse StatusResponse { get; set; }
        public ApiResponse FedInfoResponse { get; set; }
        public List<PendingPoll> PendingPolls { get; set; }
        public List<PendingPoll> KickFederationMememberPendingPolls { get; set; }
        public int FederationMemberCount { get; set; }
        public (double confirmedBalance, double unconfirmedBalance) WalletBalance { get; set; } = (0, 0);
        public NodeDashboardStats NodeDashboardStats { get; set; }
        public SidechainMinerStats SidechainMinerStats { get; set; }
        public string MiningPubKey { get; set; }

        protected const int STRATOSHI = 100_000_000;
        protected readonly string miningKeyFile = string.Empty;
        private readonly ApiRequester apiRequester;
        private readonly string endpoint;
        private readonly ILogger<NodeGetDataService> logger;
        protected readonly bool isMainnet = true;

        public NodeGetDataService(ApiRequester apiRequester, string endpoint, ILoggerFactory loggerFactory, string env, string dataFolder)
        {
            this.apiRequester = apiRequester;
            this.endpoint = endpoint;
            this.logger = loggerFactory.CreateLogger<NodeGetDataService>();
            this.isMainnet = env != NodeEnv.TestNet;

            try
            {
                string path;

                if (string.IsNullOrEmpty(dataFolder))
                    path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StratisNode", "cirrus", this.isMainnet ? "CirrusMain" : "CirrusTest");
                else
                    path = dataFolder;

                miningKeyFile = Path.Combine(path, "federationKey.dat");

                try
                {
                    using FileStream readStream = File.OpenRead(miningKeyFile);

                    var privateKey = new Key();
                    var stream = new BitcoinStream(readStream, false);
                    stream.ReadWrite(ref privateKey);
                    this.MiningPubKey = Encoders.Hex.EncodeData(privateKey.PubKey.ToBytes());
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, $"Failed to read file {miningKeyFile}");
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Failed to get APPDATA");
            }
        }

        public virtual async Task<NodeGetDataService> Update()
        {
            NodeDashboardStats = await UpdateDashboardStats();
            NodeStatus = await UpdateNodeStatus();
            LogRules = await UpdateLogRules();
            RawMempool = await UpdateMempool();
            BestHash = await UpdateBestHash();
            return this;
        }

        protected async Task<NodeStatus> UpdateNodeStatus()
        {
            NodeStatus nodeStatus = new NodeStatus();
            try
            {
                StatusResponse = await apiRequester.GetRequestAsync(endpoint, "/api/Node/status");
                nodeStatus.BlockStoreHeight = StatusResponse.Content.blockStoreHeight;
                nodeStatus.HeaderHeight = StatusResponse.Content.headerHeight;
                nodeStatus.ConsensusHeight = StatusResponse.Content.consensusHeight;
                string runningTime = StatusResponse.Content.runningTime;
                string[] parseTime = runningTime.Split('.');
                parseTime = parseTime.Take(parseTime.Length - 1).ToArray();
                nodeStatus.Uptime = string.Join(".", parseTime);
                nodeStatus.State = StatusResponse.Content.state;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to update node status");
            }

            return nodeStatus;
        }

        protected async Task<List<LogRule>> UpdateLogRules()
        {
            List<LogRule> responseLog = new List<LogRule>();
            try
            {
                ApiResponse response = await apiRequester.GetRequestAsync(endpoint, "/api/Node/logrules");
                responseLog = JsonConvert.DeserializeObject<List<LogRule>>(response.Content.ToString());
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to get log rules");
            }

            return responseLog;
        }

        protected async Task<int> UpdateMempool()
        {
            int mempoolSize = 0;
            try
            {
                ApiResponse response = await apiRequester.GetRequestAsync(endpoint, "/api/Mempool/getrawmempool");
                mempoolSize = response.Content.Count;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to get mempool info");
            }

            return mempoolSize;
        }

        protected async Task<string> UpdateBestHash()
        {
            string hash = string.Empty;
            try
            {
                ApiResponse response = await apiRequester.GetRequestAsync(endpoint, "/api/Consensus/getbestblockhash");
                hash = response.Content;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to get best hash");
            }

            return hash;
        }

        protected async Task<(double, double)> UpdateWalletBalance()
        {
            double confirmed = 0;
            double unconfirmed = 0;
            try
            {
                ApiResponse response = await apiRequester.GetRequestAsync(endpoint, "/api/FederationWallet/balance");
                double.TryParse(response.Content.balances[0].amountConfirmed.ToString(), out confirmed);
                double.TryParse(response.Content.balances[0].amountUnconfirmed.ToString(), out unconfirmed);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to get wallet balance");
            }
            return (confirmed / STRATOSHI, unconfirmed / STRATOSHI);
        }

        protected async Task<(double, double)> UpdateMiningWalletBalance()
        {
            double confirmed = 0;
            double unconfirmed = 0;

            try
            {
                ApiResponse responseWallet = await apiRequester.GetRequestAsync(endpoint, "/api/Wallet/list-wallets");
                string firstWalletName = responseWallet.Content.walletNames[0].ToString();
                ApiResponse responseBalance = await apiRequester.GetRequestAsync(endpoint, "/api/Wallet/balance", $"WalletName={firstWalletName}");
                double.TryParse(responseBalance.Content.balances[0].amountConfirmed.ToString(), out confirmed);
                double.TryParse(responseBalance.Content.balances[0].amountUnconfirmed.ToString(), out unconfirmed);

            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to get mining wallet balance");
            }
            return (confirmed / STRATOSHI, unconfirmed / STRATOSHI);
        }

        protected async Task<object> UpdateHistory()
        {
            object history = new object();

            try
            {
                ApiResponse response = await apiRequester.GetRequestAsync(endpoint, "/api/FederationWallet/history", "maxEntriesToReturn=30");
                history = response.Content;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to get history");
            }

            return history;
        }

        protected async Task<string> UpdateFederationGatewayInfo()
        {
            string multiSigAddress = string.Empty;

            try
            {
                FedInfoResponse = await apiRequester.GetRequestAsync(endpoint, "/api/FederationGateway/info").ConfigureAwait(false);
                multiSigAddress = FedInfoResponse.Content.multisigAddress;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to update federation gateway info.");
            }

            return multiSigAddress;
        }

        protected async Task<SidechainMinerStats> UpdateFederationMemberInfo()
        {
            SidechainMinerStats sidechainMinerStats = new SidechainMinerStats();

            try
            {
                var response = await apiRequester.GetRequestAsync(endpoint, "/api/Federation/members/current").ConfigureAwait(false);
                sidechainMinerStats.BlockProducerHits = response.Content.miningStatistics.minerHits;
                sidechainMinerStats.BlockProducerHitsValue = response.Content.miningStatistics.minerHits / (float)response.Content.miningStatistics.federationSize;
                sidechainMinerStats.ProducedBlockInLastRound = (bool)response.Content.miningStatistics.producedBlockInLastRound;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to update federation member info.");
            }

            return sidechainMinerStats;
        }

        protected async Task<List<PendingPoll>> UpdatePolls()
        {
            List<PendingPoll> pendingPolls = new List<PendingPoll>();
            List<ApprovedPoll> approvedPolls = new List<ApprovedPoll>();

            try
            {

                ApiResponse responseApproved = await apiRequester.GetRequestAsync(endpoint, "/api/Voting/whitelistedhashes");
                approvedPolls = JsonConvert.DeserializeObject<List<ApprovedPoll>>(responseApproved.Content.ToString());
                ApiResponse responsePending = await apiRequester.GetRequestAsync(endpoint, "/api/Voting/polls/pending", $"voteType=2");

                pendingPolls = JsonConvert.DeserializeObject<List<PendingPoll>>(responsePending.Content.ToString());

                pendingPolls = pendingPolls.FindAll(x => x.VotingDataString.Contains("WhitelistHash"));

                if (approvedPolls == null || approvedPolls.Count == 0) return pendingPolls;

                foreach (var vote in approvedPolls)
                {
                    PendingPoll pp = new PendingPoll
                    {
                        IsPending = false,
                        IsExecuted = true,
                        VotingDataString = $"Action: 'WhitelistHash',Hash: '{vote.Hash}'"
                    };
                    pendingPolls.RemoveAll(x => x.Hash == vote.Hash);
                    pendingPolls.Add(pp);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to update polls");
            }

            return pendingPolls;
        }

        protected async Task<List<PendingPoll>> UpdateKickFederationMemberPolls()
        {
            List<PendingPoll> pendingPolls = new List<PendingPoll>();

            try
            {
                ApiResponse responseKickFedMemPending = await apiRequester.GetRequestAsync(endpoint, "/api/Voting/polls/pending", $"voteType=0");
                pendingPolls = JsonConvert.DeserializeObject<List<PendingPoll>>(responseKickFedMemPending.Content.ToString());
                return pendingPolls;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to update Kicked Federation Member polls");
            }
            return pendingPolls;
        }

        protected async Task<int> UpdateFederationMemberCount()
        {
            try
            {
                ApiResponse response = await apiRequester.GetRequestAsync(endpoint, "/api/Federation/members");
                if (response.IsSuccess)
                {
                    var token = JToken.Parse(response.Content.ToString());
                    return token.Count;
                }

            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to update fed members count");
            }

            return 0;
        }

        Regex orphanSize = new Regex("Orphan Size:\\s+([0-9]+)", RegexOptions.Compiled);
        Regex asyncLoopStats = new Regex("====== Async loops ======   (.*)", RegexOptions.Compiled);
        Regex addressIndexer = new Regex("AddressIndexer\\.Height:\\s+([0-9]+)", RegexOptions.Compiled);

        protected async Task<NodeDashboardStats> UpdateDashboardStats()
        {
            var nodeDashboardStats = new NodeDashboardStats();
            try
            {
                string response;
                using (HttpClient client = new HttpClient())
                {
                    response = await client.GetStringAsync($"{endpoint}/api/Dashboard/Stats").ConfigureAwait(false);
                    nodeDashboardStats.OrphanSize = orphanSize.Match(response).Groups[1].Value;

                    if (int.TryParse(this.addressIndexer.Match(response).Groups[1].Value, out var height))
                    {
                        nodeDashboardStats.AddressIndexerHeight = height;
                    }

                    nodeDashboardStats.AsyncLoops = asyncLoopStats.Match(response).Groups[1].Value.Replace("[", "").Replace("]", "").Replace(" ", "").Replace("Running", "R").Replace("Faulted", ", F");
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to get /api/Dashboard/Stats");
            }

            return nodeDashboardStats;
        }
    }

    public sealed class NodeGetDataServiceMainchainMiner : NodeGetDataService
    {
        public NodeGetDataServiceMainchainMiner(ApiRequester apiRequester, DefaultEndpointsSettings defaultEndpointSettings, ILoggerFactory loggerFactory)
            : base(apiRequester, defaultEndpointSettings.StratisNode, loggerFactory, defaultEndpointSettings.EnvType, defaultEndpointSettings.DataFolder)
        {

        }
    }

    public abstract class NodeDataServiceMultisig : NodeGetDataService
    {
        public (double confirmedBalance, double unconfirmedBalance) FedWalletBalance { get; set; } = (0, 0);
        public object WalletHistory { get; set; }
        public string FedAddress { get; set; }

        public NodeDataServiceMultisig(ApiRequester apiRequester, string endpoint, ILoggerFactory loggerFactory, string environment, string dataFolder)
            : base(apiRequester, endpoint, loggerFactory, environment, dataFolder)
        {
        }

        protected async Task<NodeGetDataService> UpdateMultiSig()
        {
            NodeDashboardStats = await UpdateDashboardStats().ConfigureAwait(false);
            NodeStatus = await UpdateNodeStatus().ConfigureAwait(false);
            LogRules = await UpdateLogRules().ConfigureAwait(false);
            RawMempool = await UpdateMempool().ConfigureAwait(false);
            BestHash = await UpdateBestHash().ConfigureAwait(false);
            FedWalletBalance = await this.UpdateWalletBalance().ConfigureAwait(false);
            WalletHistory = await this.UpdateHistory().ConfigureAwait(false);
            FedAddress = await this.UpdateFederationGatewayInfo().ConfigureAwait(false);

            return this;
        }
    }

    public sealed class NodeDataServiceMainChainMultisig : NodeDataServiceMultisig
    {
        public NodeDataServiceMainChainMultisig(ApiRequester apiRequester, DefaultEndpointsSettings defaultEndpointSettings, ILoggerFactory loggerFactory)
            : base(apiRequester, defaultEndpointSettings.StratisNode, loggerFactory, defaultEndpointSettings.EnvType, defaultEndpointSettings.DataFolder)
        {
        }

        public override async Task<NodeGetDataService> Update()
        {
            await UpdateMultiSig().ConfigureAwait(false);

            return this;
        }
    }

    public sealed class NodeDataServiceSidechainMultisig : NodeDataServiceMultisig
    {
        public NodeDataServiceSidechainMultisig(ApiRequester apiRequester, DefaultEndpointsSettings defaultEndpointSettings, ILoggerFactory loggerFactory)
            : base(apiRequester, defaultEndpointSettings.SidechainNode, loggerFactory, defaultEndpointSettings.EnvType, defaultEndpointSettings.DataFolder)
        {
        }

        public override async Task<NodeGetDataService> Update()
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

    public sealed class NodeDataServicesSidechainMiner : NodeGetDataService
    {
        public NodeDataServicesSidechainMiner(ApiRequester apiRequester, DefaultEndpointsSettings defaultEndpointSettings, ILoggerFactory loggerFactory)
            : base(apiRequester, defaultEndpointSettings.SidechainNode, loggerFactory, defaultEndpointSettings.EnvType, defaultEndpointSettings.DataFolder)
        {
        }

        public override async Task<NodeGetDataService> Update()
        {
            NodeDashboardStats = await UpdateDashboardStats().ConfigureAwait(false);
            NodeStatus = await UpdateNodeStatus().ConfigureAwait(false);
            LogRules = await UpdateLogRules().ConfigureAwait(false);
            RawMempool = await UpdateMempool().ConfigureAwait(false);
            BestHash = await UpdateBestHash().ConfigureAwait(false);
            WalletBalance = await UpdateMiningWalletBalance().ConfigureAwait(false);
            PendingPolls = await UpdatePolls().ConfigureAwait(false);
            KickFederationMememberPendingPolls = await UpdateKickFederationMemberPolls().ConfigureAwait(false);
            FederationMemberCount = await UpdateFederationMemberCount().ConfigureAwait(false);
            SidechainMinerStats = await UpdateFederationMemberInfo().ConfigureAwait(false);

            return this;
        }
    }
}
