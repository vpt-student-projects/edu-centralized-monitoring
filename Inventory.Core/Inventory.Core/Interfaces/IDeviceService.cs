using Inventory.Core.Models;

namespace Inventory.Core.Interfaces;

public interface IDeviceService
{
    Task<Device> GetDeviceByIdAsync(int deviceId);
    Task<IEnumerable<Device>> GetDevicesByRoomAsync(int roomId);
    Task MoveDeviceAsync(int deviceId, int newRoomId, int? newPosition = null, string? reason = null);
    Task UpdateDeviceStatusAsync(int deviceId, int newStatusId);
    Task<IEnumerable<Device>> GetDevicesWithOpenTicketsAsync();
}