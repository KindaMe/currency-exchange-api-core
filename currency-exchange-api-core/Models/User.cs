using System;
using System.Collections.Generic;

namespace currency_exchange_api_core.Models;

public partial class User
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual ICollection<Wallet> Wallets { get; set; } = new List<Wallet>();
}
