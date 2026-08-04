using Microsoft.Data.Sqlite;
using SteamMarketDeepSearch.Models;
using System;
using System.Threading.Tasks;

namespace SteamMarketDeepSearch.Services
{
    public class ListingDatabaseService
    {
        private readonly string _connectionString;


        public ListingDatabaseService()
        {
            string path =
                @"C:\Users\Brydżu\source\repos\SteamMarketDeepSearch\SteamMarketDeepSearch\SteamMarketDeepSearch\Data\ListingsDatabase.db";


            _connectionString =
                $"Data Source={path};" +
                $"Default Timeout=10;";
        }


        public SqliteConnection CreateConnection()
        {
            return new SqliteConnection(
                _connectionString);
        }


        public async Task InitializeAsync()
        {
            await using SqliteConnection connection =
                CreateConnection();


            await connection.OpenAsync();


            await using SqliteCommand command =
                connection.CreateCommand();


            command.CommandText =
            """
            PRAGMA journal_mode=WAL;
            PRAGMA busy_timeout=10000;


            CREATE TABLE IF NOT EXISTS BucketScanState
            (
                BucketId TEXT PRIMARY KEY,

                AssetPropertyIndex INTEGER NOT NULL
            );
            """;


            await command.ExecuteNonQueryAsync();
        }



        public async Task<BucketScanState?> GetStateAsync(
            string bucketId)
        {
            await using SqliteConnection connection =
                CreateConnection();


            await connection.OpenAsync();


            await using SqliteCommand command =
                connection.CreateCommand();


            command.CommandText =
            """
            SELECT
                BucketId,
                AssetPropertyIndex
            FROM BucketScanState
            WHERE BucketId = @bucketId;
            """;


            command.Parameters.AddWithValue(
                "@bucketId",
                bucketId);



            await using SqliteDataReader reader =
                await command.ExecuteReaderAsync();


            if (!await reader.ReadAsync())
            {
                return null;
            }


            return new BucketScanState
            {
                BucketId =
                    reader.GetString(0),


                AssetPropertyIndex =
                    reader.GetInt32(1)
            };
        }



        public async Task SaveStateAsync(
            BucketScanState state)
        {
            await using SqliteConnection connection =
                CreateConnection();


            await connection.OpenAsync();


            await using SqliteCommand command =
                connection.CreateCommand();



            command.CommandText =
            """
            INSERT INTO BucketScanState
            (
                BucketId,
                AssetPropertyIndex
            )
            VALUES
            (
                @bucketId,
                @index
            )
            ON CONFLICT(BucketId)
            DO UPDATE SET

                AssetPropertyIndex =
                    excluded.AssetPropertyIndex;
            """;


            command.Parameters.AddWithValue(
                "@bucketId",
                state.BucketId);


            command.Parameters.AddWithValue(
                "@index",
                state.AssetPropertyIndex);



            int attempts = 0;


            while (true)
            {
                try
                {
                    await command.ExecuteNonQueryAsync();

                    return;
                }
                catch (SqliteException ex)
                    when (ex.SqliteErrorCode == 5)
                {
                    attempts++;


                    if (attempts >= 5)
                    {
                        throw;
                    }


                    await Task.Delay(
                        TimeSpan.FromSeconds(attempts));
                }
            }
        }



        public async Task<BucketScanState?> GetFirstIncompleteStateAsync(
            int maxIndex)
        {
            await using SqliteConnection connection =
                CreateConnection();


            await connection.OpenAsync();


            await using SqliteCommand command =
                connection.CreateCommand();



            command.CommandText =
            """
            SELECT
                BucketId,
                AssetPropertyIndex
            FROM BucketScanState
            WHERE AssetPropertyIndex < @maxIndex
            ORDER BY rowid
            LIMIT 1;
            """;


            command.Parameters.AddWithValue(
                "@maxIndex",
                maxIndex);



            await using SqliteDataReader reader =
                await command.ExecuteReaderAsync();



            if (!await reader.ReadAsync())
            {
                return null;
            }



            return new BucketScanState
            {
                BucketId =
                    reader.GetString(0),


                AssetPropertyIndex =
                    reader.GetInt32(1)
            };
        }



        public async Task<bool> IsBucketCompletedAsync(
            string bucketId,
            int maxIndex)
        {
            BucketScanState? state =
                await GetStateAsync(bucketId);


            return state != null &&
                   state.AssetPropertyIndex >= maxIndex;
        }
    }
}