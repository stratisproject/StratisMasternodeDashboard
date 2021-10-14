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
    public abstract class NodeDataService
    {
        public int AddressIndexerHeight { get; set; } = 0;
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
        private readonly ILogger<NodeDataService> logger;
        protected readonly bool isMainnet = true;

        public NodeDataService(ApiRequester apiRequester, string endpoint, ILoggerFactory loggerFactory, string env, string dataFolder)
        {
            this.apiRequester = apiRequester;
            this.endpoint = endpoint;
            this.logger = loggerFactory.CreateLogger<NodeDataService>();
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

        public virtual async Task<NodeDataService> Update()
        {
            BestHash = await UpdateBestHash();
            LogRules = await UpdateLogRules();
            NodeDashboardStats = await UpdateDashboardStats();
            NodeStatus = await UpdateNodeStatus();
            RawMempool = await UpdateMempool();

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

        protected async Task<SidechainMinerStats> UpdateFederationMemberInfo()
        {
            var sidechainMinerStats = new SidechainMinerStats();

            try
            {
                var response = await apiRequester.GetRequestAsync(endpoint, "/api/Federation/members/current").ConfigureAwait(false);

                if (response.Content != null)
                {
                    sidechainMinerStats.BlockProducerHits = response.Content.miningStats?.minerHits ?? 0;
                    if (sidechainMinerStats.BlockProducerHits != 0)
                        sidechainMinerStats.BlockProducerHitsValue = (int)(response.Content.miningStats.minerHits / (float)response.Content.federationSize * 100);
                    else
                        sidechainMinerStats.BlockProducerHitsValue = 0;

                    sidechainMinerStats.MiningAddress = response.Content.miningStats?.miningAddress ?? "Waiting for block to be mined.";

                    if (response.Content.miningStats != null)
                        sidechainMinerStats.ProducedBlockInLastRound = (bool)response.Content.miningStats.producedBlockInLastRound;
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to update federation member info.");
            }

            return sidechainMinerStats;
        }

        protected async Task<int> UpdateAddressIndexerTipAsync()
        {
            int tip = 0;

            try
            {
                var response = await apiRequester.GetRequestAsync(endpoint, "/api/BlockStore/addressindexertip").ConfigureAwait(false);
                tip = response.Content.tipHeight;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to update the address indexer tip.");
            }

            return tip;
        }

        protected async Task<List<PendingPoll>> UpdatePolls()
        {
            List<PendingPoll> pendingPolls = new List<PendingPoll>();

            try
            {
                ApiResponse responseApproved = await apiRequester.GetRequestAsync(endpoint, "/api/Voting/whitelistedhashes");

                var approvedPolls = JsonConvert.DeserializeObject<List<ApprovedPoll>>(responseApproved.Content.ToString());
                ApiResponse responsePending = await apiRequester.GetRequestAsync(endpoint, "/api/Voting/polls/pending", $"voteType=2");

                pendingPolls = JsonConvert.DeserializeObject<List<PendingPoll>>(responsePending.Content.ToString());

                pendingPolls = pendingPolls.FindAll(x => x.VotingDataString.Contains("WhitelistHash"));

                if (approvedPolls == null || approvedPolls.Count == 0)
                    return pendingPolls;

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
                if (responseKickFedMemPending.Content != null)
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

        protected async Task<NodeDashboardStats> UpdateDashboardStats()
        {
            var nodeDashboardStats = new NodeDashboardStats();
            try
            {
                string response;
                using HttpClient client = new HttpClient();
                response = await client.GetStringAsync($"{endpoint}/api/Dashboard/Stats").ConfigureAwait(false);
                nodeDashboardStats.OrphanSize = orphanSize.Match(response).Groups[1].Value;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to get /api/Dashboard/Stats");
            }

            return nodeDashboardStats;
        }
    }
}
