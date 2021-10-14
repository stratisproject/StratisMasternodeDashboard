using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using Stratis.FederatedSidechains.AdminDashboard.Entities;
using Stratis.FederatedSidechains.AdminDashboard.Helpers;
using Stratis.FederatedSidechains.AdminDashboard.Models;
using Stratis.FederatedSidechains.AdminDashboard.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Stratis.FederatedSidechains.AdminDashboard.Services
{
    public class ApiRequester
    {
        private readonly ILogger<ApiRequester> logger;

        public ApiRequester(ILogger<ApiRequester> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Make a HTTP request to a specified node
        /// </summary>
        /// <param name="endpoint">HTTP node endpoint</param>
        /// <param name="path">URL</param>
        /// <returns>An ApiResponse object</returns>
        public async Task<ApiResponse> GetRequestAsync(string endpoint, string path, string query = null)
        {
            var restClient = new RestClient(UriHelper.BuildUri(endpoint, path, query));
            var restRequest = new RestRequest(Method.GET);
            IRestResponse restResponse = await restClient.ExecuteTaskAsync(restRequest).ConfigureAwait(false);
            var isSuccess = restResponse.StatusCode.Equals(HttpStatusCode.OK);
            return new ApiResponse
            {
                IsSuccess = isSuccess,
                Content = JsonConvert.DeserializeObject(restResponse.Content)
            };
        }

        /// <summary>
        /// Make a HTTP request with POST method
        /// </summary>
        /// <param name="endpoint">HTTP node endpoint</param>
        /// <param name="path">URL</param>
        /// <param name="body">Specify the body request</param>
        /// <returns>An ApiResponse object</returns>
        public async Task<ApiResponse> PostRequestAsync(string endpoint, string path, object body, Method method = Method.POST)
        {
            using (var cancellationToken = new CancellationTokenSource(TimeSpan.FromMinutes(5)))
            {
                var restClient = new RestClient(UriHelper.BuildUri(endpoint, path));
                var restRequest = new RestRequest(method);
                restRequest.AddHeader("Content-type", "application/json");
                restRequest.AddJsonBody(body);

                IRestResponse restResponse = await restClient.ExecuteTaskAsync(restRequest, cancellationToken.Token).ConfigureAwait(false);

                var isSuccess = restResponse.StatusCode.Equals(HttpStatusCode.OK);

                return new ApiResponse
                {
                    IsSuccess = isSuccess,
                    Content = JsonConvert.DeserializeObject(restResponse.Content)
                };
            }
        }

        #region SDA Proposal Voting

        public async Task<ApiResponse> VoteSDAProposalSmartContractCall(DefaultEndpointsSettings settings, SDAVoteModel sDAVote)
        {
            try
            {
                List<WalletAddress> walletAddresses = new List<WalletAddress>();

                ApiResponse responseWalletAddress = await GetRequestAsync(settings.SidechainNode, "/api/Wallet/addresses", $"WalletName={sDAVote.WalletName}" + "&" + $"AccountName=account 0").ConfigureAwait(false);

                if (responseWalletAddress.IsSuccess)
                {
                    var items = JsonConvert.DeserializeObject(responseWalletAddress.Content.addresses.ToString());
                    walletAddresses = JsonConvert.DeserializeObject<List<WalletAddress>>(responseWalletAddress.Content.addresses.ToString());

                    var usedWalletAddress = walletAddresses.FindAll(x => x.IsUsed).FirstOrDefault();

                    var sDAVoteContractCall = new SDAVoteContractCall
                    {
                        GasPrice = 100,
                        GasLimit = 50000,
                        WalletName = sDAVote.WalletName,
                        Password = sDAVote.WalletPassword,
                        Amount = 0,
                        FeeAmount = 0.001,
                        MethodName = "Vote",
                        AccountName = "account 0",
                        ContractAddress = settings.SDADaoContractAddress,
                        Sender = usedWalletAddress.Address,
                        Parameters = new string[] { "5#" + sDAVote.ProposalId, "1#" + sDAVote.VotingDecision },
                    };

                    ApiResponse response = await PostRequestAsync(settings.SidechainNode, "/api/SmartContracts/build-and-send-call", sDAVoteContractCall).ConfigureAwait(false);

                    return response;
                }
                else
                    return responseWalletAddress;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to vote on the SDA Proposal");
            }

            return null;
        }

        #endregion
    }
}