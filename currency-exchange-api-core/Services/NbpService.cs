using currency_exchange_api_core.Models;
using RestSharp;

namespace currency_exchange_api_core.Services
{
    public class NbpService
    {
        private readonly CurrencyExchangeApiDbContext _context;
        private readonly CountriesService _countriesService;

        public NbpService(CurrencyExchangeApiDbContext context, CountriesService countriesService)
        {
            _context = context;
            _countriesService = countriesService;
        }

        public async Task MidRatesConverter(NbpTable table)
        {
            if (table.rates == null) return;

            foreach (var rate in table.rates)
            {
                var rateDate = !string.IsNullOrEmpty(rate.effectiveDate)
                                       ? DateTime.Parse(rate.effectiveDate)
                                       : DateTime.Parse(table.effectiveDate!);

                rateDate = new DateTime(rateDate.Year, rateDate.Month, rateDate.Day, 23, 59, 59, 999);

                var cut = _context.Cuts
                    .Where(c => c.EffectiveDate <= rateDate)
                    .OrderByDescending(c => c.EffectiveDate)
                    .FirstOrDefault();

                if (cut != null)
                {
                    rate.bid = Math.Round(rate.mid - (rate.mid * (cut.BuyPercentage / 100)), 8);
                    rate.ask = Math.Round(rate.mid + (rate.mid * (cut.SellPercentage / 100)), 8);
                }
            }

            var countries = await _countriesService.GetAllCountriesWithFlagsAndCurrencies();

            if (countries != null)
            {
                if (table.code != null)
                {
                    var appearances = countries.SelectMany(country => country.Currencies.Keys).Count(currencyKey => currencyKey == table.code);

                    var countryWithCode = countries.FirstOrDefault(c => c.Currencies.ContainsKey(table.code));

                    if (appearances != 1)
                    {
                        table.flagUrl = "https://media.wired.co.uk/photos/606da9af687a704c2c361d4b/1:1/w_1280,h_1280,c_limit/Flag_20x30.jpg";
                    }
                    else
                    {
                        table.flagUrl = countryWithCode != null
                            ? countryWithCode.Flags.Png
                            : "https://media.wired.co.uk/photos/606da9af687a704c2c361d4b/1:1/w_1280,h_1280,c_limit/Flag_20x30.jpg";
                    }

                    table.currency = countryWithCode != null ? countryWithCode.Currencies[table.code].Name : string.Empty;
                }
                else
                {
                    foreach (var rate in table.rates)
                    {
                        var appearances = countries.SelectMany(country => country.Currencies.Keys).Count(currencyKey => currencyKey == rate.code);

                        var countryWithCode = countries.FirstOrDefault(c => c.Currencies.ContainsKey(rate.code!));

                        if (appearances != 1)
                        {
                            rate.flagUrl = "https://media.wired.co.uk/photos/606da9af687a704c2c361d4b/1:1/w_1280,h_1280,c_limit/Flag_20x30.jpg";
                        }
                        else
                        {
                            rate.flagUrl = countryWithCode != null
                                ? countryWithCode.Flags.Png
                                : "https://media.wired.co.uk/photos/606da9af687a704c2c361d4b/1:1/w_1280,h_1280,c_limit/Flag_20x30.jpg";
                        }

                        rate.currency = countryWithCode != null ? countryWithCode.Currencies[rate.code!].Name : string.Empty;
                    }
                }
            }
        }

        public async Task<NbpTable?> GetCurrentCurrencyRate(string currency)
        {
            RestClient client = new RestClient("http://api.nbp.pl/api");
            RestRequest restRequest = new RestRequest($"exchangerates/rates/a/{currency}/");

            var response = await client.ExecuteGetAsync<NbpTable>(restRequest);

            if (response != null && response.Data != null && response.Data.rates.Count > 0)
            {
                await MidRatesConverter(response.Data);

                return response.Data;
            }
            else
            {
                return null;
            }
        }

        public async Task<NbpTable?> GetAllRates()
        {
            RestClient client = new RestClient("http://api.nbp.pl/api");
            RestRequest restRequest = new RestRequest("exchangerates/tables/a");

            var response = await client.ExecuteGetAsync<List<NbpTable>>(restRequest);

            if (response != null && response.Data != null && response.Data.Count > 0 && response.Data[0].rates.Count > 0)
            {
                await MidRatesConverter(response.Data[0]);

                return response.Data[0];
            }
            else
            {
                return null;
            }
        }

        public async Task<List<NbpTable>?> GetAllRatesWithDates(DateOnly startDate, DateOnly endDate)
        {
            if (IsDateValid(startDate) == false || IsDateValid(endDate) == false)
            {
                return null;
            }

            RestClient client = new RestClient("http://api.nbp.pl/api");
            RestRequest restRequest = new RestRequest($"exchangerates/tables/a/{startDate.ToString("yyyy-MM-dd")}/{endDate.ToString("yyyy-MM-dd")}/");

            var response = await client.ExecuteGetAsync<List<NbpTable>>(restRequest);

            if (response != null && response.Data != null && response.Data.Count > 0)
            {
                //await MidRatesConverter(response.Data);

                return response.Data;
            }
            else
            {
                return null;
            }
        }

        public bool IsDateValid(DateOnly dateTime)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var dateToVerify = dateTime;

            if (dateToVerify <= today)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}