using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using Stratis.FederatedSidechains.AdminDashboard.Entities;
using Stratis.FederatedSidechains.AdminDashboard.Helpers;
using Stratis.FederatedSidechains.AdminDashboard.Models;

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
            IRestResponse restResponse = await restClient.ExecuteTaskAsync(restRequest);
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
            var restClient = new RestClient(UriHelper.BuildUri(endpoint, path));
            var restRequest = new RestRequest(method);
            restRequest.AddHeader("Content-type", "application/json");
            restRequest.AddJsonBody(body);
            IRestResponse restResponse = await restClient.ExecuteTaskAsync(restRequest);
            var isSuccess = restResponse.StatusCode.Equals(HttpStatusCode.OK);
            return new ApiResponse
            {
                IsSuccess = isSuccess,
                Content = JsonConvert.DeserializeObject(restResponse.Content)
            };
        }

        #region SDA Proposal Voting
        private SDAVoteContractCall sDAVoteContractCall;
        private string accountName = "account 0";
        public async Task<ApiResponse> VoteSDAProposalSmartContractCall(string endpoint, SDAVoteModel sDAVote)
        {
            string senderAddress = null;
            List<WalletAddress> walletAddresses = new List<WalletAddress>();
            try
            {
                ApiResponse responseWalletAddress = await GetRequestAsync(endpoint, "/api/Wallet/addresses", $"WalletName={sDAVote.WalletName}" + "&" + $"AccountName={accountName}");
                if (responseWalletAddress.IsSuccess)
                {
                    var items = JsonConvert.DeserializeObject(responseWalletAddress.Content.addresses.ToString());
                    walletAddresses = JsonConvert.DeserializeObject<List<WalletAddress>>(responseWalletAddress.Content.addresses.ToString());

                    var usedWalletAddress = walletAddresses.FindAll(x => x.IsUsed).FirstOrDefault();

                    senderAddress = usedWalletAddress.Address;
                    sDAVoteContractCall = new SDAVoteContractCall
                    {
                        GasPrice = 100,
                        GasLimit = 50000,
                        WalletName = sDAVote.WalletName,
                        Password = sDAVote.WalletPassword,
                        Amount = 0,
                        FeeAmount = 0.001,
                        MethodName = "Vote",
                        AccountName = accountName,
                        ContractAddress = "tSSDFN88s3mLpQbHVMA3GYhwjWah6gW8ss",
                        Sender = senderAddress,
                        Parameters = new string[] { "5#" + sDAVote.ProposalId, "1#" + sDAVote.VotingDecision },
                    };
                    ApiResponse response = await PostRequestAsync(endpoint, "/api/SmartContracts/build-and-send-call", sDAVoteContractCall);
                    return response;
                }
                else
                    return responseWalletAddress;

            }
            catch (Exception ex)
            {

                this.logger.LogError(ex, "Failed to vote SDA Proposal");
            }
            return null;

        }

        #endregion
    }
}