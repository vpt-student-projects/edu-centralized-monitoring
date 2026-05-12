using Inventory.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.Core.Interfaces;

public interface IDeviceRepository
{
    Task<Device?> GetByIdAsync(int deviceId);
    Task<IEnumerable<Device>> GetByRoomAsync(int roomId);
    Task<IEnumerable<Device>> GetDevicesWithOpenTicketsAsync();
    Task UpdateAsync(Device device);
    Task UpdateStatusAsync(int deviceId, int statusId);
    Task<List<Device>> GetAllAsync();               // новый метод
}