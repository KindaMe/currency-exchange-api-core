using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace currency_exchange_api_core.Models;

public partial class Transaction
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    public int WalletId { get; set; }

    public string? CurrencyIn { get; set; }

    public decimal AmountIn { get; set; }

    public decimal? RateIn { get; set; }

    [JsonIgnore]
    public virtual Wallet Wallet { get; set; } = null!;
}