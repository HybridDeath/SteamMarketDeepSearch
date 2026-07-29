using SteamMarketDeepSearch.Constants;
using SteamMarketDeepSearch.Infrastructure;
using SteamMarketDeepSearch.Models;
using SteamMarketDeepSearch.Parsers;
using System;
using System.Collections.Generic;
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


        public async Task<SkinIndexResult> IndexSkinsAsync()
        {
            List<WeaponCatalogEntry> catalog =
                await _catalogService.LoadAsync();


            List<SkinDefinition> indexedSkins = [];

            HashSet<string> indexedSkinNames = [];

            foreach (WeaponCatalogEntry weapon in catalog)
            {
                if (string.IsNullOrWhiteSpace(
                    weapon.MarketQuery))
                {
                    continue;
                }


                string url =
                    $"{SteamMarketConstants.MarketSearchUrl}" +
                    $"?category_Weapon={weapon.MarketQuery}" +
                    $"&appid={SteamMarketConstants.AppId}";


                List<MarketSkinData> skins =
                    await DownloadAllPagesAsync(url);


                foreach (MarketSkinData skin in skins)
                {
                    if (string.IsNullOrWhiteSpace(skin.MarketHashName))
                    {
                        continue;
                    }


                    if (string.IsNullOrWhiteSpace(skin.MarketBucketId))
                    {
                        continue;
                    }


                    string normalizedName = NormalizeSkinName(skin.MarketHashName);


                    if (indexedSkinNames.Contains(normalizedName))
                    {
                        continue;
                    }


                    DateTime now =
                        DateTime.UtcNow;


                    indexedSkins.Add(
                        new SkinDefinition
                        {
                            WeaponType =
                                weapon.WeaponType,


                            MarketHashName =
                                normalizedName,


                            MarketBucketId =
                                skin.MarketBucketId,


                            SellOrderCount =
                                skin.SellOrderCount,


                            CreatedAt =
                                now,


                            LastUpdatedAt =
                                now
                        });


                    indexedSkinNames.Add(normalizedName);
                }
                await Task.Delay(GlobalThrottling.GetMarketIndexDelay());
            }


            return new SkinIndexResult
            {
                Skins = indexedSkins
            };
        }


        private static string NormalizeSkinName(
            string name)
        {
            name =
                name.Replace(
                    "StatTrak™ ",
                    "",
                    StringComparison.Ordinal);


            name =
                name.Replace(
                    "Souvenir ",
                    "",
                    StringComparison.Ordinal);


            string[] conditions =
            [
                " (Factory New)",
                " (Minimal Wear)",
                " (Field-Tested)",
                " (Well-Worn)",
                " (Battle-Scarred)"
            ];


            foreach (string condition in conditions)
            {
                if (name.EndsWith(
                    condition,
                    StringComparison.Ordinal))
                {
                    name =
                        name[..^condition.Length];

                    break;
                }
            }


            return name.Trim();
        }


        private async Task<List<MarketSkinData>> DownloadAllPagesAsync(
            string url)
        {
            List<MarketSkinData> allSkins = [];


            int start = 0;


            const int pageSize = 100;


            while (true)
            {
                string pageUrl =
                    $"{url}&start={start}&count={pageSize}";


                string html =
                    await _client.GetMarketPageAsync(
                        pageUrl);


                List<MarketSkinData> page =
                    SteamMarketParser.ParseListings(
                        html);


                if (page.Count == 0)
                {
                    break;
                }


                allSkins.AddRange(
                    page);


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