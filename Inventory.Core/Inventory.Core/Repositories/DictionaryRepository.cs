using Inventory.Core.Interfaces;
using Inventory.Core.Models;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace Inventory.Core.Repositories;

public class DictionaryRepository : BaseRepository, IDictionaryRepository
{
    public DictionaryRepository(string connectionString) : base(connectionString) { }

    public async Task<List<DictionaryItem>> GetByCategoryAsync(string category)
    {
        var items = new List<DictionaryItem>();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand("SELECT ID, Category, Value FROM Dictionary WHERE Category = @cat", conn);
        cmd.Parameters.AddWithValue("@cat", category);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new DictionaryItem
            {
                ID = reader.GetInt32(0),
                Category = reader.GetString(1),
                Value = reader.GetString(2)
            });
        }
        return items;
    }

    public async Task<DictionaryItem?> GetByIdAsync(int id)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand("SELECT ID, Category, Value FROM Dictionary WHERE ID = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new DictionaryItem
            {
                ID = reader.GetInt32(0),
                Category = reader.GetString(1),
                Value = reader.GetString(2)
            };
        }
        return null;
    }
}