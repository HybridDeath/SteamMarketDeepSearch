using SteamMarketDeepSearch.Enums;
using System;

namespace SteamMarketDeepSearch.Models
{
    public class SkinDefinition
    {
        public long Id { get; set; }
        public WeaponType WeaponType { get; set; }
        public string MarketHashName { get; set; } = string.Empty;
        public string MarketBucketId { get; set; } = string.Empty;
        public int SellOrderCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
