using SteamMarketDeepSearch.Constants;
using SteamMarketDeepSearch.Models;
using SteamMarketDeepSearch.Parsers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SteamMarketDeepSearch.Services
{
    public class SkinIndexerService(
        SteamMarketClient client,
        WeaponCatalogService catalogService)
    {
        private readonly SteamMarketClient _client = client;

        private readonly WeaponCatalogService _catalogService =
            catalogService;


        public async Task IndexSkinsAsync()
        {
            List<WeaponCatalogEntry> catalog =
                await _catalogService.LoadAsync();


            foreach (WeaponCatalogEntry weapon in catalog)
            {
                if (string.IsNullOrWhiteSpace(weapon.MarketQuery))
                {
                    Debug.WriteLine(
                        $"Skipping {weapon.DisplayName}");

                    continue;
                }


                Debug.WriteLine(
                    $"Processing {weapon.DisplayName}");


                string url =
                    $"{SteamConstants.MarketSearchUrl}" +
                    $"?category_Weapon={weapon.MarketQuery}" +
                    $"&appid={SteamConstants.AppId}";


                Debug.WriteLine(url);


                string html =
                    await _client.GetMarketPageAsync(url);


                List<MarketSkinData> skins =
                    SteamMarketParser.ParseListings(html);


                Debug.WriteLine(
                    $"Found {skins.Count} skins");
            }


            Debug.WriteLine(
                "Indexing finished.");
        }
    }
}