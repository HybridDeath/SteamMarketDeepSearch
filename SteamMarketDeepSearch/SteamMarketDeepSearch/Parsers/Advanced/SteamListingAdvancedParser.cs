using System.Text.Json;
using SteamMarketDeepSearch.Models.Advanced;

namespace SteamMarketDeepSearch.Parsers.Advanced
{
    public class SteamListingAdvancedParser
    {
        public static AdvancedListingPage Parse(string json)
        {
            using JsonDocument document =
                JsonDocument.Parse(json);


            JsonElement root =
                document.RootElement;


            AdvancedListingPage result =
                new()
                {
                    More =
                        root.GetProperty("more").GetBoolean(),

                    Start =
                        root.GetProperty("start").GetInt32(),

                    TotalCount =
                        root.GetProperty("total_count").GetInt32()
                };


            foreach (JsonElement listing in root.GetProperty("listings").EnumerateArray())
            {
                AdvancedListingData data =
                    ParseListing(listing);


                result.Listings.Add(data);
            }


            return result;
        }


        private static AdvancedListingData ParseListing(
            JsonElement listing)
        {
            AdvancedListingData result =
                new()
                {
                    ListingId =
                        listing.GetProperty("listingid").GetString() ?? string.Empty
                };


            JsonElement asset =
                listing.GetProperty("asset");


            result.AssetId =
                asset.GetProperty("assetid").GetString() ?? string.Empty;


            foreach (JsonElement property in asset.GetProperty("asset_properties").EnumerateArray())
            {
                int propertyId =
                    property.GetProperty("propertyid").GetInt32();


                switch (propertyId)
                {
                    case 1:
                        result.PaintSeed = int.Parse(property.GetProperty("int_value").GetString() ?? "0");
                        break;


                    case 2:
                        result.WearValue = property.GetProperty("float_value").GetDouble();
                        result.WearText = property.GetProperty("float_value").GetRawText();
                        break;


                    case 6:
                        result.PaintToken = property.GetProperty("string_value").GetString() ?? string.Empty;
                        break;
                }
            }


            return result;
        }
    }
}