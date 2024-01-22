namespace currency_exchange_api_core.Models
{
    public class Country
    {
        public Flags Flags { get; set; }
        public Dictionary<string, Currency> Currencies { get; set; }
    }

    public class Flags
    {
        public string Png { get; set; }
        public string Svg { get; set; }
        public string Alt { get; set; }
    }

    public class Currency
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
    }
}