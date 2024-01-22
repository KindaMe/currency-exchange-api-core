using currency_exchange_api_core.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace currency_exchange_api_core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly CountriesService _countriesService;

        public CountriesController(CountriesService countriesService)
        {
            _countriesService = countriesService;
        }

        // GET: api/<RestCountriesController>
        [HttpGet("FlagUrlByCurrencyCode")]
        public async Task<ActionResult<string>> GetCountryFlagUrlByCurrencyCode(string code)
        {
            var flagUrl = await _countriesService.GetFlagUrl(code);

            if (flagUrl != string.Empty)
            {
                return flagUrl;
            }
            else
            {
                return NotFound();
            }
        }
    }
}