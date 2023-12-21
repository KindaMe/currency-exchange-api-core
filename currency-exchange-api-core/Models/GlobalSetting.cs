using System;
using System.Collections.Generic;

namespace currency_exchange_api_core.Models;

public partial class GlobalSetting
{
    public int Id { get; set; }

    public decimal SellPercentageCut { get; set; }

    public decimal BuyPercentageCut { get; set; }
}
