using Microsoft.AspNetCore.Mvc;
using Stratis.FederatedSidechains.AdminDashboard.Entities;
using Stratis.FederatedSidechains.AdminDashboard.Filters;
using Stratis.FederatedSidechains.AdminDashboard.Models;
using Stratis.FederatedSidechains.AdminDashboard.Services;
using Stratis.FederatedSidechains.AdminDashboard.Settings;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Stratis.FederatedSidechains.AdminDashboard.Controllers
{
    [Route("sidechain-node")]
    public class SidechainNodeController : Controller
    {
        private readonly DefaultEndpointsSettings defaultEndpointsSettings;
        private readonly ApiRequester apiRequester;

        public SidechainNodeController(DefaultEndpointsSettings defaultEndpointsSettings, ApiRequester apiRequester)
        {
            this.defaultEndpointsSettings = defaultEndpointsSettings;
            this.apiRequester = apiRequester;
        }

        [Ajax]
        [Route("enable-federation")]
        public async Task<IActionResult> EnableFederationAsync(string mnemonic, string password)
        {
            ApiResponse importWalletRequest = await this.apiRequester.PostRequestAsync(this.defaultEndpointsSettings.SidechainNodeEndpoint, "/api/FederationWallet/import-key", new { mnemonic, password });
            if (importWalletRequest.IsSuccess)
            {
                ApiResponse enableFederationRequest = await this.apiRequester.PostRequestAsync(this.defaultEndpointsSettings.SidechainNodeEndpoint, "/api/FederationWallet/enable-federation", new { password });
                return enableFederationRequest.IsSuccess ? (IActionResult)Ok() : BadRequest();
            }
            return BadRequest();
        }

        [Ajax]
        [HttpPost]
        [Route("resync")]
        public async Task<IActionResult> ResyncAsync(string value)
        {
            bool isHeight = int.TryParse(value, out _);
            if (isHeight)
            {
                ApiResponse getblockhashRequest = await this.apiRequester.GetRequestAsync(this.defaultEndpointsSettings.SidechainNodeEndpoint, $"/api/Consensus/getblockhash?height={value}");
                ApiResponse syncRequest = await this.apiRequester.PostRequestAsync(this.defaultEndpointsSettings.SidechainNodeEndpoint, "/api/Wallet/sync", new { hash = ((string)getblockhashRequest.Content) });
                return syncRequest.IsSuccess ? (IActionResult)Ok() : BadRequest();
            }
            else
            {
                ApiResponse syncRequest = await this.apiRequester.PostRequestAsync(this.defaultEndpointsSettings.SidechainNodeEndpoint, "/api/Wallet/sync", new { hash = value });
                return syncRequest.IsSuccess ? (IActionResult)Ok() : BadRequest();
            }
        }

        [Ajax]
        [Route("resync-crosschain-transactions")]
        public async Task<IActionResult> ResyncCrosschainTransactionsAsync()
        {
            //TODO: implement this method
            ApiResponse stopNodeRequest = await this.apiRequester.GetRequestAsync(this.defaultEndpointsSettings.MainchainNodeEndpoint, "/api/Node/status");
            return stopNodeRequest.IsSuccess ? (IActionResult)Ok() : BadRequest();
        }

        [Ajax]
        [Route("stop")]
        public async Task<IActionResult> StopNodeAsync()
        {
            ApiResponse stopNodeRequest = await this.apiRequester.PostRequestAsync(this.defaultEndpointsSettings.SidechainNodeEndpoint, "/api/Node/stop", true);
            return stopNodeRequest.IsSuccess ? (IActionResult)Ok() : BadRequest();
        }

        [Ajax]
        [Route("change-log-level/{level}")]
        public async Task<IActionResult> ChangeLogLevelAsync(string rule, string level)
        {
            ApiResponse changeLogLevelRequest = await this.apiRequester.PostRequestAsync(this.defaultEndpointsSettings.MainchainNodeEndpoint, "/api/Node/loglevels", new { logRules = new[] { new { ruleName = rule, logLevel = level } } });
            return changeLogLevelRequest.IsSuccess ? (IActionResult)Ok() : BadRequest();
        }

        [Ajax]
        [HttpPost]
        [Route("vote")]
        public async Task<IActionResult> Vote(Vote vote)
        {
            if (string.IsNullOrEmpty(vote?.Hash))
                return this.BadRequest("Hash is required");

            ApiResponse response = await this.apiRequester.PostRequestAsync(this.defaultEndpointsSettings.SidechainNodeEndpoint, "/api/Voting/schedulevote-whitelisthash", new { hash = vote.Hash });

            if (response.IsSuccess)
                return this.Ok();

            if (response.Content?.errors != null)
            {
                return this.BadRequest($"Failed to whitelist hash. Reason: {response.Content?.errors[0].message}");
            }

            return this.BadRequest($"Failed to whitelist hash. Reason: {response.Content}");
        }

        [HttpPost]
        [Route("schedulekick")]
        public async Task<IActionResult> ScheduleKick([FromBody] string pubKey)
        {
            if (string.IsNullOrEmpty(pubKey))
                return this.BadRequest("Member key is required");

            ApiResponse response = await this.apiRequester.PostRequestAsync(this.defaultEndpointsSettings.SidechainNodeEndpoint, "/api/Voting/schedulevote-kickmember", new { pubkey = pubKey }).ConfigureAwait(false);
            if (response.IsSuccess)
                return this.Ok();

            if (response.Content?.errors != null)
                return this.BadRequest($"An error occurred trying to schedule a kick federation member vote: {response.Content?.errors[0].message}");

            return this.BadRequest($"An error occurred trying to schedule a kick federation member vote: {response.Content}");
        }

        [HttpPost]
        [Route("sdavote")]
        public async Task<IActionResult> VoteSDAProposal([FromBody] SDAVoteModel sDAVote)
        {
            if (!ModelState.IsValid)
                return this.BadRequest("Please enter all the required values.");

            ApiResponse response = await this.apiRequester.VoteSDAProposalSmartContractCall(this.defaultEndpointsSettings, sDAVote);

            if (response == null || response.Content == null)
                return this.BadRequest("An error occurred trying to vote, please try again.");

            if (!response.IsSuccess || response.IsSuccess && response.Content.transactionId == null)
                return this.BadRequest(GetBadResponseMessage(response));

            var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(5));

            ApiResponse responseReceipt;

            do
            {
                responseReceipt = await this.apiRequester.GetRequestAsync(this.defaultEndpointsSettings.SidechainNodeEndpoint, "/api/SmartContracts/receipt", $"txHash={response.Content.transactionId}").ConfigureAwait(false);

                if (!responseReceipt.IsSuccess)
                {
                    if (cancellation.IsCancellationRequested)
                        return this.BadRequest($"The request timed out getting receipt for '{response.Content.transactionId}'");

                    await Task.Delay(TimeSpan.FromSeconds(5));

                    continue;
                }

                if ((bool)responseReceipt.Content.success)
                    return this.Ok();
                else
                    return this.BadRequest((string)responseReceipt.Content.error);

            } while (true);
        }

        private string GetBadResponseMessage(ApiResponse apiResponse)
        {
            if (apiResponse.Content?.errors != null)
                return $"An error occurred: {apiResponse.Content?.errors[0].message}";

            if (apiResponse.Content?.message != null)
                return $"An error occurred: {apiResponse.Content?.message}";

            return $"An error occurred: {apiResponse.Content}";
        }
    }
}