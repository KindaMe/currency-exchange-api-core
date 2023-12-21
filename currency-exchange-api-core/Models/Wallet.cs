using System;
using System.Collections.Generic;

namespace currency_exchange_api_core.Models;

public partial class Wallet
{
    public int Id { get; set; }

    public decimal? Balance { get; set; }

    public string Currency { get; set; } = null!;

    public int UserId { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual User User { get; set; } = null!;
}
