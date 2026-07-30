using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SteamMarketDeepSearch.Models;
using SteamMarketDeepSearch.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SteamMarketDeepSearch.Views
{
    public sealed partial class MainPage : Page
    {
        public static readonly SteamMarketClient client =
            new();

        public static readonly WeaponCatalogService catalog =
            new();

        public static readonly SkinIndexerService indexer =
            new(client, catalog);

        public static readonly SkinsDatabaseService database =
            new();

        public static readonly ListingScannerService scanner =
            new(database, client);

        public MainPage()
        {
            InitializeComponent();
        }

        private async void IndexButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Debug.WriteLine(
                "INDEX LOOP STARTED");

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

                using StreamWriter writer =
                    new(resultPath);

                await scanner.ScanAllBucketsAsync(
                    listings =>
                    {
                        foreach (MarketListingData listing in listings)
                        {
                            string output =
                                "\n\nFOUND COMBO!" +
                                $"\nListingId: {listing.ListingId}" +
                                $"\nPaintSeed: {listing.PaintSeed}" +
                                $"\nWear: {listing.WearValue}" +
                                $"\nPrice: {listing.Price}" +
                                $"\n{listing.InspectLink}";

                            writer.WriteLine(output);

                            DispatcherQueue.TryEnqueue(() =>
                            {
                                DebugText.Text += output;
                            });
                        }
                    });

                await writer.FlushAsync();

                DispatcherQueue.TryEnqueue(() =>
                {
                    DebugText.Text +=
                        "\n\nSCAN FINISHED.";
                });
            }
            catch (Exception ex)
            {
                DebugText.Text =
                    ex.ToString();
            }
        }
    }
}