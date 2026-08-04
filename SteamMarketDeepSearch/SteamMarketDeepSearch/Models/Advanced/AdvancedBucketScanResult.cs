using System.Collections.Generic;

namespace SteamMarketDeepSearch.Models.Advanced
{
    public class AdvancedBucketScanResult
    {
        public string BucketId { get; set; } = string.Empty;
        public int TotalListingsFound { get; set; }
        public int PaintSeedMatches { get; set; }
        public int WearMatches { get; set; }
        public List<AdvancedListingData> Listings { get; set; } = [];
    }
}