using Inventory.Core.Interfaces;
using Inventory.Core.Models;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace Inventory.Core.Repositories;

public class UserRepository : BaseRepository, IUserRepository
{
    public UserRepository(string connectionString) : base(connectionString) { }

    public async Task<User?> GetByLoginAsync(string login, string passwordHash)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand(
            "SELECT * FROM Users WHERE Login = @login AND PasswordHash = @password", conn);
        cmd.Parameters.AddWithValue("@login", login);
        cmd.Parameters.AddWithValue("@password", passwordHash);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                UserID = reader.GetInt32(0),
                Login = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                FullName = reader.IsDBNull(3) ? null : reader.GetString(3),
                Role = reader.GetString(4)
            };
        }
        return null;
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand("SELECT * FROM Users WHERE UserID = @id", conn);
        cmd.Parameters.AddWithValue("@id", userId);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                UserID = reader.GetInt32(0),
                Login = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                FullName = reader.IsDBNull(3) ? null : reader.GetString(3),
                Role = reader.GetString(4)
            };
        }
        return null;
    }
    public async Task<List<User>> GetAllAsync()
    {
        var users = new List<User>();
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand("SELECT * FROM Users", conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            users.Add(new User
            {
                UserID = reader.GetInt32(0),
                Login = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                FullName = reader.IsDBNull(3) ? null : reader.GetString(3),
                Role = reader.GetString(4)
            });
        }
        return users;
    }
    public async Task AddAsync(User user)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand(
            "INSERT INTO Users (Login, PasswordHash, FullName, Role) VALUES (@login, @pass, @name, @role)", conn);
        cmd.Parameters.AddWithValue("@login", user.Login);
        cmd.Parameters.AddWithValue("@pass", user.PasswordHash);
        cmd.Parameters.AddWithValue("@name", (object?)user.FullName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@role", user.Role);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(User user)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand(
            "UPDATE Users SET Login = @login, FullName = @name, Role = @role WHERE UserID = @id", conn);
        cmd.Parameters.AddWithValue("@login", user.Login);
        cmd.Parameters.AddWithValue("@name", (object?)user.FullName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@role", user.Role);
        cmd.Parameters.AddWithValue("@id", user.UserID);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int userId)
    {
        using var conn = CreateConnection();
        await conn.OpenAsync();
        using var cmd = new SQLiteCommand("DELETE FROM Users WHERE UserID = @id", conn);
        cmd.Parameters.AddWithValue("@id", userId);
        await cmd.ExecuteNonQueryAsync();
    }
}