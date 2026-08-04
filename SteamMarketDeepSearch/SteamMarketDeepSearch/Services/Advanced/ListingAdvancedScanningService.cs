using SteamMarketDeepSearch.Clients.Advanced;
using SteamMarketDeepSearch.Constants;
using SteamMarketDeepSearch.Models.Advanced;
using SteamMarketDeepSearch.Parsers.Advanced;
using System;
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

        public async Task<AdvancedListingPage> ScanBucketAsync(string bucketId)
        {
            string assetProperty = AssetPropertyBuilder.BuildSingleAssetProperty().First();

            string url =
                $"{SteamMarketConstants.MarketBaseUrl}/" +
                $"{SteamMarketConstants.AppId}/" +
                $"{bucketId}" +
                $"?appid={SteamMarketConstants.AppId}" +
                $"&sort=0" +
                $"&dir=0" +
                $"&{assetProperty}";

            Debug.WriteLine(url);

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
                    "start":0
                }
            ]
            """;

            Debug.WriteLine(json);

            json = json.Replace("BUCKET", bucketId);

            Debug.WriteLine(json);

            HttpResponseMessage response = await _client.SendJsonAsync(url, json);

            Debug.WriteLine(response);

            string responseJson = await response.Content.ReadAsStringAsync();

            Debug.WriteLine(responseJson);

            return SteamListingAdvancedParser.Parse(responseJson);
        }
    }
}