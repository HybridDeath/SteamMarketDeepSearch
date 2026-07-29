using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SteamMarketDeepSearch.Models;
using SteamMarketDeepSearch.Services;
using System.Diagnostics;

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

        public MainPage()
        {
            InitializeComponent();
        }

        private async void IndexButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("INDEX BUTTON CLICKED");

            SkinIndexResult result =
                await indexer.IndexSkinsAsync();


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

            DebugText.Text = $"Total Skins Indexed: {result.TotalSkins}";
        }
    }
}
