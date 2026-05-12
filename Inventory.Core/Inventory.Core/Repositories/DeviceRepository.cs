using Inventory.Core.Interfaces;
using Inventory.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace Inventory.Core.Repositories;

public class DeviceRepository : BaseRepository, IDeviceRepository
{
    public DeviceRepository(string connectionString) : base(connectionString) { }

    public async Task<Device?> GetByIdAsync(int deviceId)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand("SELECT * FROM Devices WHERE DeviceID = @id", conn);
        cmd.Parameters.AddWithValue("@id", deviceId);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapDevice((SQLiteDataReader)reader);
        }
        return null;
    }

    public async Task<IEnumerable<Device>> GetByRoomAsync(int roomId)
    {
        var devices = new List<Device>();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand(
            "SELECT * FROM Devices WHERE CurrentRoomID = @roomId ORDER BY PositionInRoom", conn);
        cmd.Parameters.AddWithValue("@roomId", roomId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            devices.Add(MapDevice((SQLiteDataReader)reader));

        return devices;
    }

    public async Task UpdateAsync(Device device)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand(
            @"UPDATE Devices SET CurrentRoomID = @roomId, PositionInRoom = @position, StatusID = @statusId 
              WHERE DeviceID = @id", conn);
        cmd.Parameters.AddWithValue("@roomId", device.CurrentRoomID);
        cmd.Parameters.AddWithValue("@position", (object?)device.PositionInRoom ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@statusId", device.StatusID);
        cmd.Parameters.AddWithValue("@id", device.DeviceID);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateStatusAsync(int deviceId, int statusId)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand("UPDATE Devices SET StatusID = @status WHERE DeviceID = @id", conn);
        cmd.Parameters.AddWithValue("@status", statusId);
        cmd.Parameters.AddWithValue("@id", deviceId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<Device>> GetDevicesWithOpenTicketsAsync()
    {
        var devices = new List<Device>();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand(
            @"SELECT DISTINCT d.* FROM Devices d 
              INNER JOIN Tickets t ON d.DeviceID = t.DeviceID 
              WHERE t.StatusID != 7", conn);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            devices.Add(MapDevice((SQLiteDataReader)reader));

        return devices;
    }

    public async Task<List<Device>> GetAllAsync()
    {
        var devices = new List<Device>();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand("SELECT * FROM Devices ORDER BY Name", conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            devices.Add(MapDevice((SQLiteDataReader)reader));
        return devices;
    }

    private Device MapDevice(SQLiteDataReader reader)
    {
        return new Device
        {
            DeviceID = reader.GetInt32(0),
            Name = reader.GetString(1),
            TypeID = reader.GetInt32(2),
            Specs = reader.IsDBNull(3) ? null : reader.GetString(3),
            StatusID = reader.GetInt32(4),
            CurrentRoomID = reader.GetInt32(5),
            PositionInRoom = reader.IsDBNull(6) ? null : reader.GetInt32(6),
            AssignedToUserID = reader.IsDBNull(7) ? null : reader.GetInt32(7)
        };
    }
}