using Inventory.Core.Interfaces;
using Inventory.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace Inventory.Core.Repositories;

public class TicketRepository : BaseRepository, ITicketRepository
{
    public TicketRepository(string connectionString) : base(connectionString) { }

    public async Task AddAsync(Ticket ticket)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand(@"
            INSERT INTO Tickets (DeviceID, RoomID, Description, Priority, StatusID, CreatedAt)
            VALUES (@deviceId, @roomId, @desc, @priority, @statusId, @created)", conn);

        cmd.Parameters.AddWithValue("@deviceId", ticket.DeviceID);
        cmd.Parameters.AddWithValue("@roomId", ticket.RoomID.HasValue ? (object)ticket.RoomID.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@desc", ticket.Description);
        cmd.Parameters.AddWithValue("@priority", ticket.Priority);
        cmd.Parameters.AddWithValue("@statusId", ticket.StatusID);
        cmd.Parameters.AddWithValue("@created", ticket.CreatedAt);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<Ticket?> GetByIdAsync(int ticketId)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand("SELECT * FROM Tickets WHERE TicketID = @id", conn);
        cmd.Parameters.AddWithValue("@id", ticketId);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return MapTicket((SQLiteDataReader)reader);
        return null;
    }

    public async Task<IEnumerable<Ticket>> GetOpenTicketsAsync()
    {
        var tickets = new List<Ticket>();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand(
            "SELECT * FROM Tickets WHERE StatusID != @closedStatus", conn);
        cmd.Parameters.AddWithValue("@closedStatus", 7);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            tickets.Add(MapTicket((SQLiteDataReader)reader));
        return tickets;
    }

    public async Task<IEnumerable<Ticket>> GetTicketsByRoomAsync(int roomId)
    {
        var tickets = new List<Ticket>();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand(
            "SELECT * FROM Tickets WHERE RoomID = @roomId", conn);
        cmd.Parameters.AddWithValue("@roomId", roomId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            tickets.Add(MapTicket((SQLiteDataReader)reader));
        return tickets;
    }

    public async Task UpdateAsync(Ticket ticket)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand(@"
            UPDATE Tickets 
            SET StatusID = @statusId, ClosedAt = @closedAt, Description = @description
            WHERE TicketID = @id", conn);

        cmd.Parameters.AddWithValue("@statusId", ticket.StatusID);
        cmd.Parameters.AddWithValue("@closedAt", ticket.ClosedAt.HasValue ? (object)ticket.ClosedAt.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@description", ticket.Description);
        cmd.Parameters.AddWithValue("@id", ticket.TicketID);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<Ticket>> GetByDeviceAsync(int deviceId)
    {
        var tickets = new List<Ticket>();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand(
            "SELECT * FROM Tickets WHERE DeviceID = @deviceId ORDER BY CreatedAt DESC", conn);
        cmd.Parameters.AddWithValue("@deviceId", deviceId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            tickets.Add(MapTicket((SQLiteDataReader)reader));
        return tickets;
    }

    public async Task<IEnumerable<Ticket>> GetAllTicketsAsync()
    {
        var tickets = new List<Ticket>();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand("SELECT * FROM Tickets ORDER BY CreatedAt DESC", conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            tickets.Add(MapTicket((SQLiteDataReader)reader));
        return tickets;
    }

    private Ticket MapTicket(SQLiteDataReader reader)
    {
        var ticket = new Ticket
        {
            TicketID = Convert.ToInt32(reader["TicketID"]),
            DeviceID = Convert.ToInt32(reader["DeviceID"]),
            RoomID = reader["RoomID"] == DBNull.Value ? null : Convert.ToInt32(reader["RoomID"]),
            Description = reader["Description"] == DBNull.Value ? string.Empty : reader["Description"].ToString(),
            Priority = reader["Priority"] == DBNull.Value ? 3 : Convert.ToInt32(reader["Priority"]),
            StatusID = reader["StatusID"] == DBNull.Value ? 1 : Convert.ToInt32(reader["StatusID"]),
            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
        };

        // Безопасное чтение необязательного столбца ClosedAt
        try
        {
            int closedOrd = reader.GetOrdinal("ClosedAt");
            if (!reader.IsDBNull(closedOrd))
                ticket.ClosedAt = reader.GetDateTime(closedOrd);
        }
        catch (IndexOutOfRangeException) { }

        return ticket;
    }
}