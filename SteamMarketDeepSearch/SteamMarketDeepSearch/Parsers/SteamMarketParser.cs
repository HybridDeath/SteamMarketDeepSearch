using SteamMarketDeepSearch.Constants;
using SteamMarketDeepSearch.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SteamMarketDeepSearch.Parsers
{
    public static partial class SteamMarketParser
    {
        private static readonly Regex listingRegex =
            new(
                $@"{SteamConstants.MarketBaseUrl}/{SteamConstants.AppId}/([^?""\\]+)",
                RegexOptions.Compiled
            );


        private static readonly Regex skinNameRegex =
            SkinNameRegex();


        public static List<MarketSkinData> ParseListings(string html)
        {
            List<MarketSkinData> results = [];


            MatchCollection listings =
                listingRegex.Matches(html);


            foreach (Match listingMatch in listings)
            {
                string listingId =
                    listingMatch.Groups[1].Value;


                string remainingHtml =
                    html[listingMatch.Index..];


                Match nameMatch =
                    skinNameRegex.Match(remainingHtml);


                string skinName =
                    nameMatch.Success
                        ? nameMatch.Groups[1].Value.Trim()
                        : string.Empty;


                results.Add(
                    new MarketSkinData
                    {
                        MarketListingId = listingId,
                        MarketHashName = skinName
                    });
            }


            return results;
        }

        [GeneratedRegex(@">([^<>]+ \| [^<>]+)<", RegexOptions.Compiled)]
        private static partial Regex SkinNameRegex();
    }
}