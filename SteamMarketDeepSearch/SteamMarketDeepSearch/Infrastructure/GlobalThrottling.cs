namespace SteamMarketDeepSearch.Infrastructure
{
    public static class GlobalThrottling
    {
        public static readonly int MarketIndexDelayMs = 15000;

        public static int GetMarketIndexDelay()
        {
            return MarketIndexDelayMs;
        }
    }
}