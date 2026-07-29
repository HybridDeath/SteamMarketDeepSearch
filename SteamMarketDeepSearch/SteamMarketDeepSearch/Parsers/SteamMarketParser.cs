using SteamMarketDeepSearch.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SteamMarketDeepSearch.Parsers
{
    public static partial class SteamMarketParser
    {
        private static readonly Regex HashNameRegex =
            GenerateHashNameRegex();


        private static readonly Regex SellOrderRegex =
            GenerateSellOrderRegex();


        private static readonly Regex BucketIdRegex =
            GenerateBucketIdRegex();


        public static List<MarketSkinData> ParseListings(string html)
        {
            List<MarketSkinData> results = [];


            foreach (Match match in HashNameRegex.Matches(html))
            {
                int start =
                    html.LastIndexOf(
                        @"\""strHash\"":",
                        match.Index);


                if (start < 0)
                {
                    start = match.Index;
                }


                int end =
                    html.IndexOf(
                        @"}},\""app\"":",
                        match.Index);


                if (end < 0)
                {
                    end =
                        html.Length;
                }


                string fragment =
                    html[start..end];


                Match sellMatch =
                    SellOrderRegex.Match(fragment);


                Match bucketMatch =
                    BucketIdRegex.Match(fragment);


                results.Add(
                    new MarketSkinData
                    {
                        MarketHashName =
                            match.Groups[1].Value,


                        SellOrderCount =
                            sellMatch.Success
                            ?
                            int.Parse(
                                sellMatch.Groups[1].Value)
                            :
                            0,


                        MarketBucketId =
                            bucketMatch.Success
                            ?
                            bucketMatch.Groups[1].Value
                            :
                            string.Empty
                    });
            }


            return results;
        }

        [GeneratedRegex(
            @"market_hash_name\\\\\\?""?\\\\?""?[:\\]+\\\\?""?([^""\\]+)",
            RegexOptions.Compiled)]
        private static partial Regex GenerateHashNameRegex();


        [GeneratedRegex(
            @"cSellOrders\\+""?:(\d+)",
            RegexOptions.Compiled)]
        private static partial Regex GenerateSellOrderRegex();


        [GeneratedRegex(
            @"market_bucket_group_id\\+.*?([A-Z0-9]{10,})",
            RegexOptions.Compiled)]
        private static partial Regex GenerateBucketIdRegex();
    }
}