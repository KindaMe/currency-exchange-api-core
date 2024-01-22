using currency_exchange_api_core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace currency_exchange_api_core.Models;

public partial class Transaction
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    [Column(TypeName = "nvarchar(50)")]
    public TransactionTypes? Type { get; set; }

    public int? WalletFromId { get; set; }

    public int? WalletToId { get; set; }

    public virtual ICollection<Conversion> Conversions { get; set; } = new List<Conversion>();

    [JsonIgnore] public virtual Wallet? WalletFrom { get; set; }

    [JsonIgnore] public virtual Wallet? WalletTo { get; set; }
}