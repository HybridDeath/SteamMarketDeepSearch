using SteamMarketDeepSearch.Constants;
using SteamMarketDeepSearch.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SteamMarketDeepSearch.Parsers
{
    public partial class SteamMarketParser
    {
        private static readonly Regex ListingRegex =
            new($@"{SteamConstants.MarketBaseUrl}/{SteamConstants.AppId}/([^?""\\]+)", RegexOptions.Compiled);


        public static List<MarketSkinData> ParseListings(string html)
        {
            List<MarketSkinData> results = [];


            foreach (Match match in ListingRegex.Matches(html))
            {
                string listingId = match.Groups[1].Value;


                results.Add(
                    new MarketSkinData
                    {
                        MarketListingId = listingId,
                        MarketHashName = string.Empty
                    });
            }


            return results;
        }
    }
}