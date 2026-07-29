using SteamMarketDeepSearch.Constants;
using SteamMarketDeepSearch.Infrastructure;
using SteamMarketDeepSearch.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SteamMarketDeepSearch.Services
{
    public class ListingScannerService(DatabaseService database, SteamMarketClient client)
    {
        private readonly DatabaseService _database = database;

        private readonly SteamMarketClient _client = client;

        public async Task ScanAsync()
        {
            List<SkinDefinition> skins =
                await _database.GetAllSkinsAsync();


            Debug.WriteLine(
                $"L2 START. Buckets (skins): {skins.Count}");


            foreach (SkinDefinition skin in skins)
            {
                if (string.IsNullOrWhiteSpace(
                    skin.MarketBucketId))
                {
                    continue;
                }


                await ScanBucketAsync(
                    skin);
            }


            Debug.WriteLine(
                "L2 FINISHED");
        }


        private async Task ScanBucketAsync(SkinDefinition skin)
        {
            Debug.WriteLine(
                $"Scanning bucket: {skin.MarketBucketId}");


            foreach (string filter in AssetPropertyBuilder.BuildAssetProperties())
            {
                string url =
                    $"{SteamMarketConstants.MarketBaseUrl}/730/" +
                    $"{skin.MarketBucketId}" +
                    $"?appid={SteamMarketConstants.AppId}" +
                    $"&{filter}";


                Debug.WriteLine(
                    url);


                string html =
                    await _client.GetMarketPageAsync(
                        url);


                /*
                 * TODO:
                 *
                 * ListingParser.Parse(html)
                 *
                 * ListingDatabaseService.Save(...)
                 */


                await Task.Delay(GlobalThrottling.GetMarketScanDelay());
            }
        }
    }
}