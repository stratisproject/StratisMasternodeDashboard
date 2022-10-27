using System;
using Newtonsoft.Json;

namespace Stratis.FederatedSidechains.AdminDashboard.Models
{
    public sealed class FederationWalletHistoryModel
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "depositId")]
        public string DepositId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public long Amount { get; set; }

        [JsonProperty(PropertyName = "payingTo")]
        public string PayingTo { get; set; }

        [JsonProperty(PropertyName = "blockHeight")]
        public int BlockHeight { get; set; }

        [JsonProperty(PropertyName = "blockHash")]
        public string BlockHash { get; set; }

        [JsonProperty(PropertyName = "signatureCount")]
        public int SignatureCount { get; set; }

        [JsonProperty(PropertyName = "transferStatus")]
        public string TransferStatus { get; set; }
    }
}
