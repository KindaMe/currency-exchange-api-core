using currency_exchange_api_core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestSharp;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace currency_exchange_api_core.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NbpController : ControllerBase
    {
        private readonly CurrencyExchangeApiDbContext _context;

        public NbpController(CurrencyExchangeApiDbContext context)
        {
            _context = context;
        }

        // GET: api/<NbpController>
        [HttpGet("AllRates")]
        public async Task<ActionResult<NbpTable>> GetAllRates()
        {
            RestClient client = new RestClient("http://api.nbp.pl/api");
            RestRequest restRequest = new RestRequest("exchangerates/tables/a");

            var response = await client.ExecuteGetAsync<List<NbpTable>>(restRequest);

            if (response != null && response.Data != null && response.Data.Count > 0)
            {
                MidRatesConverter(response.Data[0]);

                return response.Data[0];
            }
            else
            {
                return NotFound();
            }
        }

        // GET api/<NbpController>/
        [HttpGet("AllRatesWithDate")]
        public async Task<ActionResult<NbpTable>> GetAllRatesWithDate(string date)
        {
            RestClient client = new RestClient("http://api.nbp.pl/api");
            RestRequest restRequest = new RestRequest($"exchangerates/tables/a/{date}");

            var response = await client.ExecuteGetAsync<List<NbpTable>>(restRequest);

            if (response != null && response.Data != null && response.Data.Count > 0)
            {
                MidRatesConverter(response.Data[0]);

                return response.Data[0];
            }
            else
            {
                return NotFound();
            }
        }

        // GET api/<NbpController>/
        [HttpGet("CurrencyRatesWithDates")]
        public async Task<ActionResult<NbpTable>> GetCurrencyRatesWithDates(string currency, string startDate, string endDate)
        {
            RestClient client = new RestClient("http://api.nbp.pl/api");
            RestRequest restRequest = new RestRequest($"exchangerates/rates/a/{currency}/{startDate}/{endDate}/");

            var response = await client.ExecuteGetAsync<NbpTable>(restRequest);

            if (response != null && response.Data != null)
            {
                MidRatesConverter(response.Data);

                return response.Data;
            }
            else
            {
                return NotFound();
            }
        }

        // GET api/<NbpController>/
        [HttpGet("CurrentCurrencyRate")]
        public async Task<ActionResult<NbpTable>> GetCurrentCurrencyRate(string currency)
        {
            RestClient client = new RestClient("http://api.nbp.pl/api");
            RestRequest restRequest = new RestRequest($"exchangerates/rates/a/{currency}/");

            var response = await client.ExecuteGetAsync<NbpTable>(restRequest);

            if (response != null && response.Data != null)
            {
                MidRatesConverter(response.Data);

                return response.Data;
            }
            else
            {
                return NotFound();
            }
        }

        private void MidRatesConverter(NbpTable table)
        {
            if (table.rates != null && _context != null)
            {
                double buyPercentageCut = 0;
                double sellPercentageCut = 0;

                var globalSettings = _context.GlobalSettings.Find(1);
                if (globalSettings != null)
                {
                    buyPercentageCut = (double)globalSettings.BuyPercentageCut;
                    sellPercentageCut = (double)globalSettings.SellPercentageCut;
                }

                foreach (var rate in table.rates)
                {
                    rate.bid = Math.Round(rate.mid + (rate.mid * (buyPercentageCut / 100)), 8);
                    rate.ask = Math.Round(rate.mid - (rate.mid * (sellPercentageCut / 100)), 8);
                }
            }
        }
    }
}