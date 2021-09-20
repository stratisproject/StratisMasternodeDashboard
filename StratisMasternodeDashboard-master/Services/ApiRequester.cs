using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        #region Kick
        protected async Task<List<FederationMember>> GetFederationMembers(string endpoint)
        {
            List<FederationMember> federationMembers = new List<FederationMember>();
            try
            {
                ApiResponse response = await GetRequestAsync(endpoint, "/api/Federation/members");
                federationMembers = JsonConvert.DeserializeObject<List<FederationMember>>(response.Content.ToString());
                return federationMembers;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to get Federation Members");
            }
            return federationMembers;
        }

        public async Task<IDGBKickModel> CheckAndKickFeberationMember(string endpoint, string pubKey)
        {
            bool isKeyAvailable = false;
            bool isSuccess = false;
            string message=null;
            IDGBKickModel iDBGKick = new IDGBKickModel();
            try
            {
                var members = await GetFederationMembers(endpoint);
                var member = members.FirstOrDefault(x => x.PubKey == pubKey);
                if (member != null)
                {
                    isKeyAvailable = true;                   
                    ApiResponse response = await PostRequestAsync(endpoint, "/api/Voting/schedulevote-kickmember", pubKey); 
                    if (response.IsSuccess)
                        isSuccess = true;
                    if (response.Content?.errors != null)
                        message = JsonConvert.DeserializeObject(response.Content?.errors[0].message);
                }
                iDBGKick = new IDGBKickModel()
                {
                    IsValidKey = isKeyAvailable,
                    IsResponseSuccess = isSuccess,
                    Message=message
                 };
                return iDBGKick;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to Validate and Kick the IDGB Member");
            }
            return iDBGKick;
        }
        #endregion
    }
}