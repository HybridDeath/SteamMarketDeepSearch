using SteamMarketDeepSearch.Clients;
using SteamMarketDeepSearch.Constants;
using SteamMarketDeepSearch.Infrastructure;
using SteamMarketDeepSearch.Models;
using SteamMarketDeepSearch.Parsers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SteamMarketDeepSearch.Services
{
    public class ListingScannerService(
        SkinsDatabaseService database,
        ListingDatabaseService listingDatabase,
        SteamMarketClient client)
    {
        private readonly SkinsDatabaseService _database = database;
        private readonly ListingDatabaseService _listingDatabase = listingDatabase;
        private readonly SteamMarketClient _client = client;

        private const int TotalAssetProperties = 4;

        private DateTime? _firstSuccessfulResponse;
        private DateTime? _lastSuccessfulResponse;
        private int _successfulResponseCount;


        public async Task ScanAllBucketsAsync(
            IProgress<MarketListingData> progress)
        {
            List<SkinDefinition> buckets =
                await _database.GetAllBucketsDescAsync();


            while (true)
            {
                int bucketIndex = -1;


                for (int i = 0; i < buckets.Count; i++)
                {
                    bool completed =
                        await _listingDatabase.IsBucketCompletedAsync(
                            buckets[i].MarketBucketId,
                            TotalAssetProperties);


                    if (!completed)
                    {
                        bucketIndex = i;
                        break;
                    }
                }


                if (bucketIndex < 0)
                {
                    Debug.WriteLine(
                        $"[{DateTime.UtcNow:O}] [SCAN COMPLETED] No buckets remaining.");

                    return;
                }


                SkinDefinition bucket =
                    buckets[bucketIndex];


                bool completedBucket =
                    await ScanBucketAsync(
                        bucket,
                        progress);


                if (completedBucket)
                {
                    await Task.Delay(
                        GlobalThrottling.GetMarketScanDelay());
                }
            }
        }

        private async Task<bool> ScanBucketAsync(
            SkinDefinition skin,
            IProgress<MarketListingData> progress)
        {
            List<string> assetProperties =
            [
                .. AssetPropertyBuilder.BuildAssetProperties()
            ];

            BucketScanState? state =
                await _listingDatabase.GetStateAsync(
                    skin.MarketBucketId);

            int i =
                state?.AssetPropertyIndex ?? 0;


            Debug.WriteLine(
                $"[{DateTime.UtcNow:O}] [SCAN]");

            Debug.WriteLine(
                $"Bucket: {skin.MarketBucketId}");

            Debug.WriteLine(
                $"Starting AssetProperty: {i + 1}/{assetProperties.Count}");


            while (i < assetProperties.Count)
            {
                string assetProperty =
                    assetProperties[i];


                Debug.WriteLine(
                    $"AssetProperty: {i + 1}/{assetProperties.Count}");


                string url =
                    $"{SteamMarketConstants.MarketBaseUrl}/730/" +
                    $"{skin.MarketBucketId}" +
                    $"?appid={SteamMarketConstants.AppId}&" +
                    assetProperty +
                    $"&_={DateTime.UtcNow.Ticks}";


                Debug.WriteLine(
                    "URL generated: YES");


                string html =
                    await _client.GetMarketPageAsync(url);


                Debug.WriteLine(
                    $"HTML size: {html.Length:N0}");


                if (html.Length < 300000)
                {
                    Debug.WriteLine(
                        $"[{DateTime.UtcNow:O}] [SCANNING STOPPED: RATE-LIMIT]");


                    if (_firstSuccessfulResponse is DateTime first &&
                        _lastSuccessfulResponse is DateTime last)
                    {
                        TimeSpan totalTime =
                            last - first;


                        double suggestedDelay =
                            totalTime.TotalMilliseconds /
                            _successfulResponseCount;


                        Debug.WriteLine(
                            $"First successful response at: {first:O}");

                        Debug.WriteLine(
                            $"Last successful response at: {last:O}");

                        Debug.WriteLine(
                            $"Successful responses: {_successfulResponseCount}");

                        Debug.WriteLine(
                            $"Total time: {totalTime}");

                        Debug.WriteLine(
                            $"Suggested delay: ~{Math.Ceiling(suggestedDelay):N0} ms");


                        _firstSuccessfulResponse = null;
                        _lastSuccessfulResponse = null;
                        _successfulResponseCount = 0;
                    }
                    else
                    {
                        Debug.WriteLine(
                            "No successful responses recorded.");
                    }


                    await _listingDatabase.SaveStateAsync(
                        new BucketScanState
                        {
                            BucketId =
                                skin.MarketBucketId,

                            AssetPropertyIndex =
                                i
                        });


                    Debug.WriteLine(
                        "======= [THREAD NOW PAUSED] =======");


                    await Task.Delay(TimeSpan.FromHours(1));


                    continue;
                }


                DateTime responseTime =
                    DateTime.UtcNow;


                if (_firstSuccessfulResponse == null)
                {
                    _firstSuccessfulResponse =
                        responseTime;
                }


                _lastSuccessfulResponse =
                    responseTime;


                _successfulResponseCount++;


                bool containsListings =
                    html.Contains("\\\\\\\"listingid\\\\\\\"");


                Debug.WriteLine(
                    $"Contains listingid: {(containsListings ? "YES" : "NO")}");


                if (containsListings)
                {
                    List<MarketListingData> listings =
                        SteamListingParser.Parse(
                            html,
                            skin.MarketBucketId);


                    Debug.WriteLine(
                        $"Parser result: {listings.Count} listings");


                    foreach (MarketListingData listing in listings)
                    {
                        progress.Report(
                            listing);
                    }
                }
                else
                {
                    Debug.WriteLine(
                        "Parser result: 0 listings");
                }


                await _listingDatabase.SaveStateAsync(
                    new BucketScanState
                    {
                        BucketId =
                            skin.MarketBucketId,

                        AssetPropertyIndex =
                            i + 1
                    });


                Debug.WriteLine(
                    $"Database state updated: {i + 1}/{assetProperties.Count}");


                i++;


                await Task.Delay(
                    GlobalThrottling.GetMarketScanDelay());
            }


            Debug.WriteLine(
                $"[{DateTime.UtcNow:O}] [SCAN COMPLETED]");

            Debug.WriteLine(
                $"Bucket: {skin.MarketBucketId}");


            return true;
        }
    }
}