using System.Text.Json.Serialization;

namespace currency_exchange_api_core.Models;

public partial class Conversion
{
    public int Id { get; set; }

    public int TransactionId { get; set; }

    public int Order { get; set; }

    public string? CurrencyBefore { get; set; }

    public decimal? AmountBefore { get; set; }

    public string? CurrencyAfter { get; set; }

    public decimal? AmountAfter { get; set; }

    public decimal? Rate { get; set; }

    [JsonIgnore] public virtual Transaction Transaction { get; set; } = null!;
}