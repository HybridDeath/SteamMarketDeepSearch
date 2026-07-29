using SteamMarketDeepSearch.Enums;
using System;

namespace SteamMarketDeepSearch.Models
{
    public class SkinDefinition
    {
        public long Id { get; set; }

        public string MarketHashName { get; set; } = string.Empty;

        public WeaponType WeaponType { get; set; }

        public string MarketListingId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
