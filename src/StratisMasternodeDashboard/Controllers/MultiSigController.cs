using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using QRCoder;
using Stratis.FederatedSidechains.AdminDashboard.Entities;
using Stratis.FederatedSidechains.AdminDashboard.Filters;
using Stratis.FederatedSidechains.AdminDashboard.Hubs;
using Stratis.FederatedSidechains.AdminDashboard.Models;
using Stratis.FederatedSidechains.AdminDashboard.Services;
using Stratis.FederatedSidechains.AdminDashboard.Settings;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace Stratis.FederatedSidechains.AdminDashboard.Controllers
{
    public sealed class MultiSigController : Controller
    {
        private readonly IDistributedCache distributedCache;
        private readonly DefaultEndpointsSettings defaultEndpointsSettings;
        private readonly IHubContext<DataUpdaterHub> updaterHub;

        public MultiSigController(IDistributedCache distributedCache, IHubContext<DataUpdaterHub> hubContext, DefaultEndpointsSettings defaultEndpointsSettings)
        {
            this.distributedCache = distributedCache;
            this.defaultEndpointsSettings = defaultEndpointsSettings;
            this.updaterHub = hubContext;
        }

        /// <summary>
        /// This is the Index action that return the dashboard if the local cache is built otherwise the initialization page is displayed
        /// </summary>
        public IActionResult Index()
        {
            var members = new List<MultiSigMemberModel>();
            members.Add(new MultiSigMemberModel() { MemberName = "Cirrus1", MemberPubKey = "03cfc06ef56352038e1169deb3b4fa228356e2a54255cf77c271556d2e2607c28c" });
            members.Add(new MultiSigMemberModel() { MemberName = "Cirrus3", MemberPubKey = "02fc828e06041ae803ab5378b5ec4e0def3d4e331977a69e1b6ef694d67f5c9c13" });
            members.Add(new MultiSigMemberModel() { MemberName = "Cirrus4", MemberPubKey = "02fd4f3197c40d41f9f5478d55844f522744258ca4093b5119571de1a5df1bc653" });
            return View("State", members);
        }
    }
}
