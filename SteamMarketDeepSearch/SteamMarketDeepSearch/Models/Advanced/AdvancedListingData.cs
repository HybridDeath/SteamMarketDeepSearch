namespace SteamMarketDeepSearch.Models.Advanced
{
    public class AdvancedListingData
    {
        public string ListingId { get; set; } = string.Empty;
        public string AssetId { get; set; } = string.Empty;
        public int PaintSeed { get; set; }
        public float WearValue { get; set; }
        public string PaintToken { get; set; } = string.Empty;
    }
}