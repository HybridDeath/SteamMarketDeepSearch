using SteamMarketDeepSearch.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace SteamMarketDeepSearch.Parsers
{
    public static class SteamListingParser
    {
        public static List<MarketListingData> Parse(
            string html,
            string marketBucketId)
        {
            List<MarketListingData> result = [];


            const string listingsMarker =
                "\\\\\\\"listings\\\\\\\":[";


            int listingsIndex =
                html.IndexOf(
                    listingsMarker,
                    StringComparison.Ordinal);


            if (listingsIndex < 0)
            {
                return result;
            }


            string section =
                html[(listingsIndex + listingsMarker.Length)..];


            const string listingMarker =
                "\\\\\\\"listingid\\\\\\\":\\\\\\\"";


            string[] entries =
                section.Split(
                    listingMarker,
                    StringSplitOptions.RemoveEmptyEntries);


            foreach (string entry in entries)
            {
                string listingId =
                    ReadUntil(
                        entry,
                        "\\\\\\\"");


                if (string.IsNullOrWhiteSpace(listingId))
                {
                    continue;
                }


                MarketListingData? listing =
                    ParseSingle(
                        entry,
                        listingId,
                        marketBucketId);


                if (listing != null)
                {
                    result.Add(listing);
                }
            }


            return result;
        }



        private static MarketListingData? ParseSingle(
            string data,
            string listingId,
            string marketBucketId)
        {
            int assetIndex =
                data.IndexOf(
                    "\\\\\\\"asset\\\\\\\":",
                    StringComparison.Ordinal);

            foreach (string marker in new[]
            {
                "seed"
            })
            {
                int index =
                    data.IndexOf(
                        marker,
                        StringComparison.OrdinalIgnoreCase);


                Debug.WriteLine(
                    $"{marker}: {index}");


                if (index >= 0)
                {
                    Debug.WriteLine(
                        data.Substring(
                            Math.Max(0, index - 200),
                            400));
                }
            }


            if (assetIndex < 0)
            {
                return null;
            }


            string asset =
                data[assetIndex..];


            int paint =
                ReadPropertyInt(
                    asset,
                    1);


            double wear =
                ReadDouble(
                    asset,
                    "\\\\\\\"float_value\\\\\\\":");


            int price =
                ReadInt(
                    data,
                    "\\\\\\\"unPrice\\\\\\\":");


            return new MarketListingData
            {
                ListingId = listingId,

                PaintSeed = paint,

                WearValue = wear,

                Price = price,

                InspectLink =
                    $"https://steamcommunity.com/market/listings/730/{marketBucketId}?detail={listingId}"
            };
        }

        private static int ReadPropertyInt(
            string data,
            int propertyId)
        {
            string marker =
                $"\\\\\\\"propertyid\\\\\\\":{propertyId}";


            int index =
                data.IndexOf(
                    marker,
                    StringComparison.Ordinal);


            if (index < 0)
            {
                return -1;
            }


            int value =
                data.IndexOf(
                    "\\\\\\\"int_value\\\\\\\":\\\\\\\"",
                    index,
                    StringComparison.Ordinal);


            if (value < 0)
            {
                return -1;
            }


            value +=
                "\\\\\\\"int_value\\\\\\\":\\\\\\\"".Length;


            int end =
                data.IndexOf(
                    "\\\\\\\"",
                    value,
                    StringComparison.Ordinal);


            if (end < 0)
            {
                return -1;
            }


            return int.Parse(
                data[value..end]);
        }

        private static int ReadInt(
            string data,
            string marker)
        {
            int index =
                data.IndexOf(
                    marker,
                    StringComparison.Ordinal);


            if (index < 0)
            {
                return -1;
            }


            index += marker.Length;


            int end =
                index;


            while (
                end < data.Length &&
                char.IsDigit(data[end]))
            {
                end++;
            }


            return int.Parse(
                data[index..end]);
        }



        private static double ReadDouble(
            string data,
            string marker)
        {
            int index =
                data.IndexOf(
                    marker,
                    StringComparison.Ordinal);


            if (index < 0)
            {
                return -1;
            }


            index += marker.Length;


            int end =
                index;


            while (
                end < data.Length &&
                (
                    char.IsDigit(data[end]) ||
                    data[end] == '.'
                ))
            {
                end++;
            }


            return double.Parse(
                data[index..end],
                CultureInfo.InvariantCulture);
        }



        private static string ReadUntil(
            string data,
            string separator)
        {
            int index =
                data.IndexOf(
                    separator,
                    StringComparison.Ordinal);


            return index < 0
                ? data
                : data[..index];
        }
    }
}