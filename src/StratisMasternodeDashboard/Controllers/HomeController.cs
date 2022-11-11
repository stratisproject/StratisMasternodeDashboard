using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using NBitcoin.JsonConverters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QRCoder;
using Stratis.FederatedSidechains.AdminDashboard.Entities;
using Stratis.FederatedSidechains.AdminDashboard.Filters;
using Stratis.FederatedSidechains.AdminDashboard.Helpers;
using Stratis.FederatedSidechains.AdminDashboard.Hubs;
using Stratis.FederatedSidechains.AdminDashboard.Models;
using Stratis.FederatedSidechains.AdminDashboard.Services;
using Stratis.FederatedSidechains.AdminDashboard.Settings;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Stratis.FederatedSidechains.AdminDashboard.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDistributedCache distributedCache;
        private readonly DefaultEndpointsSettings defaultEndpointsSettings;
        private readonly IHubContext<DataUpdaterHub> updaterHub;
        private readonly ApiRequester apiRequester;
        private readonly IConfiguration configuration;

        public HomeController(IDistributedCache distributedCache, IHubContext<DataUpdaterHub> hubContext, DefaultEndpointsSettings defaultEndpointsSettings, ApiRequester apiRequester, IConfiguration configuration)
        {
            this.distributedCache = distributedCache;
            this.defaultEndpointsSettings = defaultEndpointsSettings;
            this.updaterHub = hubContext;
            this.apiRequester = apiRequester;
            this.configuration = configuration;
        }

        /// <summary>
        /// Check if the federation is enabled, it's only called from the SignalR event
        /// </summary>
        /// <returns>True or False</returns>
        [Ajax]
        [Route("check-federation")]
        public async Task<IActionResult> CheckFederationAsync()
        {
            if (defaultEndpointsSettings.NodeType == NodeTypes.FiftyK)
            {
                ApiResponse getMainchainFederationInfo = await this.apiRequester.GetRequestAsync(this.defaultEndpointsSettings.MainchainNodeEndpoint, "/api/FederationGateway/info");
                if (getMainchainFederationInfo.IsSuccess)
                    return Json(getMainchainFederationInfo.Content.active);
            }

            return Json(true);
        }

        /// <summary>
        /// This is the Index action that return the dashboard if the local cache is built otherwise the initialization page is displayed
        /// </summary>
        public IActionResult Index()
        {
            DashboardModel dashboardModel = new();

            this.ViewBag.DisplayLoader = true;

            this.ViewBag.StratisTicker = DashboardModel.MainchainCoinTicker;
            this.ViewBag.SidechainTicker = DashboardModel.SidechainCoinTicker;
            this.ViewBag.MiningPubKeys = dashboardModel.MiningPublicKeys;
            this.ViewBag.Vote = null;
            this.ViewBag.SDAVote = new SDAVoteModel { };

            return View("Dashboard", dashboardModel);
        }

        [HttpGet]
        [Route("mainchaindata")]
        public async Task<IActionResult> MainchainData()
        {
            var nodeStatus = await GetNodeStatus(this.defaultEndpointsSettings.MainchainNodeEndpoint);

            var model = new StratisNodeModel
            {
                SwaggerUrl = UriHelper.BuildUri(this.defaultEndpointsSettings.MainchainNodeEndpoint, "/swagger").ToString(),
                MainchainNodeHeading = $"Mainchain Node [{nodeStatus.AgentVersion}]",
                MainchainNodeStarted = nodeStatus.NodeStartDateTime
            };

            if (this.defaultEndpointsSettings.NodeType == NodeTypes.FiftyK)
                model.FederationWalletHistory = await GetFederationWalletHistory(this.defaultEndpointsSettings.MainchainNodeEndpoint).ConfigureAwait(false);

            return PartialView("MainchainPartial", model);
        }

        [HttpGet]
        [Route("sidechaindata")]
        public async Task<IActionResult> SidechainData()
        {
            var nodeStatus = await GetNodeStatus(this.defaultEndpointsSettings.SidechainNodeEndpoint);

            var model = new SidechainNodeModel
            {
                SwaggerUrl = UriHelper.BuildUri(this.defaultEndpointsSettings.SidechainNodeEndpoint, "/swagger").ToString(),
                SidechainNodeHeading = $"Sidechain Node [{nodeStatus.AgentVersion}]",
                SidechainNodeStarted = nodeStatus.NodeStartDateTime
            };

            if (this.defaultEndpointsSettings.NodeType == NodeTypes.FiftyK)
                model.FederationWalletHistory = await GetFederationWalletHistory(this.defaultEndpointsSettings.SidechainNodeEndpoint).ConfigureAwait(false);

            return PartialView("SidechainPartial", model);
        }

        /// <summary>
        /// Display Qr code from text value
        /// </summary>
        [Route("qr-code/{value?}")]
        public IActionResult QrCode(string value)
        {
            QRCodeGenerator qrGenerator = new();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(value, QRCodeGenerator.ECCLevel.L);
            QRCode qrCode = new(qrCodeData);
            using var memoryStream = new MemoryStream();

            qrCode.GetGraphic(20).Save(memoryStream, ImageFormat.Png);
            return File(memoryStream.ToArray(), "image/png");
        }

        [Ajax]
        [Route("getConfiguration")]
        public IActionResult GetConfiguration(string sectionName, string paramName)
        {
            var parameterValue = configuration[$"{sectionName}:{paramName}"];
            return Json(new { parameter = parameterValue });
        }

        [Ajax]
        [Route("RefreshFederationWalletHistory")]
        [HttpGet]
        public async Task<IActionResult> RefreshFederationWalletHistory(bool isMainchain)
        {
            List<FederationWalletHistoryModel> result;
            if (isMainchain)
            {
                result = await GetFederationWalletHistory(this.defaultEndpointsSettings.MainchainNodeEndpoint).ConfigureAwait(false);
                return PartialView("FederationWalletHistory", new StratisNodeModel() { FederationWalletHistory = result });
            }
            else
            {
                result = await GetFederationWalletHistory(this.defaultEndpointsSettings.SidechainNodeEndpoint).ConfigureAwait(false);
                return PartialView("FederationWalletHistory", new SidechainNodeModel() { FederationWalletHistory = result });

            }
        }

        /// <summary>
        /// Shutdown the ASP.Net MVC Application
        /// </summary>
        [Route("shutdown")]
        public IActionResult Shutdown()
        {
            Environment.Exit(0);
            return Ok();
        }

        private async Task<NodeStatusModel> GetNodeStatus(string endpoint)
        {
            NodeStatusModel nodeStatus = new();
            ApiResponse response = await apiRequester.GetRequestAsync(endpoint, "/api/Node/status").ConfigureAwait(false);

            if (response.IsSuccess)
            {
                nodeStatus.AgentVersion = response.Content.version;
                long nodeStartedDateTime = response.Content.nodeStarted;

                nodeStatus.NodeStartDateTime = ConvertUnixTimeToDateTime(nodeStartedDateTime);

                if (endpoint == this.defaultEndpointsSettings.SidechainNodeEndpoint)
                {
                    this.distributedCache.SetString("SideChainNodeStats", JsonConvert.SerializeObject(nodeStatus));
                }

                if (endpoint == this.defaultEndpointsSettings.MainchainNodeEndpoint)
                {
                    this.distributedCache.SetString("MainChainNodeStats", JsonConvert.SerializeObject(nodeStatus));
                }
            }

            return nodeStatus;
        }

        private async Task<List<FederationWalletHistoryModel>> GetFederationWalletHistory(string endpoint)
        {
            List<FederationWalletHistoryModel> walletHistory = new();

            try
            {
                ApiResponse response = await apiRequester.GetRequestAsync(endpoint, "/api/FederationWallet/history", "maxEntriesToReturn=30").ConfigureAwait(false);
                walletHistory = Serializer.ToObject<List<FederationWalletHistoryModel>>((response.Content as JArray).ToString());
            }
            catch (Exception)
            {
            }

            return walletHistory;
        }

        public DateTime ConvertUnixTimeToDateTime(long unixtime)
        {
            System.DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixtime).ToUniversalTime();
            return dateTime;
        }

        public string GetCurrentUpTime(DateTime nodeStartedTime)
        {
            String uptime = (DateTime.UtcNow - nodeStartedTime).ToString();
            string[] parsenodeUpTime = uptime.Split('.');
            parsenodeUpTime = parsenodeUpTime.Take(parsenodeUpTime.Length - 1).ToArray();
            string nodeUptime = string.Join(".", parsenodeUpTime);

            return nodeUptime;
        }

    }
}
