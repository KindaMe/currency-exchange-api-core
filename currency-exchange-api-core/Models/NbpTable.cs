namespace currency_exchange_api_core.Models
{
    public class NbpTable
    {
        public string? table { get; set; }
        public string? currency { get; set; }
        public string? code { get; set; }
        public string? no { get; set; }
        public string? effectiveDate { get; set; }
        public List<RateModel> rates { get; set; } = new();
        public string? flagUrl { get; set; }
    }

    public class RateModel
    {
        public string? currency { get; set; }
        public string? code { get; set; }
        public string? no { get; set; }
        public string? effectiveDate { get; set; }
        public decimal mid { get; set; }
        public decimal bid { get; set; }
        public decimal ask { get; set; }
        public string? flagUrl { get; set; }
    }
}