using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace currency_exchange_api_core.Models;

public partial class Wallet
{
    public int Id { get; set; }

    public string Currency { get; set; } = null!;

    public int UserId { get; set; }

    public virtual ICollection<Transaction> TransactionWalletFroms { get; set; } = new List<Transaction>();

    public virtual ICollection<Transaction> TransactionWalletTos { get; set; } = new List<Transaction>();

    [JsonIgnore] public virtual User User { get; set; } = null!;

    [NotMapped] public string FlagUrl { get; set; } = null!;
}