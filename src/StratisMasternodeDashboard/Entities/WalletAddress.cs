using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stratis.FederatedSidechains.AdminDashboard.Entities
{
    public class WalletAddress
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("isUsed")]
        public bool IsUsed { get; set; }

        [JsonProperty("isChange")]
        public bool IsChange { get; set; }

        [JsonProperty("amountConfirmed")]
        public decimal AmountConfirmed { get; set; }

        [JsonProperty("amountUnconfirmed")]
        public decimal AmountUnconfirmed { get; set; }
    }
}
