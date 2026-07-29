using SteamMarketDeepSearch.Enums;

namespace SteamMarketDeepSearch.Models
{
    public class WeaponCatalogEntry
    {
        public string DisplayName { get; set; } = string.Empty;

        public ItemCategory Category { get; set; }

        public WeaponType WeaponType { get; set; }

        public string MarketQuery { get; set; } = string.Empty;
    }
}