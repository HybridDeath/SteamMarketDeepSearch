using SteamMarketDeepSearch.Constants;
using SteamMarketDeepSearch.Infrastructure;
using SteamMarketDeepSearch.Models;
using SteamMarketDeepSearch.Parsers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public async Task ScanAllBucketsAsync(
            Action<List<MarketListingData>> onListingsFound)
        {
            List<SkinDefinition> buckets =
                await _database.GetAllBucketsDescAsync();


            foreach (SkinDefinition bucket in buckets)
            {
                List<MarketListingData> listings =
                    await ScanBucketAsync(bucket);


                if (listings.Count > 0)
                {
                    onListingsFound(listings);
                }


                await Task.Delay(
                    GlobalThrottling.GetMarketScanDelay());
            }
        }

        private async Task<List<MarketListingData>> ScanBucketAsync(SkinDefinition skin)
        {
            List<MarketListingData> result = [];


            foreach (string assetProperty in AssetPropertyBuilder.BuildAssetProperties())
            {
                string url =
                    $"{SteamMarketConstants.MarketBaseUrl}/730/" +
                    $"{skin.MarketBucketId}" +
                    $"?appid={SteamMarketConstants.AppId}&" +
                    assetProperty;

                string html =
                    await _client.GetMarketPageAsync(url);

                if (!html.Contains("\\\\\\\"listingid\\\\\\\""))
                {
                    await Task.Delay(
                        GlobalThrottling.GetMarketScanDelay());

                    Debug.WriteLine($"Scanning bucket {skin.MarketBucketId} failed! HTML Length: {html.Length}");

                    continue;
                }

                List<MarketListingData> listings =
                    SteamListingParser.Parse(
                        html,
                        skin.MarketBucketId);

                result.AddRange(
                    listings);
            }

            return result;
        }
    }
}