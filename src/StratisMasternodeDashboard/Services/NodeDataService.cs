using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NBitcoin;
using NBitcoin.DataEncoders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stratis.FederatedSidechains.AdminDashboard.Entities;
using Stratis.FederatedSidechains.AdminDashboard.Settings;

namespace Stratis.FederatedSidechains.AdminDashboard.Services
{
    public abstract class NodeDataService
    {
        public List<LogRule> LogRules { get; set; }       
        public ApiResponse StatusResponse { get; set; }
        public ApiResponse FedInfoResponse { get; set; }
        public List<PendingPoll> PendingPolls { get; set; }
        public List<PendingPoll> KickFederationMememberPendingPolls { get; set; }
        public int FederationMemberCount { get; set; }
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
            LogRules = await UpdateLogRules();
            return this;
        }

        protected async Task<List<LogRule>> UpdateLogRules()
        {
            List<LogRule> responseLog = new();
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

    }
}
