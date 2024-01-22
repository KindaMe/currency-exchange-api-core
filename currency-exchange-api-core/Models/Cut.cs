namespace currency_exchange_api_core.Models;

public partial class Cut
{
    public int Id { get; set; }

    public decimal SellPercentage { get; set; }

    public decimal BuyPercentage { get; set; }

    public DateTime EffectiveDate { get; set; }
}