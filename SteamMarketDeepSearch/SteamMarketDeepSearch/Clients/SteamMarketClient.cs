using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SteamMarketDeepSearch.Clients
{
    public class SteamMarketClient
    {
        private readonly HttpClient _httpClient;


        public SteamMarketClient()
        {
            _httpClient = new HttpClient();

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "SteamMarketDeepSearch/1.0");
        }


        public async Task<string> GetMarketPageAsync(string url)
        {
            using HttpResponseMessage response =
                await _httpClient.GetAsync(url);


            response.EnsureSuccessStatusCode();


            return await response.Content.ReadAsStringAsync();
        }
    }
}
