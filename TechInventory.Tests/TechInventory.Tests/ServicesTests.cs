using Moq;
using Inventory.Core.Interfaces;
using Inventory.Core.Models;
using Inventory.Core.Services;
using Inventory.Core.Common.Exceptions;
using Xunit;

namespace TechInventory.Tests
{
    public class ServicesTests
    {
        [Fact]
        public async Task MoveDeviceAsync_ValidMove_CreatesMovementAndUpdatesDevice()
        {
            var deviceRepo = new Mock<IDeviceRepository>();
            var moveRepo = new Mock<IMovementRepository>();
            var device = new Device { DeviceID = 1, CurrentRoomID = 1 };
            deviceRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(device);
            var service = new DeviceService(deviceRepo.Object, moveRepo.Object);

            await service.MoveDeviceAsync(1, 2, 3, "Test reason");

            moveRepo.Verify(m => m.AddMovementAsync(It.Is<MovementHistory>(h =>
                h.DeviceID == 1 && h.OldRoomID == 1 && h.NewRoomID == 2 && h.Reason == "Test reason")), Times.Once);
            deviceRepo.Verify(d => d.UpdateAsync(It.Is<Device>(dev =>
                dev.CurrentRoomID == 2 && dev.PositionInRoom == 3)), Times.Once);
        }

        [Fact]
        public async Task MoveDeviceAsync_SameRoom_ThrowsException()
        {
            var deviceRepo = new Mock<IDeviceRepository>();
            deviceRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Device { DeviceID = 1, CurrentRoomID = 1 });
            var service = new DeviceService(deviceRepo.Object, new Mock<IMovementRepository>().Object);
            await Assert.ThrowsAsync<BusinessException>(() => service.MoveDeviceAsync(1, 1));
        }

        [Fact]
        public async Task CreateTicketAsync_Valid_CreatesAndUpdatesStatus()
        {
            var ticketRepo = new Mock<ITicketRepository>();
            var devService = new Mock<IDeviceService>();
            devService.Setup(d => d.GetDeviceByIdAsync(1)).ReturnsAsync(new Device { DeviceID = 1, CurrentRoomID = 2 });
            var service = new TicketService(ticketRepo.Object, devService.Object);

            var ticket = await service.CreateTicketAsync(1, "Проблема", 1);

            Assert.NotNull(ticket);
            Assert.Equal(1, ticket.DeviceID);
            Assert.Equal("Проблема", ticket.Description);
            Assert.Equal(1, ticket.Priority);
            Assert.Equal(TicketService.NewStatusId, ticket.StatusID);
            ticketRepo.Verify(r => r.AddAsync(It.IsAny<Ticket>()), Times.Once);
            devService.Verify(d => d.UpdateDeviceStatusAsync(1, TicketService.BrokenStatusId), Times.Once);
        }

        [Fact]
        public async Task CloseTicketAsync_Valid_ClosesAndUpdates()
        {
            var ticketRepo = new Mock<ITicketRepository>();
            var ticket = new Ticket { TicketID = 10, StatusID = TicketService.NewStatusId };
            ticketRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(ticket);
            var service = new TicketService(ticketRepo.Object, new Mock<IDeviceService>().Object);

            await service.CloseTicketAsync(10, "Решение");

            Assert.Equal(TicketService.ClosedStatusId, ticket.StatusID);
            Assert.NotNull(ticket.ClosedAt);
            Assert.Contains("Решение", ticket.Description);
            ticketRepo.Verify(r => r.UpdateAsync(ticket), Times.Once);
        }
        [Fact]
        public async Task CreateTicketAsync_EmptyDescription_ThrowsBusinessException()
        {
            // Arrange
            var ticketRepo = new Mock<ITicketRepository>();
            var deviceService = new Mock<IDeviceService>();
            var service = new TicketService(ticketRepo.Object, deviceService.Object);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessException>(
                () => service.CreateTicketAsync(1, "   "));
        }

        [Fact]
        public async Task CloseTicketAsync_TicketNotFound_ThrowsBusinessException()
        {
            // Arrange
            var ticketRepo = new Mock<ITicketRepository>();
            var deviceService = new Mock<IDeviceService>();
            ticketRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Ticket?)null);
            var service = new TicketService(ticketRepo.Object, deviceService.Object);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessException>(
                () => service.CloseTicketAsync(999));
        }

        [Fact]
        public async Task MoveDeviceAsync_DeviceNotFound_ThrowsBusinessException()
        {
            // Arrange
            var deviceRepo = new Mock<IDeviceRepository>();
            var movementRepo = new Mock<IMovementRepository>();
            deviceRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Device?)null);
            var service = new DeviceService(deviceRepo.Object, movementRepo.Object);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessException>(
                () => service.MoveDeviceAsync(999, 2));
        }

        [Fact]
        public async Task UpdateDeviceStatusAsync_ValidDevice_ChangesStatus()
        {
            // Arrange
            var deviceRepo = new Mock<IDeviceRepository>();
            var movementRepo = new Mock<IMovementRepository>();
            var device = new Device { DeviceID = 1, StatusID = 3 };
            deviceRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(device);
            var service = new DeviceService(deviceRepo.Object, movementRepo.Object);

            // Act
            await service.UpdateDeviceStatusAsync(1, 4); // 4 = Сломан

            // Assert
            Assert.Equal(4, device.StatusID);
            deviceRepo.Verify(d => d.UpdateAsync(It.Is<Device>(dev => dev.StatusID == 4)), Times.Once);
        }
    }
}