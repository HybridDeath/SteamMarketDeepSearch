using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SteamMarketDeepSearch.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SteamMarketDeepSearch.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly Services.ScannerService _scanner;

        public MainPage()
        {
            InitializeComponent();

            Loaded += MainPage_Loaded;

            _scanner = App.ScannerService;
        }

        private async void MainPage_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            Loaded -= MainPage_Loaded;

            SteamMarketClient client =
                new();

            WeaponCatalogService catalog =
                new();

            SkinIndexerService indexer =
                new(client, catalog);

            await indexer.IndexSkinsAsync();
        }
    }
}
