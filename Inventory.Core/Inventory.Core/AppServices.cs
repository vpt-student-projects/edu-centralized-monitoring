using Inventory.Core.Interfaces;
using Inventory.Core.Repositories;
using Inventory.Core.Services;
using System;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace Inventory.Core;

public class AppServices
{
    public IDeviceService DeviceService { get; }
    public ITicketService TicketService { get; }
    public IRoomRepository RoomRepository { get; }
    public IDeviceRepository DeviceRepository { get; }
    public ITicketRepository TicketRepository { get; }
    public IDictionaryRepository DictionaryRepository { get; }
    public IMovementRepository MovementRepository { get; }
    public IUserRepository UserRepository { get; }

    private readonly string _connectionString;

    public AppServices(string connectionString)
    {
        _connectionString = connectionString;

        DeviceRepository = new DeviceRepository(connectionString);
        TicketRepository = new TicketRepository(connectionString);
        var movementRepository = new MovementRepository(connectionString);
        RoomRepository = new RoomRepository(connectionString);
        DictionaryRepository = new DictionaryRepository(connectionString);
        UserRepository = new UserRepository(connectionString);

        MovementRepository = movementRepository; // <-- исправлено

        DeviceService = new DeviceService(DeviceRepository, movementRepository);
        TicketService = new TicketService(TicketRepository, DeviceService);
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using var conn = new SQLiteConnection(_connectionString);
            await conn.OpenAsync();
            await conn.CloseAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка подключения к БД: {ex.Message}");
            return false;
        }
    }

    public static async Task<AppServices> InitializeAsync(string connectionString)
    {
        var services = new AppServices(connectionString);
        bool isConnected = await services.TestConnectionAsync();
        if (!isConnected)
            throw new InvalidOperationException("Не удалось подключиться к базе данных inventory.db");
        Console.WriteLine("✓ Успешное подключение к базе данных");
        return services;
    }
}