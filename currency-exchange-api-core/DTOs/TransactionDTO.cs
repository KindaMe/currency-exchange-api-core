using System.Text.Json.Serialization;
using currency_exchange_api_core.Enums;

namespace currency_exchange_api_core.DTOs
{
    public class TransactionDTO
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TransactionTypes Type { get; set; }

        public int? FromWalletId { get; set; }
        public int? ToWalletId { get; set; }
        public decimal Amount { get; set; }
    }
}