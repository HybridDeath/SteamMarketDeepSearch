using System.Collections.Generic;

namespace SteamMarketDeepSearch.Models.Advanced
{
    public class AdvancedListingPage
    {
        public bool More { get; set; }
        public int Start { get; set; }
        public int TotalCount { get; set; }
        public List<AdvancedListingData> Listings { get; set; } = [];
    }
}