using System;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using Inventory.Core;
using Inventory.Core.Services;
using Xunit;

namespace TechInventory.Tests
{
    public class ViewModelTests : IAsyncLifetime
    {
        private string _connectionString = null!;

        public async Task InitializeAsync()
        {
            _connectionString = await TestDatabase.InitializeAsync();
        }

        public Task DisposeAsync()
        {
            TestDatabase.Cleanup();
            return Task.CompletedTask;
        }

        //Проверяет, что создание заявки через TicketService корректно добавляет запись в базу и меняет статус устройства на "Сломан".
        [Fact]
        public async Task CreateTicket_ShouldAddTicketAndUpdateDeviceStatus()
        {
            // Arrange
            var services = new AppServices(_connectionString);

            // Убедимся, что устройство 1 существует и работает
            var deviceBefore = await services.DeviceService.GetDeviceByIdAsync(1);
            Assert.NotNull(deviceBefore);
            Assert.Equal(3, deviceBefore!.StatusID); // 3 – Работает (по справочнику в тестовой базе)

            // Act
            var ticket = await services.TicketService.CreateTicketAsync(
                deviceId: 1,
                description: "Тестовая заявка",
                priority: 1);

            // Assert
            Assert.NotNull(ticket);
            Assert.Equal("Тестовая заявка", ticket.Description);
            Assert.Equal(1, ticket.Priority);
            Assert.Equal(5, ticket.StatusID); // 5 – Новая

            // Проверяем, что устройство теперь "Сломано"
            var deviceAfter = await services.DeviceService.GetDeviceByIdAsync(1);
            Assert.Equal(4, deviceAfter!.StatusID); // 4 – Сломан
        }

        //Проверяет, что заявки можно получить по идентификатору комнаты.
        [Fact]
        public async Task GetTicketsByRoom_ShouldReturnTicketsForGivenRoom()
        {
            // Arrange
            var services = new AppServices(_connectionString);

            // Вставим две заявки: одна привязана к комнате 1, другая без комнаты
            using (var conn = new SQLiteConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SQLiteCommand(
                    @"INSERT INTO Tickets (DeviceID, RoomID, Description, Priority, StatusID)
                      VALUES (1, 1, 'Заявка в комнате 1', 2, 5),
                             (2, NULL, 'Без комнаты', 3, 6)", conn);
                await cmd.ExecuteNonQueryAsync();
            }

            // Act
            var ticketsInRoom1 = (await services.TicketService.GetTicketsByRoomAsync(1)).ToList();

            // Assert
            Assert.Single(ticketsInRoom1); // только одна заявка с RoomID=1
            Assert.Equal("Заявка в комнате 1", ticketsInRoom1[0].Description);
        }
    }
}