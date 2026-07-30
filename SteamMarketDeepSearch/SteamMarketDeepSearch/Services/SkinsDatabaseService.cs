using Microsoft.Data.Sqlite;
using SteamMarketDeepSearch.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SteamMarketDeepSearch.Services
{
    public class SkinsDatabaseService
    {
        private readonly string _connectionString;

        public SkinsDatabaseService()
        {
            string path =
                @"C:\Users\Brydżu\source\repos\SteamMarketDeepSearch\SteamMarketDeepSearch\SteamMarketDeepSearch\Data\SkinsDatabase.db";

            _connectionString =
                $"Data Source={path}";
        }


        public SqliteConnection CreateConnection()
        {
            return new SqliteConnection(_connectionString);
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
            CREATE TABLE IF NOT EXISTS Skins
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,

                MarketHashName TEXT NOT NULL,

                WeaponType INTEGER NOT NULL,

                MarketBucketId TEXT NOT NULL UNIQUE,

                SellOrderCount INTEGER NOT NULL,

                CreatedAt TEXT NOT NULL,

                LastUpdatedAt TEXT NOT NULL
            );


            CREATE INDEX IF NOT EXISTS IX_Skins_MarketBucketId
            ON Skins(MarketBucketId);
            """;

            await command.ExecuteNonQueryAsync();
        }


        public async Task UpsertSkinsAsync(
            IEnumerable<SkinDefinition> skins)
        {
            await using SqliteConnection connection =
                CreateConnection();

            await connection.OpenAsync();

            await using SqliteTransaction transaction =
                (SqliteTransaction)
                await connection.BeginTransactionAsync();

            foreach (SkinDefinition skin in skins)
            {
                await using SqliteCommand command =
                    connection.CreateCommand();

                command.Transaction = transaction;

                command.CommandText =
                """
                INSERT INTO Skins
                (
                    MarketHashName,
                    WeaponType,
                    MarketBucketId,
                    SellOrderCount,
                    CreatedAt,
                    LastUpdatedAt
                )
                VALUES
                (
                    @name,
                    @weapon,
                    @bucket,
                    @orders,
                    @created,
                    @updated
                )
                ON CONFLICT(MarketBucketId)
                DO UPDATE SET

                    MarketHashName =
                        excluded.MarketHashName,

                    WeaponType =
                        excluded.WeaponType,

                    SellOrderCount =
                        excluded.SellOrderCount,

                    LastUpdatedAt =
                        excluded.LastUpdatedAt;
                """;

                command.Parameters.AddWithValue(
                    "@name",
                    skin.MarketHashName);

                command.Parameters.AddWithValue(
                    "@weapon",
                    (int)skin.WeaponType);

                command.Parameters.AddWithValue(
                    "@bucket",
                    skin.MarketBucketId);

                command.Parameters.AddWithValue(
                    "@orders",
                    skin.SellOrderCount);

                command.Parameters.AddWithValue(
                    "@created",
                    skin.CreatedAt
                        .ToUniversalTime()
                        .ToString("O"));

                command.Parameters.AddWithValue(
                    "@updated",
                    skin.LastUpdatedAt
                        .ToUniversalTime()
                        .ToString("O"));

                await command.ExecuteNonQueryAsync();
            }


            await transaction.CommitAsync();
        }

        public async Task<SkinDefinition?> GetLargestSkinBucketAsync()
        {
            await using SqliteConnection connection =
                CreateConnection();

            await connection.OpenAsync();

            await using SqliteCommand command =
                connection.CreateCommand();


            command.CommandText =
            """
            SELECT
                *
            FROM Skins
            ORDER BY SellOrderCount DESC
            LIMIT 1;
            """;


            await using SqliteDataReader reader =
                await command.ExecuteReaderAsync();


            if (!await reader.ReadAsync())
            {
                return null;
            }


            return new SkinDefinition
            {
                Id =
                    reader.GetInt64(0),

                MarketHashName =
                    reader.GetString(1),

                WeaponType =
                    (Enums.WeaponType)
                    reader.GetInt32(2),

                MarketBucketId =
                    reader.GetString(3),

                SellOrderCount =
                    reader.GetInt32(4),

                CreatedAt =
                    DateTime.Parse(
                        reader.GetString(5)),

                LastUpdatedAt =
                    DateTime.Parse(
                        reader.GetString(6))
            };
        }

        public async Task<List<SkinDefinition>> GetAllSkinsAsync()
        {
            List<SkinDefinition> skins = [];


            await using SqliteConnection connection =
                CreateConnection();

            await connection.OpenAsync();

            await using SqliteCommand command =
                connection.CreateCommand();

            command.CommandText =
            """
            SELECT
                *
            FROM Skins;
            """;

            await using SqliteDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                skins.Add(
                    new SkinDefinition
                    {
                        Id =
                            reader.GetInt64(0),

                        MarketHashName =
                            reader.GetString(1),

                        WeaponType =
                            (Enums.WeaponType)
                            reader.GetInt32(2),

                        MarketBucketId =
                            reader.GetString(3),

                        SellOrderCount =
                            reader.GetInt32(4),

                        CreatedAt =
                            DateTime.Parse(
                                reader.GetString(5)),

                        LastUpdatedAt =
                            DateTime.Parse(
                                reader.GetString(6))
                    });
            }

            return skins;
        }
    }
}