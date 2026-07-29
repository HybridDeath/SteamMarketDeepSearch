using SteamMarketDeepSearch.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamMarketDeepSearch.Services
{
    public static class SteamUrlBuilder
    {
        public static string BuildListingUrl(string listingId)
        {
            return $"{SteamConstants.MarketBaseUrl}/{SteamConstants.AppId}/{listingId}";
        }
    }
}
