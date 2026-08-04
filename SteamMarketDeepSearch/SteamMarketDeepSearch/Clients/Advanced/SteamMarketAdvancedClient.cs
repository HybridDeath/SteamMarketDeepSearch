using SteamMarketDeepSearch.Constants;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SteamMarketDeepSearch.Clients.Advanced
{
    public class SteamMarketAdvancedClient
    {
        private readonly HttpClient _httpClient;

        public SteamMarketAdvancedClient()
        {
            _httpClient = new HttpClient();

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0");

            _httpClient.DefaultRequestHeaders.Accept.ParseAdd(
                "*/*");
        }

        public async Task<HttpResponseMessage> SendJsonAsync(
            string url,
            string json)
        {
            HttpRequestMessage request =
                new(HttpMethod.Post, url)
                {
                    Content =
                    new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json")
                };

            request.Headers.TryAddWithoutValidation(
                "x-valve-request-type",
                "routeAction");

            request.Headers.TryAddWithoutValidation(
                "x-valve-action-type",
                "8cnlIgBs66uLKD68o1fzpLlHFSHv52c4l9_ohRpMLzk:Search");

            request.Headers.Referrer =
                new Uri(url);

            return await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead);
        }
    }
}