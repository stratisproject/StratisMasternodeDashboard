using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using QRCoder;
using Stratis.FederatedSidechains.AdminDashboard.Entities;
using Stratis.FederatedSidechains.AdminDashboard.Filters;
using Stratis.FederatedSidechains.AdminDashboard.Hubs;
using Stratis.FederatedSidechains.AdminDashboard.Models;
using Stratis.FederatedSidechains.AdminDashboard.Services;
using Stratis.FederatedSidechains.AdminDashboard.Settings;
using System;
using System.Drawing.Imaging;
using System.IO;
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
            if (defaultEndpointsSettings.SidechainNodeType == NodeTypes.FiftyK)
            {
                ApiResponse getMainchainFederationInfo = await this.apiRequester.GetRequestAsync(this.defaultEndpointsSettings.StratisNode, "/api/FederationGateway/info");
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
            if (string.IsNullOrEmpty(this.distributedCache.GetString("DashboardData")))
            {
                var nodeUnavailable = !string.IsNullOrEmpty(this.distributedCache.GetString("NodeUnavailable"));
                this.ViewBag.NodeUnavailable = nodeUnavailable;
                this.ViewBag.Status = nodeUnavailable ? "API Unavailable" : "Initialization...";
                return View("Initialization");
            }

            DashboardModel dashboardModel = JsonConvert.DeserializeObject<DashboardModel>(this.distributedCache.GetString("DashboardData"));

            this.ViewBag.DisplayLoader = true;

            if (dashboardModel != null && dashboardModel.MainchainNode != null && dashboardModel.SidechainNode != null)
                this.ViewBag.History = new[] { dashboardModel.MainchainNode.History, dashboardModel.SidechainNode.History };
            else
                this.ViewBag.History = null;

            this.ViewBag.StratisTicker = DashboardModel.MainchainCoinTicker;
            this.ViewBag.SidechainTicker = DashboardModel.SidechainCoinTicker;
            this.ViewBag.MiningPubKeys = dashboardModel.MiningPublicKeys;

            this.ViewBag.LogRules = new LogRulesModel().LoadRules(dashboardModel.MainchainNode?.LogRules ?? null, dashboardModel.SidechainNode?.LogRules ?? null);

            this.ViewBag.Vote = null;

            if (dashboardModel.SidechainNode != null)
                this.ViewBag.Vote = new Vote { Polls = dashboardModel.SidechainNode.PoAPendingPolls, FederationMemberCount = dashboardModel.SidechainNode.FederationMemberCount, KickFederationMemberPolls = dashboardModel.SidechainNode.KickFederationMemberPolls };

            this.ViewBag.SDAVote = new SDAVoteModel { };

            if (dashboardModel.MainchainNode == null || dashboardModel.SidechainNode == null)
                this.ViewBag.Status = "API Unavailable";
            else
                this.ViewBag.Status = "OK";

            return View("Dashboard", dashboardModel);
        }

        /// <summary>
        /// This action redraw the dashboard with the new cached datas, it's only called from the SignalR event
        /// </summary>
        [Ajax]
        [Route("update-dashboard")]
        public IActionResult UpdateDashboard()
        {
            if (!string.IsNullOrEmpty(this.distributedCache.GetString("DashboardData")))
            {
                DashboardModel dashboardModel = JsonConvert.DeserializeObject<DashboardModel>(this.distributedCache.GetString("DashboardData"));

                if (dashboardModel.MainchainNode != null && dashboardModel.SidechainNode != null)
                    this.ViewBag.History = new[] { dashboardModel.MainchainNode.History, dashboardModel.SidechainNode.History };
                else
                    this.ViewBag.History = null;

                this.ViewBag.StratisTicker = DashboardModel.MainchainCoinTicker;
                this.ViewBag.SidechainTicker = DashboardModel.SidechainCoinTicker;

                return PartialView("Dashboard", dashboardModel);
            }
            return NoContent();
        }

        /// <summary>
        /// Display Qr code from text value
        /// </summary>
        [Route("qr-code/{value?}")]
        public IActionResult QrCode(string value)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(value, QRCodeGenerator.ECCLevel.L);
            QRCode qrCode = new QRCode(qrCodeData);
            using (var memoryStream = new MemoryStream())
            {
                qrCode.GetGraphic(20).Save(memoryStream, ImageFormat.Png);
                return File(memoryStream.ToArray(), "image/png");
            }
        }

        [Ajax]
        [Route("getConfiguration")]
        public IActionResult GetConfiguration(string sectionName, string paramName)
        {
            var parameterValue = configuration[$"{sectionName}:{paramName}"];
            return Json(new { parameter = parameterValue });
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
    }
}
