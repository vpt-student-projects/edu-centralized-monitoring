using Inventory.Core.Interfaces;
using Inventory.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace Inventory.Core.Repositories;

public class MovementRepository : BaseRepository, IMovementRepository
{
    public MovementRepository(string connectionString) : base(connectionString) { }

    public async Task AddMovementAsync(MovementHistory movement)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand(@"
            INSERT INTO MovementHistory (DeviceID, OldRoomID, NewRoomID, MoveDate)
            VALUES (@device, @old, @new, @date)", conn);
        cmd.Parameters.AddWithValue("@device", movement.DeviceID);
        cmd.Parameters.AddWithValue("@old", movement.OldRoomID);
        cmd.Parameters.AddWithValue("@new", movement.NewRoomID);
        cmd.Parameters.AddWithValue("@date", movement.MoveDate);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<MovementHistory>> GetByDeviceAsync(int deviceId)
    {
        var movements = new List<MovementHistory>();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand(
            "SELECT * FROM MovementHistory WHERE DeviceID = @deviceId ORDER BY MoveDate DESC", conn);
        cmd.Parameters.AddWithValue("@deviceId", deviceId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var movement = new MovementHistory
            {
                HistoryID = Convert.ToInt32(reader["HistoryID"]),
                DeviceID = Convert.ToInt32(reader["DeviceID"]),
                OldRoomID = Convert.ToInt32(reader["OldRoomID"]),
                NewRoomID = Convert.ToInt32(reader["NewRoomID"]),
                MoveDate = Convert.ToDateTime(reader["MoveDate"]),
                Reason = null
            };

            // Безопасное чтение необязательного столбца Reason
            try
            {
                int reasonOrd = reader.GetOrdinal("Reason");
                if (!reader.IsDBNull(reasonOrd))
                    movement.Reason = reader.GetString(reasonOrd);
            }
            catch (IndexOutOfRangeException) { }

            movements.Add(movement);
        }
        return movements;
    }
}