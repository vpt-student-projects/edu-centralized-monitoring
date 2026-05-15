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
    public async Task AddAsync(Room room)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand(
            "INSERT INTO Rooms (Name, Floor, Building, Description) VALUES (@name, @floor, @building, @desc)", conn);
        cmd.Parameters.AddWithValue("@name", room.Name);
        cmd.Parameters.AddWithValue("@floor", room.Floor);
        cmd.Parameters.AddWithValue("@building", room.Building);
        cmd.Parameters.AddWithValue("@desc", (object?)room.Description ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(Room room)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand(
            "UPDATE Rooms SET Name = @name, Floor = @floor, Building = @building, Description = @desc WHERE RoomID = @id", conn);
        cmd.Parameters.AddWithValue("@name", room.Name);
        cmd.Parameters.AddWithValue("@floor", room.Floor);
        cmd.Parameters.AddWithValue("@building", room.Building);
        cmd.Parameters.AddWithValue("@desc", (object?)room.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id", room.RoomID);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int roomId)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand("DELETE FROM Rooms WHERE RoomID = @id", conn);
        cmd.Parameters.AddWithValue("@id", roomId);
        await cmd.ExecuteNonQueryAsync();
    }
}