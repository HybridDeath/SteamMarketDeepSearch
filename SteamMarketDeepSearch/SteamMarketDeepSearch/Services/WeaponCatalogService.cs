using SteamMarketDeepSearch.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SteamMarketDeepSearch.Services
{
    public class WeaponCatalogService
    {
        private readonly string _catalogPath =
            @"C:\Users\Brydżu\source\repos\SteamMarketDeepSearch\SteamMarketDeepSearch\SteamMarketDeepSearch\Data\WeaponCatalog.json";

        private readonly JsonSerializerOptions opt = new()
        {
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

        public async Task<List<WeaponCatalogEntry>> LoadAsync()
        {
            if (!File.Exists(_catalogPath))
            {
                throw new FileNotFoundException(
                    "WeaponCatalog.json not found.",
                    _catalogPath);
            }

            string json =
                await File.ReadAllTextAsync(_catalogPath);

            try
            {
                List<WeaponCatalogEntry>? catalog =
                    JsonSerializer.Deserialize<List<WeaponCatalogEntry>>(
                        json,
                        opt);

                return catalog ?? [];
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}