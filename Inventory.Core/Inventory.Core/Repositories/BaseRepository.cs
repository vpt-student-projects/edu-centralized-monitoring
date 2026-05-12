using System.Data.SQLite;

namespace Inventory.Core.Repositories;

public abstract class BaseRepository
{
    protected readonly string _connectionString;

    protected BaseRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected SQLiteConnection CreateConnection() => new SQLiteConnection(_connectionString);
}