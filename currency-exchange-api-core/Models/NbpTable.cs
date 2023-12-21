using Microsoft.AspNetCore.Mvc.RazorPages;

namespace currency_exchange_api_core.Models
{
    public class NbpTable
    {
        public string? table { get; set; }
        public string? currency { get; set; }
        public string? code { get; set; }
        public string? no { get; set; }
        public string? effectiveDate { get; set; }
        public List<RateModel>? rates { get; set; }
    }

    public class RateModel
    {
        public string? currency { get; set; }
        public string? code { get; set; }
        public string? no { get; set; }
        public string? effectiveDate { get; set; }
        public double mid { get; set; }
        public double bid { get; set; }
        public double ask { get; set; }
    }
}