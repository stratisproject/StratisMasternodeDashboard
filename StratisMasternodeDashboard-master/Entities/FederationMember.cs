using Newtonsoft.Json;
using System;

namespace Stratis.FederatedSidechains.AdminDashboard.Entities
{
    public class FederationMember
    {
        [JsonProperty("pubkey")]
        public string PubKey { get; set; }

        [JsonProperty("collateralAmount")]
        public decimal CollateralAmount { get; set; }

        [JsonProperty("lastActiveTime")]
        public DateTime? LastActiveTime { get; set; }

        [JsonProperty("periodOfInactivity")]
        public TimeSpan? PeriodOfInActivity { get; set; }
    }
}
