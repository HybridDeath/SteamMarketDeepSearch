namespace SteamMarketDeepSearch.Models
{
    public class MarketListingData
    {
        public required string ListingId { get; set; }

        public required string InspectLink { get; set; }

        public int PaintSeed { get; set; }

        public double WearValue { get; set; }

        public int Price { get; set; }
    }
}