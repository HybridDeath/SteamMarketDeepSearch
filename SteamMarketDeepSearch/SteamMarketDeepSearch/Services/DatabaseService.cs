using Microsoft.Data.Sqlite;
using SteamMarketDeepSearch.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SteamMarketDeepSearch.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
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
            await using SqliteConnection connection = CreateConnection();

            await connection.OpenAsync();

            await using SqliteCommand command = connection.CreateCommand();

            command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS Skins
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,

                MarketHashName TEXT NOT NULL,

                WeaponType INTEGER NOT NULL,

                MarketListingId TEXT NOT NULL UNIQUE,

                CreatedAt TEXT NOT NULL
            );
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
                (SqliteTransaction)await connection.BeginTransactionAsync();

            foreach (SkinDefinition skin in skins)
            {
                await using SqliteCommand command =
                    connection.CreateCommand();

                command.Transaction = transaction;

                command.CommandText =
                """
                INSERT OR IGNORE INTO Skins
                (
                    MarketHashName,
                    WeaponType,
                    MarketListingId,
                    CreatedAt
                )
                VALUES
                (
                    @name,
                    @weapon,
                    @listing,
                    @created
                );
                """;

                command.Parameters.AddWithValue(
                    "@name",
                    skin.MarketHashName);

                command.Parameters.AddWithValue(
                    "@weapon",
                    (int)skin.WeaponType);

                command.Parameters.AddWithValue(
                    "@listing",
                    skin.MarketListingId);

                command.Parameters.AddWithValue(
                    "@created",
                    skin.CreatedAt.ToUniversalTime().ToString("O"));

                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
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
                "SELECT * FROM Skins ORDER BY MarketHashName;";

            await using SqliteDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                skins.Add(
                    new SkinDefinition
                    {
                        Id = reader.GetInt64(0),
                        MarketHashName = reader.GetString(1),
                        WeaponType = (Enums.WeaponType)reader.GetInt32(2),
                        MarketListingId = reader.GetString(3),
                        CreatedAt = DateTime.Parse(reader.GetString(4))
                    });
            }

            return skins;
        }
    }
}
