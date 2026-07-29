using SteamMarketDeepSearch.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SteamMarketDeepSearch.Parsers
{
    public static partial class SteamMarketParser
    {
        private static readonly Regex HashNameRegex =
            GenerateHashNameRegex();


        public static List<MarketSkinData> ParseListings(string html)
        {
            List<MarketSkinData> results = [];


            foreach (Match match in HashNameRegex.Matches(html))
            {
                results.Add(
                    new MarketSkinData
                    {
                        MarketHashName =
                            match.Groups[1].Value
                    });
            }


            return results;
        }


        [GeneratedRegex(
            @"market_hash_name\\\\\\?""?\\\\?""?[:\\]+\\\\?""?([^""\\]+)",
            RegexOptions.Compiled)]
        private static partial Regex GenerateHashNameRegex();
    }
}