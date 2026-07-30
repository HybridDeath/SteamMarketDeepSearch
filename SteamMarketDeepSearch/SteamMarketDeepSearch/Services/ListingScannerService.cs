using SteamMarketDeepSearch.Constants;
using SteamMarketDeepSearch.Infrastructure;
using SteamMarketDeepSearch.Models;
using SteamMarketDeepSearch.Parsers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SteamMarketDeepSearch.Services
{
    public class ListingScannerService(SkinsDatabaseService database, SteamMarketClient client)
    {
        private readonly SkinsDatabaseService _database = database;

        private readonly SteamMarketClient _client = client;

        public async Task<(List<MarketListingData> Listings, string Html)> ScanLargestBucketAsync()
        {
            SkinDefinition? skin =
                await _database.GetLargestSkinBucketAsync();


            if (skin == null)
            {
                Debug.WriteLine(
                    "No skins found in database.");

                return ([], "");
            }


            Debug.WriteLine(
                $"L2 TEST BUCKET:");

            Debug.WriteLine(
                $"{skin.MarketHashName} | " +
                $"Offers: {skin.SellOrderCount}");

            Debug.WriteLine("L2 FINISHED");

            return await ScanBucketAsync(skin);
        }


        private async Task<(List<MarketListingData> Listings, string Html)> ScanBucketAsync(SkinDefinition skin)
        {
            Debug.WriteLine(
                $"Scanning bucket: {skin.MarketHashName}");


            string url =
                $"{SteamMarketConstants.MarketBaseUrl}/730/" +
                $"{skin.MarketBucketId}" +
                $"?appid={SteamMarketConstants.AppId}";


            Debug.WriteLine(url);


            string html =
                await _client.GetMarketPageAsync(url);


            if (!html.Contains("\\\\\\\"listingid\\\\\\\""))
            {
                Debug.WriteLine(
                    "HTML does not contain listings. Retrying...");


                await Task.Delay(2000);


                html =
                    await _client.GetMarketPageAsync(url);
            }


            List<MarketListingData> listings =
                SteamListingParser.Parse(
                    html,
                    skin.MarketBucketId);


            Debug.WriteLine(
                $"Parsed listings: {listings.Count}");


            foreach (MarketListingData listing in listings.Take(5))
            {
                Debug.WriteLine(
                    $"ID: {listing.ListingId} | " +
                    $"Paint: {listing.PaintSeed} | " +
                    $"Float: {listing.WearValue} | " +
                    $"Price: {listing.Price}");

                Debug.WriteLine(
                    listing.InspectLink);
            }


            return (listings, html);
        }
    }
}