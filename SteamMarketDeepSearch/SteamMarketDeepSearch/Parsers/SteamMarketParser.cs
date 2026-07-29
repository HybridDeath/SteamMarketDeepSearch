using SteamMarketDeepSearch.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;

namespace SteamMarketDeepSearch.Parsers
{
    public static class SteamMarketParser
    {
        public static List<MarketSkinData> ParseListings(string html)
        {
            List<MarketSkinData> results = [];


            int strHashIndex =
                html.IndexOf(
                    "strHash",
                    StringComparison.Ordinal);


            if (strHashIndex < 0)
            {
                Debug.WriteLine(
                    "STRHASH NOT FOUND");

                return results;
            }


            int resultsStart =
                html.LastIndexOf(
                    "results",
                    strHashIndex,
                    StringComparison.Ordinal);


            if (resultsStart < 0)
            {
                Debug.WriteLine(
                    "RESULTS NOT FOUND BEFORE STRHASH");

                return results;
            }


            int arrayStart =
                html.IndexOf(
                    '[',
                    resultsStart);


            if (arrayStart < 0)
            {
                Debug.WriteLine(
                    "ARRAY START NOT FOUND");

                return results;
            }


            int arrayEnd =
                FindJsonArrayEnd(
                    html,
                    arrayStart);


            if (arrayEnd < 0)
            {
                Debug.WriteLine(
                    "ARRAY END NOT FOUND");

                return results;
            }


            string json =
                html.Substring(
                    arrayStart,
                    arrayEnd - arrayStart + 1);


            while (json.Contains("\\\""))
            {
                json =
                    json.Replace(
                        "\\\"",
                        "\"",
                        StringComparison.Ordinal);
            }


            Debug.WriteLine(
                "===== CLEAN JSON START =====");

            Debug.WriteLine(
                json.StartsWith("[{\"strHash\""));

            Debug.WriteLine(
                json[..Math.Min(300, json.Length)]);

            Debug.WriteLine(
                "===== CLEAN JSON END =====");

            try
            {
                using JsonDocument document =
                    JsonDocument.Parse(json);


                foreach (JsonElement item in document.RootElement.EnumerateArray())
                {
                    if (!item.TryGetProperty(
                        "asset_description",
                        out JsonElement asset))
                    {
                        continue;
                    }


                    if (!asset.TryGetProperty(
                        "market_hash_name",
                        out JsonElement hashName))
                    {
                        continue;
                    }


                    if (!asset.TryGetProperty(
                        "market_bucket_group_id",
                        out JsonElement bucketId))
                    {
                        continue;
                    }


                    if (!item.TryGetProperty(
                        "cSellOrders",
                        out JsonElement sellOrders))
                    {
                        continue;
                    }


                    results.Add(
                        new MarketSkinData
                        {
                            MarketHashName =
                                hashName.GetString()
                                ?? string.Empty,


                            SellOrderCount =
                                sellOrders.GetInt32(),


                            MarketBucketId =
                                bucketId.GetString()
                                ?? string.Empty
                        });
                }
            }
            catch (JsonException ex)
            {
                Debug.WriteLine(
                    "JSON PARSE ERROR:");

                Debug.WriteLine(
                    ex.Message);

                Debug.WriteLine(
                    json[..Math.Min(500, json.Length)]);
            }


            return results;
        }



        private static int FindJsonArrayEnd(
            string text,
            int start)
        {
            int depth = 0;

            bool inString = false;

            bool escaped = false;


            for (int i = start; i < text.Length; i++)
            {
                char c = text[i];


                if (escaped)
                {
                    escaped = false;
                    continue;
                }


                if (c == '\\')
                {
                    escaped = true;
                    continue;
                }


                if (c == '"')
                {
                    inString = !inString;
                    continue;
                }


                if (inString)
                {
                    continue;
                }


                if (c == '[')
                {
                    depth++;
                }
                else if (c == ']')
                {
                    depth--;

                    if (depth == 0)
                    {
                        return i;
                    }
                }
            }


            return -1;
        }
    }
}