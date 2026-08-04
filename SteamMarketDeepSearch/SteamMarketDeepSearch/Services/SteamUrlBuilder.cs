using SteamMarketDeepSearch.Constants;

namespace SteamMarketDeepSearch.Services
{
    public static class SteamUrlBuilder
    {
        public static string BuildListingUrl(string listingId)
        {
            return $"{SteamMarketConstants.MarketBaseUrl}/{SteamMarketConstants.AppId}/{listingId}";
        }
    }
}
