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


                string baseUrl =
                    $"{SteamConstants.MarketSearchUrl}" +
                    $"?category_Weapon={weapon.MarketQuery}" +
                    $"&appid={SteamConstants.AppId}";


                Debug.WriteLine(baseUrl);


                List<MarketSkinData> skins =
                    await DownloadAllPagesAsync(baseUrl);


                Debug.WriteLine(
                    $"Found total {skins.Count} skins");
            }


            Debug.WriteLine(
                "Indexing finished.");
        }



        private async Task<List<MarketSkinData>> DownloadAllPagesAsync(
            string url)
        {
            List<MarketSkinData> allSkins = [];


            int start = 0;

            const int pageSize = 100;


            while (true)
            {
                string pagedUrl =
                    $"{url}&start={start}&count={pageSize}";


                Debug.WriteLine(
                    $"Downloading page: start={start}");


                string html =
                    await _client.GetMarketPageAsync(pagedUrl);


                List<MarketSkinData> page =
                    SteamMarketParser.ParseListings(html);


                Debug.WriteLine(
                    $"Page results: {page.Count}");


                if (page.Count == 0)
                {
                    break;
                }


                allSkins.AddRange(page);


                if (page.Count < pageSize)
                {
                    break;
                }


                start += pageSize;
            }


            return allSkins;
        }
    }
}