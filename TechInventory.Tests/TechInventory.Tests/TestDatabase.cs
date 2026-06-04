using System;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace TechInventory.Tests
{
    public static class TestDatabase
    {
        private static string? _tempDbPath;
        private static string? _connectionString;

        public static async Task<string> InitializeAsync()
        {
            _tempDbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
            _connectionString = $"Data Source={_tempDbPath};Version=3;";

            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new SQLiteCommand(@"
                CREATE TABLE Rooms (
                    RoomID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Floor INTEGER NOT NULL,
                    Building TEXT NOT NULL,
                    Description TEXT
                );
                CREATE TABLE Dictionary (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Category TEXT NOT NULL,
                    Value TEXT NOT NULL
                );
                CREATE TABLE Devices (
                    DeviceID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    TypeID INTEGER NOT NULL,
                    Specs TEXT,
                    StatusID INTEGER NOT NULL,
                    CurrentRoomID INTEGER NOT NULL,
                    PositionInRoom INTEGER,
                    AssignedToUserID INTEGER
                );
                CREATE TABLE Users (
                    UserID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Login TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    FullName TEXT,
                    Role TEXT NOT NULL DEFAULT 'User'
                );
                CREATE TABLE Tickets (
                    TicketID INTEGER PRIMARY KEY AUTOINCREMENT,
                    DeviceID INTEGER NOT NULL,
                    RoomID INTEGER,
                    Description TEXT NOT NULL,
                    Priority INTEGER NOT NULL DEFAULT 3,
                    StatusID INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
                    ClosedAt TEXT
                );
                CREATE TABLE MovementHistory (
                    HistoryID INTEGER PRIMARY KEY AUTOINCREMENT,
                    DeviceID INTEGER NOT NULL,
                    OldRoomID INTEGER NOT NULL,
                    NewRoomID INTEGER NOT NULL,
                    MoveDate TEXT NOT NULL DEFAULT (datetime('now')),
                    Reason TEXT
                );

                INSERT INTO Rooms (RoomID, Name, Floor, Building) VALUES (1, '216', 2, '1'), (2, '217', 2, '1');
                INSERT INTO Dictionary (ID, Category, Value) VALUES 
                    (1, 'DeviceType', 'Системный блок'),
                    (3, 'DeviceStatus', 'Работает'),
                    (4, 'DeviceStatus', 'Сломан'),
                    (5, 'TicketStatus', 'Новая'),
                    (6, 'TicketStatus', 'В работе'),
                    (7, 'TicketStatus', 'Закрыта');
                INSERT INTO Devices (DeviceID, Name, TypeID, Specs, StatusID, CurrentRoomID, PositionInRoom) VALUES 
                    (1, 'gk_216_1', 1, 'Стандартный ПК', 3, 1, 1),
                    (2, 'gk_216_2', 1, 'Стандартный ПК', 4, 1, 2);
                INSERT INTO Users (UserID, Login, PasswordHash, FullName, Role) VALUES 
                    (1, 'admin', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 'Администратор', 'Admin');
            ", conn);
            await cmd.ExecuteNonQueryAsync();

            return _connectionString;
        }

        public static void Cleanup()
        {
            if (_tempDbPath != null && File.Exists(_tempDbPath))
            {
                try { File.Delete(_tempDbPath); } catch { }
            }
        }
    }
}