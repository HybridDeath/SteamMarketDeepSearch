using SteamMarketDeepSearch.Clients.Advanced;
using SteamMarketDeepSearch.Constants;
using SteamMarketDeepSearch.Filters;
using SteamMarketDeepSearch.Models.Advanced;
using SteamMarketDeepSearch.Parsers.Advanced;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SteamMarketDeepSearch.Services.Advanced
{
    public class ListingAdvancedScanningService(SteamMarketAdvancedClient client)
    {
        private readonly SteamMarketAdvancedClient _client = client;

        private async Task<AdvancedListingPage> ScanPageAsync(string bucketId, int start)
        {
            string assetProperty =
                AssetPropertyBuilder.BuildSingleAssetProperty().First();

            string url =
                $"{SteamMarketConstants.MarketBaseUrl}/" +
                $"{SteamMarketConstants.AppId}/" +
                $"{bucketId}" +
                $"?appid={SteamMarketConstants.AppId}" +
                $"&sort=0" +
                $"&dir=0" +
                $"&{assetProperty}";

            string json =
            """
            [
                {
                    "appid":730,
                    "strItemName":"BUCKET",
                    "sort":
                    {
                        "field":0,
                        "direction":0
                    },
                    "filters":{},
                    "accessoryFilters":{},
                    "propertyFilters":
                    {
                        "1":
                        {
                            "property_id":1,
                            "int_max":"727",
                            "int_min":"727"
                        }
                    },
                    "start":START
                }
            ]
            """;

            json =
                json.Replace("BUCKET", bucketId)
                    .Replace("START", start.ToString());

            HttpResponseMessage response =
                await _client.SendJsonAsync(url, json);

            response.EnsureSuccessStatusCode();

            string responseJson =
                await response.Content.ReadAsStringAsync();

            return SteamListingAdvancedParser.Parse(responseJson);
        }

        public async Task<AdvancedBucketScanResult> ScanBucketAsync(string bucketId)
        {
            AdvancedBucketScanResult result =
                new()
                {
                    BucketId = bucketId
                };

            int start = 0;

            bool more;

            do
            {
                AdvancedListingPage page =
                    await ScanPageAsync(
                        bucketId,
                        start);

                result.TotalListingsFound +=
                    page.Listings.Count;


                foreach (AdvancedListingData listing in page.Listings)
                {
                    result.PaintSeedMatches++;


                    if (WearChecker.Matches(listing.WearText))
                    {
                        result.WearMatches++;

                        result.Listings.Add(
                            listing);
                    }
                }


                more = page.More;

                start += 20;

            }
            while (more);


            return result;
        }
    }
}