using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SteamMarketDeepSearch.Models;
using SteamMarketDeepSearch.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SteamMarketDeepSearch.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
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

        private async void IndexButton_Click(object sender, RoutedEventArgs e)
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

                    Debug.WriteLine(
                        ex);
                }


                await Task.Delay(
                    TimeSpan.FromSeconds(10));
            }
        }

        private async void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            DebugText.Text =
                "L2 scan started...";


            try
            {
                (List<MarketListingData> listings, string html) =
                    await scanner.ScanLargestBucketAsync();


                Debug.WriteLine(
                    $"Received listings: {listings.Count}");


                string htmlPath =
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.Desktop),
                        "steam_l2_dump.html");


                await File.WriteAllTextAsync(
                    htmlPath,
                    html);


                Debug.WriteLine(
                    $"HTML saved: {htmlPath}");

                Debug.WriteLine(
                    $"HTML size: {html.Length:N0} chars");


                string resultPath =
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.Desktop),
                        "steam_l2_results.txt");


                using StreamWriter writer =
                    new(resultPath);


                foreach (MarketListingData listing in listings)
                {
                    await writer.WriteLineAsync(
                        $"ListingId: {listing.ListingId}");

                    await writer.WriteLineAsync(
                        $"PaintSeed: {listing.PaintSeed}");

                    await writer.WriteLineAsync(
                        $"Wear: {listing.WearValue}");

                    await writer.WriteLineAsync(
                        $"InspectLink: {listing.InspectLink}");

                    await writer.WriteLineAsync(
                        $"Price: {listing.Price}");

                    await writer.WriteLineAsync();
                }


                DebugText.Text =
                    $"HTML:\n{htmlPath}\n\n" +
                    $"Results:\n{resultPath}\n\n" +
                    $"HTML size: {html.Length:N0}\n" +
                    $"Listings: {listings.Count}";
            }
            catch (Exception ex)
            {
                DebugText.Text =
                    ex.ToString();
            }
        }
    }
}
