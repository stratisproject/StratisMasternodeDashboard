using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stratis.FederatedSidechains.AdminDashboard.Entities;
using Stratis.FederatedSidechains.AdminDashboard.Helpers;
using Stratis.FederatedSidechains.AdminDashboard.Hubs;
using Stratis.FederatedSidechains.AdminDashboard.Models;
using Stratis.FederatedSidechains.AdminDashboard.Services;
using Stratis.FederatedSidechains.AdminDashboard.Settings;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Stratis.FederatedSidechains.AdminDashboard.HostedServices
{
    /// <summary>
    /// This Background Service fetch APIs an cache content
    /// </summary>
    public class FetchingBackgroundService : IHostedService, IDisposable
    {
        private readonly DefaultEndpointsSettings defaultEndpointsSettings;
        private readonly IDistributedCache distributedCache;
        private readonly IHubContext<DataUpdaterHub> updaterHub;
        private readonly ILoggerFactory loggerFactory;
        private readonly ILogger<FetchingBackgroundService> logger;
        private readonly ApiRequester apiRequester;
        private bool successfullyBuilt;
        private Timer dataRetrieverTimer;
        private readonly bool multiSigNode = true;
        private readonly NodeDataService nodeDataServiceMainchain;
        private readonly NodeDataService nodeDataServiceSidechain;

        public FetchingBackgroundService(IDistributedCache distributedCache, DefaultEndpointsSettings defaultEndpointsSettings, IHubContext<DataUpdaterHub> hubContext, ILoggerFactory loggerFactory, ApiRequester apiRequester, IConfiguration configuration)
        {
            this.defaultEndpointsSettings = defaultEndpointsSettings;
            this.distributedCache = distributedCache;
            this.updaterHub = hubContext;
            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory.CreateLogger<FetchingBackgroundService>();
            this.apiRequester = apiRequester;

            if (this.defaultEndpointsSettings.SidechainNodeType == NodeTypes.TenK)
                this.multiSigNode = false;

            this.logger.LogInformation("Default settings {settings}", defaultEndpointsSettings);
            if (this.multiSigNode)
            {
                nodeDataServiceMainchain = new MultiSigMainChainService(this.apiRequester, this.defaultEndpointsSettings, this.loggerFactory);
                nodeDataServiceSidechain = new MultiSigSideChainService(this.apiRequester, this.defaultEndpointsSettings, this.loggerFactory);
            }
            else
            {
                nodeDataServiceMainchain = new MasterNodeMainChainService(this.apiRequester, this.defaultEndpointsSettings, this.loggerFactory);
                nodeDataServiceSidechain = new MasterNodeSideChainService(this.apiRequester, this.defaultEndpointsSettings, this.loggerFactory);
            }
        }

        /// <summary>
        /// Start the Fetching Background Service to Populate Dashboard Datas
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"Starting the Fetching Background Service");

            DoWorkAsync(null);

            int interval = int.Parse(this.defaultEndpointsSettings.IntervalTime);

            this.dataRetrieverTimer = new Timer(DoWorkAsync, null, TimeSpan.Zero, TimeSpan.FromSeconds(interval));
            await Task.CompletedTask;
        }

        /// <summary>
        /// Retrieve all node information and store it in IDistributedCache object
        /// </summary>
        private async Task BuildCacheAsync()
        {
            this.logger.LogInformation($"Refresh the Dashboard Data");

            await nodeDataServiceMainchain.Update();
            await nodeDataServiceSidechain.Update();

            var stratisPeers = new List<Peer>();
            var stratisFederationMembers = new List<Peer>();
            var sidechainPeers = new List<Peer>();
            var sidechainFederationMembers = new List<Peer>();

            try
            {
                if (this.multiSigNode)
                {
                    this.ParsePeers(nodeDataServiceMainchain, stratisPeers, stratisFederationMembers);
                    this.ParsePeers(nodeDataServiceSidechain, sidechainPeers, sidechainFederationMembers);
                }
                else
                {
                    this.ParsePeers(nodeDataServiceMainchain, stratisPeers);
                    this.ParsePeers(nodeDataServiceSidechain, sidechainPeers);
                }
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Unable to parse feeds");
            }

            var dashboardModel = new DashboardModel();

            try
            {
                dashboardModel.Status = true;
                dashboardModel.IsCacheBuilt = true;
                dashboardModel.MiningPublicKeys = nodeDataServiceMainchain.FedInfoResponse?.Content?.federationMultisigPubKeys ?? new JArray();

                // Mainchain Node
                var mainchainNode = new StratisNodeModel
                {
                    History = this.multiSigNode ? ((MultiSigService)nodeDataServiceMainchain).WalletHistory : new JArray(),
                    ConfirmedBalanceFed = this.multiSigNode ? ((MultiSigService)nodeDataServiceMainchain).FedWalletBalance.confirmedBalance : -1,
                    UnconfirmedBalanceFed = this.multiSigNode ? ((MultiSigService)nodeDataServiceMainchain).FedWalletBalance.unconfirmedBalance : -1,

                    WebAPIUrl = UriHelper.BuildUri(this.defaultEndpointsSettings.StratisNode, "/api").ToString(),
                    SwaggerUrl = UriHelper.BuildUri(this.defaultEndpointsSettings.StratisNode, "/swagger").ToString(),
                    SyncingStatus = nodeDataServiceMainchain.NodeStatus.SyncingProgress,
                    Peers = stratisPeers,
                    FederationMembers = stratisFederationMembers,
                    BlockHash = nodeDataServiceMainchain.BestHash,
                    BlockHeight = (int)nodeDataServiceMainchain.NodeStatus.BlockStoreHeight,
                    HeaderHeight = (int)nodeDataServiceMainchain.NodeStatus.HeaderHeight,
                    MempoolSize = nodeDataServiceMainchain.RawMempool,

                    CoinTicker = "STRAX",
                    LogRules = nodeDataServiceMainchain.LogRules,
                    Uptime = nodeDataServiceMainchain.NodeStatus.Uptime,
                    AddressIndexer = this.nodeDataServiceMainchain.AddressIndexerHeight,
                    OrphanSize = this.nodeDataServiceMainchain.NodeDashboardStats?.OrphanSize ?? string.Empty
                };

                dashboardModel.StratisNode = mainchainNode;

                // Sidechain Node
                var sidechainNode = new SidechainNodeModel
                {
                    History = this.multiSigNode ? ((MultiSigSideChainService)nodeDataServiceSidechain).WalletHistory : new JArray(),
                    ConfirmedBalanceFed = this.multiSigNode ? ((MultiSigSideChainService)nodeDataServiceSidechain).FedWalletBalance.confirmedBalance : -1,
                    UnconfirmedBalanceFed = this.multiSigNode ? ((MultiSigSideChainService)nodeDataServiceSidechain).FedWalletBalance.unconfirmedBalance : -1,

                    WebAPIUrl = UriHelper.BuildUri(this.defaultEndpointsSettings.SidechainNode, "/api").ToString(),
                    SwaggerUrl = UriHelper.BuildUri(this.defaultEndpointsSettings.SidechainNode, "/swagger").ToString(),
                    SyncingStatus = nodeDataServiceSidechain.NodeStatus.SyncingProgress,
                    Peers = sidechainPeers,
                    FederationMembers = sidechainFederationMembers,
                    BlockHash = nodeDataServiceSidechain.BestHash,
                    BlockHeight = (int)nodeDataServiceSidechain.NodeStatus.BlockStoreHeight,
                    HeaderHeight = (int)nodeDataServiceSidechain.NodeStatus.HeaderHeight,
                    MempoolSize = nodeDataServiceSidechain.RawMempool,

                    CoinTicker = "CRS",
                    LogRules = nodeDataServiceSidechain.LogRules,
                    PoAPendingPolls = nodeDataServiceSidechain.PendingPolls,
                    Uptime = nodeDataServiceSidechain.NodeStatus.Uptime,

                    BlockProducerHits = this.nodeDataServiceSidechain.SidechainMinerStats.BlockProducerHits,
                    BlockProducerHitsValue = this.nodeDataServiceSidechain.SidechainMinerStats.BlockProducerHitsValue,
                    IsMining = this.nodeDataServiceSidechain.SidechainMinerStats.ProducedBlockInLastRound,
                    SidechainMiningAddress = this.nodeDataServiceSidechain.SidechainMinerStats.MiningAddress,

                    OrphanSize = this.nodeDataServiceSidechain.NodeDashboardStats?.OrphanSize ?? string.Empty,
                    FederationMemberCount = this.nodeDataServiceSidechain.FederationMemberCount,
                    ConfirmedBalance = this.nodeDataServiceSidechain.WalletBalance.confirmedBalance,
                    UnconfirmedBalance = this.nodeDataServiceSidechain.WalletBalance.unconfirmedBalance,
                    KickFederationMemberPolls = nodeDataServiceSidechain.KickFederationMememberPendingPolls
                };

                dashboardModel.SidechainNode = sidechainNode;
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Unable to fetch feeds.");
                return;
            }

            if (!string.IsNullOrEmpty(this.distributedCache.GetString("DashboardData")))
                await this.updaterHub.Clients.All.SendAsync("CacheIsDifferent");

            this.distributedCache.SetString("DashboardData", JsonConvert.SerializeObject(dashboardModel));
        }

        private void ParsePeers(NodeDataService dataService, List<Peer> peers, List<Peer> federationMembers)
        {
            string fedEndpoints = dataService.FedInfoResponse?.Content?.endpoints?.ToString() ?? string.Empty;

            if (dataService.StatusResponse.Content.outboundPeers is JArray outboundPeers)
            {
                this.LoadPeers(fedEndpoints, outboundPeers, "outbound", peers, federationMembers);
            }

            if (dataService.StatusResponse.Content.inboundPeers is JArray inboundPeers)
            {
                this.LoadPeers(fedEndpoints, inboundPeers, "inbound", peers, federationMembers);
            }
        }

        private void ParsePeers(NodeDataService dataService, List<Peer> peers)
        {
            if (dataService.StatusResponse.Content.outboundPeers is JArray outboundPeers)
            {
                this.LoadPeers(outboundPeers, "outbound", peers);
            }

            if (dataService.StatusResponse.Content.inboundPeers is JArray inboundPeers)
            {
                this.LoadPeers(inboundPeers, "inbound", peers);
            }
        }

        private void LoadPeers(JArray peersToProcess, string direction, List<Peer> peers)
        {
            foreach (dynamic peer in peersToProcess)
            {
                var peerToAdd = new Peer
                {
                    Endpoint = peer.remoteSocketEndpoint,
                    Type = direction,
                    Height = peer.tipHeight,
                    Version = peer.version
                };

                peers.Add(peerToAdd);
            }
        }

        private void LoadPeers(string fedEndpoints, JArray peersToProcess, string direction, List<Peer> peers, List<Peer> federationMembers)
        {
            foreach (dynamic peer in peersToProcess)
            {
                string peerIp = this.GetPeerIP(peer);
                var peerToAdd = new Peer
                {
                    Endpoint = peer.remoteSocketEndpoint,
                    Type = direction,
                    Height = peer.tipHeight,
                    Version = peer.version
                };

                if (fedEndpoints.Contains(peerIp))
                    federationMembers.Add(peerToAdd);
                else
                    peers.Add(peerToAdd);
            }
        }

        private string GetPeerIP(dynamic peer)
        {
            var endpointRegex = new Regex("\\[([A-Za-z0-9:.]*)\\]:([0-9]*)");
            MatchCollection endpointMatches = endpointRegex.Matches(Convert.ToString(peer.remoteSocketEndpoint));
            if (endpointMatches.Count <= 0 || endpointMatches[0].Groups.Count <= 1)
                return string.Empty;
            var endpoint = new IPEndPoint(IPAddress.Parse(endpointMatches[0].Groups[1].Value),
                int.Parse(endpointMatches[0].Groups[2].Value));

            return
                $"{endpoint.Address.MapToIPv4()}:{endpointMatches[0].Groups[2].Value}";
        }

        private async void DoWorkAsync(object state)
        {
            if (this.PerformNodeCheck())
            {
                await this.BuildCacheAsync();
                this.successfullyBuilt = true;
            }
            else
            {
                await this.distributedCache.SetStringAsync("NodeUnavailable", "true");
                if (this.successfullyBuilt)
                {
                    await this.updaterHub.Clients.All.SendAsync("NodeUnavailable");
                }
                await this.distributedCache.RemoveAsync("DashboardData");
                this.successfullyBuilt = false;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation($"Stopping the Fetching Background Service");
            this.dataRetrieverTimer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        /// <summary>
        /// When the service is disposed, the timer is disposed too
        /// </summary>
        public void Dispose()
        {
            this.dataRetrieverTimer?.Dispose();
        }

        /// <summary>
        /// Perform connection check with the nodes
        /// </summary>
        /// <remarks>The ports can be changed in the future</remarks>
        /// <returns>True if the connection are succeed</returns>
        private bool PerformNodeCheck()
        {
            var mainNodeUp = this.PortCheck(new Uri(this.defaultEndpointsSettings.StratisNode));
            var sidechainsNodeUp = this.PortCheck(new Uri(this.defaultEndpointsSettings.SidechainNode));
            return mainNodeUp && sidechainsNodeUp;
        }

        /// <summary>
        /// Perform a TCP port scan
        /// </summary>
        /// <param name="port">Specify the port to scan</param>
        /// <returns>True if the port is opened</returns>
        private bool PortCheck(Uri endpointToCheck)
        {
            this.logger.LogInformation($"Perform a port check for {endpointToCheck.Host}:{endpointToCheck.Port}");
            using (var tcpClient = new TcpClient())
            {
                try
                {
                    this.logger.LogInformation($"Host {endpointToCheck.Host}:{endpointToCheck.Port} is available");
                    tcpClient.Connect(endpointToCheck.Host, endpointToCheck.Port);
                    return true;
                }
                catch
                {
                    this.logger.LogWarning($"Host {endpointToCheck.Host}:{endpointToCheck.Port} unavailable");
                    return false;
                }
            }
        }
    }
}