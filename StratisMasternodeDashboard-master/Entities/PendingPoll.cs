using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Stratis.FederatedSidechains.AdminDashboard.Entities
{
    public class PendingPoll
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("isPending")]
        public bool IsPending { get; set; }

        [JsonProperty("isExecuted")]
        public bool IsExecuted { get; set; }

        [JsonProperty("pollVotedInFavorBlockData")]
        public string PollVotedInFavorBlockData { get; set; }

        [JsonProperty("pollStartBlockData")]
        public string PollStartBlockData { get; set; }

        [JsonProperty("pollExecutedBlockData")]
        public string PollExecutedBlockData { get; set; }

        [JsonProperty("pubKeysHexVotedInFavor")]
        public List<string> PubKeysHexVotedInFavor { get; set; }

        [JsonProperty("votingDataString")]
        public string VotingDataString { get; set; }

        [JsonIgnore]
        public string Hash
        {
            get
            {
                if (string.IsNullOrEmpty(this.VotingDataString)) return string.Empty;
                string[] tokens = this.VotingDataString.Split(',');
                if (tokens.Length < 1) return string.Empty;
                string hashToken = tokens.FirstOrDefault(t => t.StartsWith("hash", StringComparison.OrdinalIgnoreCase));
                if (hashToken == null) return string.Empty;
                string[] hashTokens = hashToken.Split(':');
                return hashTokens.Length < 2 ? string.Empty : hashTokens[1].Replace("'", string.Empty);
            }
        }

        [JsonIgnore]
        public string PubKey
        {
            get
            {
                string pubKey = GetPropertyValue("pubkey", 2);
                return pubKey;
            }
        }

        [JsonIgnore]
        public string CollateralAmount
        {
            get
            {
                string collateralAmount = GetPropertyValue("collateralamount", 1);
                return collateralAmount;
            }
        }

        [JsonIgnore]
        public string CollateralAddress
        {
            get
            {
                string collateralAddress = GetPropertyValue("collateralmainchainaddress", 1);
                return collateralAddress;
            }
        }

        public string GetPropertyValue(string contain, int index)
        {
            if (string.IsNullOrEmpty(this.VotingDataString)) return string.Empty;
            string[] tokens = this.VotingDataString.Split(',');
            if (tokens.Length < 1) return string.Empty;
            string propertyToken = tokens.FirstOrDefault(t => t.Contains(contain, StringComparison.OrdinalIgnoreCase));
            if (propertyToken == null) return string.Empty;
            string[] propertyTokens = propertyToken.Split(':');
            return propertyTokens.Length < 2 ? string.Empty : propertyTokens[index].Replace("'", string.Empty);
        }
    }

    public class ApprovedPoll
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }
    }

    public class HashHeightPairModel
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }
}
