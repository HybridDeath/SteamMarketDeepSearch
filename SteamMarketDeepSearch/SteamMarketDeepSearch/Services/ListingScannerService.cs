using SteamMarketDeepSearch.Constants;
using SteamMarketDeepSearch.Infrastructure;
using SteamMarketDeepSearch.Models;
using SteamMarketDeepSearch.Parsers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SteamMarketDeepSearch.Services
{
    public class ListingScannerService(
        SkinsDatabaseService database,
        SteamMarketClient client)
    {
        private readonly SkinsDatabaseService _database =
            database;

        private readonly SteamMarketClient _client =
            client;

        public async Task<List<MarketListingData>> ScanAllBucketsAsync()
        {
            List<MarketListingData> result = [];

            List<SkinDefinition> buckets =
                await _database.GetAllBucketsDescAsync();

            foreach (SkinDefinition bucket in buckets)
            {
                List<MarketListingData> listings =
                    await ScanBucketAsync(bucket);

                result.AddRange(listings);

                await Task.Delay(
                    GlobalThrottling.GetMarketScanDelay());
            }

            return result;
        }

        private async Task<List<MarketListingData>> ScanBucketAsync(
            SkinDefinition skin)
        {
            string url =
                $"{SteamMarketConstants.MarketBaseUrl}/730/" +
                $"{skin.MarketBucketId}" +
                $"?appid={SteamMarketConstants.AppId}";


            string html =
                await _client.GetMarketPageAsync(url);


            if (!html.Contains("\\\\\\\"listingid\\\\\\\""))
            {
                return [];
            }


            return SteamListingParser.Parse(
                html,
                skin.MarketBucketId);
        }
    }
}