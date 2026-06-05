using System;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace Inventory.Core;

public static class DatabaseMigrator
{
    public static async Task MigrateAsync(string connectionString)
    {
        using var conn = new SQLiteConnection(connectionString);
        await conn.OpenAsync();

        await EnsureColumnAsync(conn, "Tickets", "ClosedAt", "DATETIME");
        await EnsureColumnAsync(conn, "MovementHistory", "Reason", "TEXT");

        Console.WriteLine("✓ Миграция БД выполнена");
    }

    private static async Task EnsureColumnAsync(SQLiteConnection conn, string table, string column, string type)
    {
        using var checkCmd = new SQLiteCommand($"PRAGMA table_info({table})", conn);
        using var reader = await checkCmd.ExecuteReaderAsync();

        bool exists = false;
        while (await reader.ReadAsync())
        {
            if (reader["name"].ToString() == column)
            {
                exists = true;
                break;
            }
        }

        if (!exists)
        {
            using var alterCmd = new SQLiteCommand(
                $"ALTER TABLE {table} ADD COLUMN {column} {type}", conn);
            await alterCmd.ExecuteNonQueryAsync();
            Console.WriteLine($"✓ Добавлена колонка {table}.{column}");
        }
    }
}