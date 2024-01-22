using currency_exchange_api_core.Models;
using RestSharp;

namespace currency_exchange_api_core.Services
{
    public class CountriesService
    {
        public async Task<string> GetFlagUrl(string code)
        {
            RestClient client = new RestClient("https://restcountries.com/v3.1");
            RestRequest restRequest = new RestRequest($"currency/{code}");

            restRequest.AddQueryParameter("fields", "flags");

            var response = await client.ExecuteGetAsync<List<Country>>(restRequest);

            if (response != null && response.Data != null)
            {
                if (response.Data.Count == 1)
                {
                    return response.Data[0].Flags.Png;
                }
            }

            return "https://media.wired.co.uk/photos/606da9af687a704c2c361d4b/1:1/w_1280,h_1280,c_limit/Flag_20x30.jpg";
        }

        public async Task<List<Country>?> GetAllCountriesWithFlagsAndCurrencies()
        {
            RestClient client = new RestClient("https://restcountries.com/v3.1");
            RestRequest restRequest = new RestRequest("all");

            restRequest.AddQueryParameter("fields", "flags,currencies");

            var response = await client.ExecuteGetAsync<List<Country>>(restRequest);

            if (response != null && response.Data != null)
            {
                return response.Data;
            }

            return null;
        }
    }
}