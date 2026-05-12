using Inventory.Core.Interfaces;
using Inventory.Core.Models;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace Inventory.Core.Repositories;

public class RoomRepository : BaseRepository, IRoomRepository
{
    public RoomRepository(string connectionString) : base(connectionString) { }

    public async Task<List<Room>> GetAllAsync()
    {
        var rooms = new List<Room>();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand("SELECT * FROM Rooms ORDER BY Building, Floor, Name", conn);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            rooms.Add(new Room
            {
                RoomID = reader.GetInt32(0),
                Name = reader.GetString(1),
                Floor = reader.GetInt32(2),
                Building = reader.GetString(3),
                Description = reader.IsDBNull(4) ? null : reader.GetString(4)
            });
        }
        return rooms;
    }

    public async Task<Room?> GetByIdAsync(int roomId)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand("SELECT * FROM Rooms WHERE RoomID = @id", conn);
        cmd.Parameters.AddWithValue("@id", roomId);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Room
            {
                RoomID = reader.GetInt32(0),
                Name = reader.GetString(1),
                Floor = reader.GetInt32(2),
                Building = reader.GetString(3),
                Description = reader.IsDBNull(4) ? null : reader.GetString(4)
            };
        }
        return null;
    }
}