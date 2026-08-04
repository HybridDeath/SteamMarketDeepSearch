using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SteamMarketDeepSearch.Clients;
using SteamMarketDeepSearch.Clients.Advanced;
using SteamMarketDeepSearch.Constants;
using SteamMarketDeepSearch.Models;
using SteamMarketDeepSearch.Services;
using SteamMarketDeepSearch.Services.Advanced;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SteamMarketDeepSearch.Views
{
    public sealed partial class MainPage : Page
    {
        public static readonly SteamMarketClient client = new();
        public static readonly SteamMarketAdvancedClient advancedClient = new();
        public static readonly WeaponCatalogService catalog = new();
        public static readonly SkinIndexerService indexer = new(client, catalog);
        public static readonly SkinsDatabaseService database = new();
        public static readonly ListingDatabaseService listingDatabase = new();
        public static readonly ListingScannerService scanner = new(database, listingDatabase, client);
        public static readonly ListingAdvancedScanningService advancedScanner = new(advancedClient);

        public MainPage()
        {
            InitializeComponent();
        }

        private async void IndexButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("INDEX LOOP STARTED");

            await database.InitializeAsync();

            while (true)
            {
                try
                {
                    SkinIndexResult result =
                        await indexer.IndexSkinsAsync();

                    await database.UpsertSkinsAsync(
                        result.Skins);

                    Debug.WriteLine(
                        $"Indexed: {result.TotalSkins}");

                    LowestText.Text =
                        result.LowestSupply == null
                        ? "-"
                        :
                        $"{result.LowestSupply.MarketHashName}\n" +
                        $"Offers: {result.LowestSupply.SellOrderCount}";

                    HighestText.Text =
                        result.HighestSupply == null
                        ? "-"
                        :
                        $"{result.HighestSupply.MarketHashName}\n" +
                        $"Offers: {result.HighestSupply.SellOrderCount}";
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                        "INDEX LOOP ERROR:");

                    Debug.WriteLine(ex);
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(10));
            }
        }


        private async void DownloadHtmlButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            string bucketId =
                BucketIdText.Text.Trim();

            if (string.IsNullOrWhiteSpace(bucketId))
            {
                DebugText.Text =
                    "Bucket ID is empty.";

                return;
            }

            try
            {
                string url =
                    $"{SteamMarketConstants.MarketBaseUrl}/730/" +
                    $"{bucketId}" +
                    $"?appid={SteamMarketConstants.AppId}";

                DebugText.Text =
                    $"Downloading:\n{url}\n";

                string html =
                    await client.GetMarketPageAsync(url);

                Debug.WriteLine(
                    $"HTML size: {html.Length:N0}");

                bool containsListings =
                    html.Contains("\\\\\\\"listings\\\\\\\":[");

                bool containsListingId =
                    html.Contains("\\\\\\\"listingid\\\\\\\":\\\\\\\"");

                DebugText.Text +=
                    $"\nHTML size: {html.Length:N0}" +
                    $"\nContains listings marker: {containsListings}" +
                    $"\nContains listingid: {containsListingId}";

                string path =
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.Desktop),
                        $"steam_debug_{bucketId}.html");

                await File.WriteAllTextAsync(
                    path,
                    html);

                DebugText.Text +=
                    $"\n\nSaved:\n{path}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                DebugText.Text =
                    ex.ToString();
            }
        }


        private async void ScanButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            DebugText.Text =
                "L2 full bucket scan started...";

            string resultPath =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.Desktop),
                    "steam_l2_results.txt");

            try
            {
                await database.InitializeAsync();
                await listingDatabase.InitializeAsync();

                using StreamWriter writer =
                    new(resultPath);

                Progress<MarketListingData> progress =
                    new(listing =>
                    {
                        string output =
                            "\n\nFOUND COMBO!" +
                            $"\nListingId: {listing.ListingId}" +
                            $"\nPaintSeed: {listing.PaintSeed}" +
                            $"\nWear: {listing.WearValue}" +
                            $"\nPrice: {listing.Price}" +
                            $"\n{listing.InspectLink}";

                        DebugText.Text += output;

                        writer.WriteLine(output);
                        writer.Flush();
                    });

                await scanner.ScanAllBucketsAsync(
                    progress);

                DebugText.Text +=
                    "\n\nSCAN FINISHED.";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                DebugText.Text =
                    ex.ToString();
            }
        }


        private async void L3ScanButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            L3DebugText.Text =
                "L3 scan started...";

            try
            {
                await database.InitializeAsync();

                SkinDefinition? largestBucket = await database.GetLargestSkinBucketAsync();

                if (largestBucket == null)
                {
                    L3DebugText.Text +=
                        "No bucket found.";

                    return;
                }

                L3DebugText.Text +=
                    $"Bucket:\n{largestBucket.MarketBucketId}" +
                    $"\n\nMarketHashName:\n{largestBucket.MarketHashName}" +
                    $"\n\nStored offers:\n{largestBucket.SellOrderCount}" +
                    "\n\nSending request...";

                var result = await advancedScanner.ScanBucketAsync(largestBucket.MarketBucketId);

                L3DebugText.Text +=
                    $"\n\nResponse received." +
                    $"\n\nStart:\n{result.Start}" +
                    $"\nTotalCount:\n{result.TotalCount}" +
                    $"\nListings:\n{result.Listings.Count}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                L3DebugText.Text +=
                    ex.ToString();

                L3DebugText.Text +=
                    "\n\nL3 scan failed.";
            }
        }
    }
}