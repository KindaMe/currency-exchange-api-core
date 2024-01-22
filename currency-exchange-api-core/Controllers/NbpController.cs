using currency_exchange_api_core.Models;
using currency_exchange_api_core.Services;
using Microsoft.AspNetCore.Mvc;
using RestSharp;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace currency_exchange_api_core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NbpController : ControllerBase
    {
        private readonly CurrencyExchangeApiDbContext _context;
        private readonly NbpService _nbpService;

        public NbpController(CurrencyExchangeApiDbContext context, NbpService nbpService)
        {
            _context = context;
            _nbpService = nbpService;
        }

        // GET: api/<NbpController>
        [HttpGet("AllRates")]
        public async Task<ActionResult<NbpTable>> GetAllRates()
        {
            var table = await _nbpService.GetAllRates();

            if (table != null)
            {
                return table;
            }
            else
            {
                return NotFound();
            }
        }

        // GET api/<NbpController>/
        [HttpGet("AllRatesWithDate")]
        public async Task<ActionResult<NbpTable>> GetAllRatesWithDate(DateOnly date)
        {
            if (_nbpService.IsDateValid(date) == false)
            {
                return BadRequest();
            }

            RestClient client = new RestClient("http://api.nbp.pl/api");
            RestRequest restRequest = new RestRequest($"exchangerates/tables/a/{date.ToString("yyyy-MM-dd")}");

            var response = await client.ExecuteGetAsync<List<NbpTable>>(restRequest);

            if (response != null && response.StatusCode != System.Net.HttpStatusCode.NotFound && response.Data != null && response.Data.Count > 0 && response.Data[0].rates.Count > 0)
            {
                await _nbpService.MidRatesConverter(response.Data[0]);

                return response.Data[0];
            }
            else if (response != null && response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var things = await _nbpService.GetAllRatesWithDates(date.AddDays(-7), date);

                if (things != null)
                {
                    var sortedThings = things.OrderByDescending(table => table.effectiveDate);

                    var first = sortedThings.First();

                    await _nbpService.MidRatesConverter(first);

                    return first;
                }
            }

            return NotFound();
        }

        // GET api/<NbpController>/
        [HttpGet("AllRatesWithDates")]
        public async Task<ActionResult<List<NbpTable>>> GetAllRatesWithDates(DateOnly startDate, DateOnly endDate)
        {
            var tables = await _nbpService.GetAllRatesWithDates(startDate, endDate);

            if (tables != null)
            {
                return tables;
            }
            else
            {
                return NotFound();
            }
        }

        // GET api/<NbpController>/
        [HttpGet("CurrencyRatesWithDates")]
        public async Task<ActionResult<NbpTable>> GetCurrencyRatesWithDates(string currency, DateOnly startDate, DateOnly endDate)
        {
            if (_nbpService.IsDateValid(startDate) == false || _nbpService.IsDateValid(endDate) == false)
            {
                return BadRequest();
            }

            RestClient client = new RestClient("http://api.nbp.pl/api");
            RestRequest restRequest = new RestRequest($"exchangerates/rates/a/{currency}/{startDate.ToString("yyyy-MM-dd")}/{endDate.ToString("yyyy-MM-dd")}/");

            var response = await client.ExecuteGetAsync<NbpTable>(restRequest);

            if (response != null && response.Data != null && response.Data.rates.Count > 0)
            {
                await _nbpService.MidRatesConverter(response.Data);

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
            var table = await _nbpService.GetCurrentCurrencyRate(currency);

            if (table != null)
            {
                return table;
            }
            else
            {
                return NotFound();
            }
        }
    }
}